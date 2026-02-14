using UnityEngine;

public class DialogueFollowCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float distance = 0.13f;           // Distance in front of camera
    [SerializeField] private float verticalOffset = -1.1f;    // Below eye level for dialogue
    [SerializeField] private float smoothSpeed = 8f;          // Smooth movement
    [SerializeField] private bool followRotation = true;      // Follow camera rotation

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
    private void Update()
    {
        if (mainCamera == null) return;

        // Calculate target position in front of camera
        Vector3 targetPos = mainCamera.transform.position 
            + mainCamera.transform.forward * distance
            + mainCamera.transform.up * verticalOffset;

        // Smooth movement
        transform.position = Vector3.Lerp(
            transform.position, 
            targetPos, 
            Time.deltaTime * smoothSpeed
        );

        // Follow camera rotation (only Y and X, not Z roll)
        if (followRotation)
        {
            Quaternion targetRot = Quaternion.LookRotation(mainCamera.transform.forward);
            transform.rotation = Quaternion.Lerp(
                transform.rotation, 
                targetRot, 
                Time.deltaTime * smoothSpeed
            );
        }
    }

    /// <summary>
    /// Show the dialogue panel
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the dialogue panel
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}