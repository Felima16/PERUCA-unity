using UnityEngine;

/// <summary>
/// Generates physical walls around the slide puzzle board to prevent pieces from moving off the board.
/// Attach this script to the board parent object and configure the board properties in the inspector.
/// </summary>
public class BoardWallGenerator : MonoBehaviour
{
    [Header("Board Configuration")]
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private Vector3 boardSize = new Vector3(10f, 0.5f, 10f); // Width, Height, Depth
    [SerializeField] private float wallThickness = 0.2f;
    [SerializeField] private float wallHeight = 2f; // Height of the walls

    [Header("Wall Material")]
    [SerializeField] private Material wallMaterial;
    [SerializeField] private bool usePhysicsCollider = true; // Use box collider if true
    [SerializeField] private bool useVisualRenderer = false; // Show visual walls if true

    private GameObject wallContainer;

    /// <summary>
    /// Called from inspector button or manually to generate walls
    /// </summary>
    public void GenerateWalls()
    {
        // Clean up existing walls
        if (wallContainer != null)
        {
            DestroyImmediate(wallContainer);
        }

        // Create container for walls
        wallContainer = new GameObject("BoardWalls");
        wallContainer.transform.SetParent(transform);
        wallContainer.transform.localPosition = Vector3.zero;

        // Create the four walls
        CreateWall("FrontWall", boardCenter + new Vector3(0, wallHeight / 2f, -boardSize.z / 2f), new Vector3(boardSize.x + wallThickness * 2, wallHeight, wallThickness));
        CreateWall("BackWall", boardCenter + new Vector3(0, wallHeight / 2f, boardSize.z / 2f), new Vector3(boardSize.x + wallThickness * 2, wallHeight, wallThickness));
        CreateWall("LeftWall", boardCenter + new Vector3(-boardSize.x / 2f, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, boardSize.z));
        CreateWall("RightWall", boardCenter + new Vector3(boardSize.x / 2f, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, boardSize.z));

        DebugManager.instance?.MyLOG("Board walls generated successfully!");
    }

    /// <summary>
    /// Creates a single wall with collider and optional visual renderer
    /// </summary>
    private void CreateWall(string wallName, Vector3 position, Vector3 size)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(wallContainer.transform);
        wall.transform.position = position;

        // Add collider
        if (usePhysicsCollider)
        {
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = size;
            collider.isTrigger = false; // Physical collider to block pieces
        }

        // Add visual renderer if enabled
        if (useVisualRenderer)
        {
            MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateBoxMesh(size);

            MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
            if (wallMaterial != null)
            {
                meshRenderer.material = wallMaterial;
            }
            else
            {
                meshRenderer.material = new Material(Shader.Find("Standard"));
                meshRenderer.material.color = new Color(0.7f, 0.7f, 0.7f, 0.3f);
            }
        }

        // Optional: Add Rigidbody to make it static
        Rigidbody rb = wall.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    /// <summary>
    /// Helper method to create a simple box mesh
    /// </summary>
    private Mesh CreateBoxMesh(Vector3 size)
    {
        Mesh mesh = new Mesh();
        mesh.name = "WallMesh";

        Vector3 halfSize = size / 2f;
        Vector3[] vertices = new Vector3[8]
        {
            new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
            new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
            new Vector3(halfSize.x, halfSize.y, -halfSize.z),
            new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
            new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
            new Vector3(halfSize.x, -halfSize.y, halfSize.z),
            new Vector3(halfSize.x, halfSize.y, halfSize.z),
            new Vector3(-halfSize.x, halfSize.y, halfSize.z)
        };

        int[] triangles = new int[36]
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            0, 1, 5, 0, 5, 4,
            2, 3, 7, 2, 7, 6,
            0, 4, 7, 0, 7, 3,
            1, 2, 6, 1, 6, 5
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Removes all generated walls
    /// </summary>
    public void RemoveWalls()
    {
        if (wallContainer != null)
        {
            DestroyImmediate(wallContainer);
        }
    }
}
