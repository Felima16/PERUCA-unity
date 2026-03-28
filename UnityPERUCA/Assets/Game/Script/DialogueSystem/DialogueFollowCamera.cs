using UnityEngine;

public class DialogueFollowCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float distance = 0.4f;             // Distance in front of camera
    [SerializeField] private float verticalOffset = -1.1f;      // Y offset below camera
    [SerializeField] private bool keepUpright = true;            // Remove roll for stable UI
    [SerializeField] private float yawOffset = 8f;               // Extra horizontal rotation in degrees

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void AdjustHeight() {
        // Adjust once based on camera pose when dialogue is shown
        if (mainCamera == null) return;

        Transform cameraTransform = mainCamera.transform;

        // Position in front of user, slightly below camera level
        Vector3 targetPos = cameraTransform.position
            + cameraTransform.forward * distance
            + Vector3.up * verticalOffset;

        transform.position = targetPos;

        // Rotate once to face the user
        Vector3 toCamera = cameraTransform.position - transform.position;
        if (keepUpright)
        {
            toCamera.y = 0f;
        }

        if (toCamera.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
            Quaternion offsetRotation = Quaternion.Euler(0f, yawOffset, 0f);
            transform.rotation = lookRotation * offsetRotation;
        }
    }

    /// <summary>
    /// Show the dialogue panel
    /// </summary>
    public void Show()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // gameObject.SetActive(true);
        AdjustHeight();
    }

    /// <summary>
    /// Hide the dialogue panel
    /// </summary>
    public void Hide()
    {
        // gameObject.SetActive(false);
    }
}