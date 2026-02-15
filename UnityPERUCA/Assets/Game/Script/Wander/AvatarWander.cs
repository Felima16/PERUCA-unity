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
        private const float ARRIVAL_DISTANCE = 1f;
        private const float EDIT_DISTANCE = 0f;
        private const float WANDER_RANGE = 10f;

        [SerializeField] private Transform playerPositionHelper;
        [SerializeField] private IdleState[] idleStates;
        [SerializeField] private MovementState[] movementStates;
        [SerializeField] private bool logChanges = false;

        private Animator animator;
        private NavMeshAgent navMeshAgent;
        private Vector3 startPosition;
        private int totalIdleStateWeight;
        private bool isStarted;

        private float moveSpeed;
        private float turnSpeed;
        private Vector3 wanderTarget;
        private Vector3 directMovementTarget;
        private float idleEndTime;
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
                        HandleBeginIdle();
                    }
                    break;

                case WanderState.Wander:
                    targetPosition = wanderTarget;
                    // FaceDirection((targetPosition - position).normalized);
                    
                    if (HasReachedTarget(targetPosition))
                    {
                        wanderTarget = GenerateRandomWanderTarget();
                    }
                    break;

                case WanderState.DirectMovement:
                    targetPosition = directMovementTarget;
                    // FaceDirection((targetPosition - position).normalized);
                    
                    if (HasReachedTarget(targetPosition))
                    {
                        SetState(WanderState.Idle);
                        moveToPositionComplete?.Invoke();
                    }
                    break;
            }

            // Use NavMeshAgent for movement
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.angularSpeed = Mathf.Max(1f, turnSpeed);
            if (!navMeshAgent.hasPath || Vector3.Distance(navMeshAgent.destination, targetPosition) > 0.1f)
                navMeshAgent.SetDestination(targetPosition);
        }

        private bool HasReachedTarget(Vector3 targetPosition)
        {
            if (navMeshAgent.pathPending)
                return false;
            return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.01f;
        }

        private void FaceDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
                return;

            transform.rotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(Vector3.RotateTowards(transform.forward, direction, turnSpeed * Time.deltaTime * Mathf.Deg2Rad, 0f), Vector3.up),
                Vector3.up
            );
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
            var randomDirection = UnityEngine.Random.insideUnitSphere * WANDER_RANGE;
            var targetPos = startPosition + randomDirection;
            ValidateNavMeshPosition(ref targetPos);
            return targetPos;
        }

        private void ValidateNavMeshPosition(ref Vector3 targetPos)
        {
            if (!navMeshAgent)
                return;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, Mathf.Infinity, NavMesh.AllAreas))
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
        /// When the avatar arrives, it automatically switches to idle animation.
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
                    MoveToPosition(startPosition);
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