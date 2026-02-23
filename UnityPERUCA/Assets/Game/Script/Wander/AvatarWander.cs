using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

namespace AvatarLab.Wander
{
    [RequireComponent(typeof(Animator))]
    public class AvatarWander : MonoBehaviour
    {
        private const float ARRIVAL_DISTANCE = 0.2f;
        private const float EDIT_DISTANCE = 0f;
        private const float WANDER_RANGE = 30f;

        [SerializeField] private Transform playerPositionHelper;
        [SerializeField] private IdleState[] idleStates;
        [SerializeField] private MovementState[] movementStates;
        [SerializeField] private bool logChanges = false;

        private Animator animator;
        private NavMeshAgent navMeshAgent;
        private Vector3 startPosition;
        private Vector3 editPosition;
        private int totalIdleStateWeight;
        private bool isStarted;

        private float moveSpeed;
        private float turnSpeed;
        private Vector3 wanderTarget;
        private Vector3 directMovementTarget;
        private float idleEndTime;
        private float wanderEndTime;
        private Quaternion targetRotation;
        private bool shouldRotateToTarget;
        private readonly HashSet<string> animatorParameters = new HashSet<string>();

        public enum WanderState { Idle, Wander, DirectMovement }
        public WanderState CurrentState { get; private set; }

        public UnityEngine.Events.UnityEvent idleEvent;
        public UnityEngine.Events.UnityEvent movementEvent;
        public UnityEngine.Events.UnityEvent moveToPositionComplete;
        
        private void Awake()
        {
            AvatarManager.OnAvatarStateChanged += OnAvatarStateChanged;
            animator = GetComponent<Animator>();

            var runtimeController = animator.runtimeAnimatorController;
            if (animator)
                animatorParameters.UnionWith(animator.parameters.Select(p => p.name));

            if (logChanges)
            {
                ValidateSetup(runtimeController);
            }

            foreach (IdleState state in idleStates)
            {
                totalIdleStateWeight += state.stateWeight;
            }

            startPosition = transform.position;
            editPosition = startPosition; // Can be set to a different position if needed
            animator.applyRootMotion = false;
            navMeshAgent = GetComponent<NavMeshAgent>();

            if (navMeshAgent)
            {
                navMeshAgent.stoppingDistance = ARRIVAL_DISTANCE;
            }
            else
            {
                Debug.LogError($"{gameObject.name} does not have a NavMeshAgent component.");
                enabled = false;
            }
        }

        private void ValidateSetup(RuntimeAnimatorController runtimeController)
        {
            if (runtimeController == null)
            {
                Debug.LogError($"{gameObject.name} has no animator controller.");
                enabled = false;
                return;
            }

            if (animator.avatar == null)
            {
                Debug.LogError($"{gameObject.name} has no avatar.");
                enabled = false;
                return;
            }

            if (animator.hasRootMotion)
            {
                Debug.LogWarning($"{gameObject.name} has root motion enabled. Disabling it.");
                animator.applyRootMotion = false;
            }

            if (idleStates.Length == 0 || movementStates.Length == 0)
            {
                Debug.LogError($"{gameObject.name} has no idle or movement states.");
                enabled = false;
                return;
            }

            foreach (var state in idleStates.Concat<AIState>(movementStates))
            {
                if (string.IsNullOrEmpty(state.animationBool))
                {
                    Debug.LogError($"{gameObject.name} has a state with no animation boolean set.");
                    enabled = false;
                    return;
                }

                if (!animatorParameters.Contains(state.animationBool))
                {
                    Debug.LogError($"{gameObject.name} animator does not have parameter '{state.animationBool}'.");
                    enabled = false;
                    return;
                }
            }

            foreach (var state in movementStates)
            {
                if (state.moveSpeed <= 0)
                {
                    Debug.LogError($"{gameObject.name} movement state '{state.stateName}' has invalid speed.");
                    enabled = false;
                    return;
                }

                if (state.turnSpeed <= 0)
                {
                    Debug.LogError($"{gameObject.name} movement state '{state.stateName}' has invalid turn speed.");
                    enabled = false;
                    return;
                }
            }
        }

        private void OnDestroy()
        {
            AvatarManager.OnAvatarStateChanged -= OnAvatarStateChanged;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Start()
        {
            isStarted = true;
            // Begin in wander so the avatar will pick a target and start moving
            SetState(WanderState.Idle);
        }

        private void Update()
        {
            if (!isStarted)
                return;

            ApplyMovement();
        }

        private void ApplyMovement()
        {
            var position = transform.position;
            var targetPosition = position;

            switch (CurrentState)
            {
                case WanderState.Idle:
                    if (Time.time >= idleEndTime)
                    {
                        if(AvatarManager.instance.state == AvatarState.Game)
                            SetState(WanderState.Wander);
                        else
                            HandleBeginIdle();
                    }
                    break;

                case WanderState.Wander:
                    targetPosition = wanderTarget;

                    if (Time.time >= wanderEndTime) {
                        SetState(WanderState.Idle);
                    }
                    else
                    {
                        if (HasReachedTarget()) {
                            wanderTarget = GenerateRandomWanderTarget();
                            targetPosition = wanderTarget;
                        }
                    }
                    break;
                case WanderState.DirectMovement:
                    targetPosition = directMovementTarget;
                    
                    if (HasReachedTarget())
                    {
                        if (shouldRotateToTarget)
                        {
                            if (playerPositionHelper != null)
                            {
                                targetRotation = CalculateFaceTowardsRotation(playerPositionHelper.position);
                            }
                            else
                            {
                                // No player position available; treat current rotation as the desired one.
                                targetRotation = transform.rotation;
                            }
                            shouldRotateToTarget = false; // Ensure we only set the target rotation once upon arrival
                        }
                        
                        if (HasReachedRotation())
                        {
                            SetState(WanderState.Idle);
                            moveToPositionComplete?.Invoke();
                        } 
                        else
                        {
                            RotateTowardsTarget();
                        }
                    }
                    break;
            }

            // Use NavMeshAgent for movement
            bool isRotating = CurrentState == WanderState.DirectMovement && HasReachedTarget();
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = !isRotating;
            navMeshAgent.isStopped = isRotating;
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.angularSpeed = Mathf.Max(1f, turnSpeed);
            if (!isRotating && (!navMeshAgent.hasPath || Vector3.Distance(navMeshAgent.destination, targetPosition) > 0.1f))
                navMeshAgent.SetDestination(targetPosition);
        }

        private bool HasReachedTarget()
        {
            if (navMeshAgent.pathPending)
                return false;
            return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.01f;
        }

        private void RotateTowardsTarget()
        {
            navMeshAgent.isStopped = true;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        private bool HasReachedRotation()
        {
            return Quaternion.Angle(transform.rotation, targetRotation) < 1f;
        }

        /// <summary>
        /// Calculate rotation to face towards a reference position (face to face)
        /// </summary>
        private Quaternion CalculateFaceTowardsRotation(Vector3 referencePosition)
        {
            Vector3 directionTowards = (referencePosition - transform.position).normalized;
            directionTowards.y = 0; // Keep it on XZ plane
            
            if (directionTowards.sqrMagnitude < 0.01f)
            {
                // If positions are too close, use avatar's current forward
                return transform.rotation;
            }
            
            return Quaternion.LookRotation(directionTowards, Vector3.up);
        }

        private void SetState(WanderState newState)
        {
            if (CurrentState == newState)
                return;

            CurrentState = newState;

            switch (CurrentState)
            {
                case WanderState.Idle:
                    HandleBeginIdle();
                    break;
                case WanderState.Wander:
                    HandleBeginWander();
                    break;
                case WanderState.DirectMovement:
                    HandleBeginDirectMovement();
                    break;
            }
        }

        private void HandleBeginIdle()
        {
            ClearAnimatorBools();
            SelectRandomIdleState();
            moveSpeed = 0f;
            turnSpeed = 0f;
            idleEvent?.Invoke();
        }

        private void SelectRandomIdleState()
        {
            var rand = UnityEngine.Random.Range(0, totalIdleStateWeight);
            var currentWeight = 0;

            foreach (var idleState in idleStates)
            {
                currentWeight += idleState.stateWeight;
                if (rand < currentWeight)
                {
                    TrySetBool(idleState.animationBool, true);
                    idleEndTime = Time.time + UnityEngine.Random.Range(idleState.minStateTime, idleState.maxStateTime);
                    return;
                }
            }
        }

        private void HandleBeginWander()
        {
            wanderEndTime = Time.time + movementStates.FirstOrDefault(s => s.stateName == "Walking").maxStateTime;
            wanderTarget = GenerateRandomWanderTarget();
            SetMovementState(GetSlowestMovementState());
            movementEvent?.Invoke();
        }

        private void HandleBeginDirectMovement()
        {
            SetMovementState(GetFastestMovementState());
            movementEvent?.Invoke();
        }

        private Vector3 GenerateRandomWanderTarget()
        {
            // Generate a random direction on the XZ plane
            var randomAngle = UnityEngine.Random.Range(0f, 360f);
            var randomDistance = UnityEngine.Random.Range(WANDER_RANGE * 0.5f, WANDER_RANGE);
            
            var direction = new Vector3(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                0f,
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );
            
            var targetPos = transform.position + direction * randomDistance;
            ValidateNavMeshPosition(ref targetPos);
            return targetPos;
        }

        private void ValidateNavMeshPosition(ref Vector3 targetPos)
        {
            if (!navMeshAgent)
                return;

            // Use WANDER_RANGE as the search radius to keep targets within intended range
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, WANDER_RANGE, NavMesh.AllAreas))
            {
                targetPos = hit.position;
            }
            else
            {
                Debug.LogError("Unable to sample nav mesh. Ensure there's a NavMesh with 'Walkable' area.");
            }
        }

        private void SetMovementState(MovementState state)
        {
            if (state == null)
            {
                Debug.LogError($"{gameObject.name} has no movement states configured.");
                return;
            }

            ClearAnimatorBools();
            TrySetBool(state.animationBool, true);
            moveSpeed = state.moveSpeed;
            turnSpeed = state.turnSpeed;
        }

        private MovementState GetFastestMovementState()
        {
            return movementStates.OrderByDescending(s => s.moveSpeed).FirstOrDefault();
        }

        private MovementState GetSlowestMovementState()
        {
            return movementStates.OrderBy(s => s.moveSpeed).FirstOrDefault();
        }

        private void ClearAnimatorBools()
        {
            foreach (var state in idleStates.Concat<AIState>(movementStates))
            {
                TrySetBool(state.animationBool, false);
            }
        }

        private void TrySetBool(string parameterName, bool value)
        {
            if (!string.IsNullOrEmpty(parameterName) && animatorParameters.Contains(parameterName))
            {
                animator.SetBool(parameterName, value);
            }
        }

        /// <summary>
        /// Moves the avatar to a specific position with running animation.
        /// When the avatar arrives, it automatically switches to idle animation
        /// and rotates to face the target direction.
        /// </summary>
        /// <param name="targetPosition">The world position to move to</param>
        public void MoveToPosition(Vector3 targetPosition)
        {
            if (!isStarted)
            {
                Debug.LogWarning("AvatarWander has not started yet.");
                return;
            }
            directMovementTarget = targetPosition;
            ValidateNavMeshPosition(ref directMovementTarget);
            shouldRotateToTarget = true;
            SetState(WanderState.DirectMovement);
        }

        /// <summary>
        /// Handles avatar state changes from AvatarManager
        /// </summary>
        private void OnAvatarStateChanged(AvatarState newState)
        {
            if (!isStarted)
                return;

            switch (newState)
            {
                case AvatarState.Game:
                    // Free to wander during gameplay
                    SetState(WanderState.Wander);
                    break;

                case AvatarState.Edit:
                    navMeshAgent.stoppingDistance = EDIT_DISTANCE;
                    MoveToPosition(editPosition);
                    break;
                case AvatarState.Help:
                    navMeshAgent.stoppingDistance = ARRIVAL_DISTANCE;
                    MoveToPosition(playerPositionHelper.position);
                    break;
            }
        }

        [ContextMenu("Setup Basic Wander States")]
        public void BasicWanderSetUp()
        {
            MovementState walking = new MovementState { stateName = "Walking", animationBool = "isWalking" };
            MovementState running = new MovementState { stateName = "Running", animationBool = "isRunning" };
            IdleState idle = new IdleState { stateName = "Idle", animationBool = "isIdling" };

            movementStates = new[] { walking, running };
            idleStates = new[] { idle };
        }
    }
}