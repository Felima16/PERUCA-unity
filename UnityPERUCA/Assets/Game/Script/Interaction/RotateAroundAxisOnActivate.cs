using System.Collections;
using UnityEngine;

public class RotateAroundAxisOnActivate : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public enum Mode { Single360, Continuous }

    [Tooltip("Axis to rotate around (local to pivot).")]
    public Axis axis = Axis.Y;

    [Tooltip("Pivot to rotate around. If null, rotation is applied to the target's own transform.")]
    public Transform pivot;

    [Tooltip("Transform that will be rotated. If null this GameObject's transform will be used.")]
    public Transform target;

    [Tooltip("Duration of the 360° rotation in seconds.")]
    public float duration = 1.0f;

    [Tooltip("Use smooth interpolation instead of linear.")]
    public bool smooth = true;

    [Tooltip("Allow the rotation to be triggered again while already rotating.")]
    public bool allowRepeat = false;

    [Tooltip("Rotation mode: single 360° or continuous until stopped.")]
    public Mode mode = Mode.Single360;

    [Tooltip("Degrees per second when in Continuous mode.")]
    public float continuousSpeed = 90f;

    bool m_IsRotating;
    bool m_IsContinuous;
    Coroutine m_ContinuousCoroutine;

    void Reset()
    {
        if (target == null) target = transform;
    }

    void Awake()
    {
        if (target == null) target = transform;
    }

    // No XR interactable required — rotation is triggered via `TriggerRotate()` or context menu.

    // Public method to trigger rotation from a UI Button or other code.
    // Example: assign this method to a Unity UI Button's OnClick in the Inspector.
    public void TriggerRotate()
    {
        if (mode == Mode.Single360)
        {
            if (m_IsRotating && !allowRepeat) return;
            StartCoroutine(Rotate360());
        }
        else
        {
            // Toggle continuous mode on/off
            if (m_IsContinuous) StopContinuous();
            else StartContinuous();
        }
    }

    public void ResetRotation()
    {
        if (target == null) target = transform;
        target.rotation = Quaternion.identity;
    }

    [ContextMenu("Trigger Rotate")]
    void ContextTriggerRotate() => TriggerRotate();

    // Start continuous rotation (callable from code)
    public void StartContinuous()
    {
        if (m_IsContinuous) return;
        m_IsContinuous = true;
        m_ContinuousCoroutine = StartCoroutine(ContinuousRotate());
    }

    // Stop continuous rotation (callable from code)
    public void StopContinuous()
    {
        if (!m_IsContinuous) return;
        m_IsContinuous = false;
        if (m_ContinuousCoroutine != null)
        {
            StopCoroutine(m_ContinuousCoroutine);
            m_ContinuousCoroutine = null;
        }
    }

    // UI Toggle can call this with the toggle's bool value
    public void ToggleContinuous(bool on)
    {
        if (on) StartContinuous(); else StopContinuous();
    }

    IEnumerator Rotate360()
    {
        if (target == null) target = transform;

        m_IsRotating = true;

        Vector3 axisVec = axis == Axis.X ? Vector3.right : (axis == Axis.Y ? Vector3.up : Vector3.forward);

        // If pivot specified and it's a different transform, rotate around pivot point
        if (pivot != null && pivot != target)
        {
            float elapsed = 0f;
            float lastAngle = 0f;
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float k = smooth ? Mathf.SmoothStep(0f, 1f, t) : t;
                float angle = Mathf.Lerp(0f, 360f, k);
                float delta = angle - lastAngle;
                target.RotateAround(pivot.position, pivot.TransformDirection(axisVec), delta);
                lastAngle = angle;
                elapsed += Time.deltaTime;
                yield return null;
            }
            // ensure final rotation completes exactly 360
            target.RotateAround(pivot.position, pivot.TransformDirection(axisVec), 360f - lastAngle);
        }
        else
        {
            // Rotate in-place by adjusting local/world rotation
            Quaternion start = target.rotation;
            Quaternion rotDelta = Quaternion.AngleAxis(360f, target.TransformDirection(axisVec));
            Quaternion end = start * rotDelta;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float k = smooth ? Mathf.SmoothStep(0f, 1f, t) : t;
                target.rotation = Quaternion.Slerp(start, end, k);
                elapsed += Time.deltaTime;
                yield return null;
            }
            target.rotation = end;
        }

        m_IsRotating = false;
    }

    IEnumerator ContinuousRotate()
    {
        if (target == null) target = transform;
        Vector3 axisVec = axis == Axis.X ? Vector3.right : (axis == Axis.Y ? Vector3.up : Vector3.forward);
        while (m_IsContinuous)
        {
            float delta = continuousSpeed * Time.deltaTime;
            if (pivot != null && pivot != target)
            {
                target.RotateAround(pivot.position, pivot.TransformDirection(axisVec), delta);
            }
            else
            {
                target.Rotate(axisVec, delta, Space.Self);
            }
            yield return null;
        }
    }
}
