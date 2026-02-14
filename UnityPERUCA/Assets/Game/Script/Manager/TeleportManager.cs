using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using System.Collections.Generic;

public class TeleportManager: MonoBehaviour
{
    private static TeleportManager instance;
    private static List<TeleportationAnchor> teleportAnchors = new List<TeleportationAnchor>();
    private TeleportationProvider teleportationProvider;

    /// <summary>
    /// Get the singleton instance of TeleportManager
    /// </summary>
    public static TeleportManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<TeleportManager>();
                if (instance == null)
                {
                    Debug.LogError("TeleportManager not found in scene!");
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            FindTeleportationProvider();
            LoadTeleportAnchors();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Find the TeleportationProvider in the scene
    /// </summary>
    private void FindTeleportationProvider()
    {
        teleportationProvider = FindFirstObjectByType<TeleportationProvider>();
        if (teleportationProvider == null)
        {
            Debug.LogWarning("[TeleportManager] TeleportationProvider not found in scene!");
        }
        else
        {
            Debug.Log("[TeleportManager] Found TeleportationProvider");
        }
    }

    /// <summary>
    /// Load all teleport anchors from the scene
    /// </summary>
    private void LoadTeleportAnchors()
    {
        teleportAnchors.Clear();
        var allAnchors = Object.FindObjectsByType<TeleportationAnchor>(FindObjectsSortMode.None);
        Debug.Log($"[TeleportManager] Found {allAnchors.Length} TeleportationAnchor objects");
        
        teleportAnchors.AddRange(allAnchors);
        Debug.Log($"[TeleportManager] Loaded {teleportAnchors.Count} teleport anchors from scene");
        
        if (teleportAnchors.Count == 0)
        {
            Debug.LogWarning("[TeleportManager] No teleport anchors found! Make sure you have TeleportationAnchor components in your scene.");
            return;
        }
        
        foreach (var anchor in teleportAnchors)
        {
            if (anchor == null)
            {
                Debug.LogError("[TeleportManager] Null anchor detected!");
                continue;
            }
            Debug.Log($"  - Anchor: {anchor.gameObject.name} at {anchor.transform.position}");
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            teleportAnchors.Clear();
        }
    }

    /// <summary>
    /// Teleport to a specific anchor by reference
    /// </summary>
    public void TeleportToAnchor(TeleportPlaces anchorPlace)
    {
        if (teleportationProvider == null)
        {
            Debug.LogError("[TeleportManager] TeleportationProvider not found!");
            return;
        }
        if (teleportAnchors.Count == 0)
        {
            Debug.LogError("[TeleportManager] No teleport anchors available!");
            return;
        }

        TeleportationAnchor anchor = teleportAnchors.Find(a => a.gameObject.name == anchorPlace.ToString());
        
        if (anchor == null)
        {            
            Debug.LogError($"[TeleportManager] Teleport anchor '{anchorPlace}' not found!");
            return;
        }

        Debug.Log($"[TeleportManager] Teleporting to anchor: {anchor.gameObject.name}");


        
        // Create a teleportation request using the anchor's position and rotation
        var teleportRequest = new TeleportRequest
        {
            destinationPosition = anchor.transform.position,
            destinationRotation = anchor.transform.rotation
        };
        teleportationProvider.QueueTeleportRequest(teleportRequest);
    }
}

public enum TeleportPlaces
{
    AvatarEdit,
    OrganiseGame
}