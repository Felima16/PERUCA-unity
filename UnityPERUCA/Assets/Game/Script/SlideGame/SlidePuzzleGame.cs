using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public enum PuzzlePiece {
    Red,
    Green,
    Yellow,
    Purple,
    none
}
public class SlidePuzzleGame : MonoBehaviour
{
    [Header("Puzzle Game")]
    [SerializeField]
    private GameObject[] puzzlePiecePositions; // Array to hold the puzzle pieces
    [SerializeField]
    private GameObject puzzlePiecePrefab; // Prefab for the puzzle pieces
    [SerializeField]
    private GameObject piecesZone; // Zone for the puzzle pieces

    [Header("UI Puzzle Game")]
    [SerializeField]
    private GameObject winnerCanvas; // Canvas to show when the player wins

    /// ******* Colors
    private Color greenColor = new Color(40.0f / 255.0f, 207.0f / 255.0f, 53.0f / 255.0f);
    private Color redColor = new Color(217.0f / 255.0f, 24.0f / 255.0f, 15.0f / 255.0f);
    private Color yellowColor = new Color(241.0f / 255.0f, 238.0f / 255.0f, 87.0f / 255.0f);
    private Color purpleColor = new Color(192.0f / 255.0f, 15.0f / 255.0f, 217.0f / 255.0f);

    // ******* Management Puzzle pieces 
    private List<GameObject> puzzlePieces = new List<GameObject>(); // List to hold the instantiated puzzle pieces
    private List<Zone> zones; // List to hold the zones
    private bool isGameStarted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameStarted)
            return;

        CheckWinCondition();
    }

    // ******* Initialize the game
    private void StartGame()
    {
        // Initialize the zones
        InitializeZones();
        // Shuffle the puzzle pieces
        ShufflePuzzlePieces();
    }

    private void InitializeZones()
    {
        // Initialize the zones
        zones = new List<Zone>();
        for (int i = 0; i < 4; i++)
        {
            Zone zone = new Zone();
            zone.zoneNumber = i;
            zones.Add(zone);
        }
    }
    private void ShufflePuzzlePieces()
    {
        FirstPieceInZone();
        winnerCanvas.SetActive(false); // Hide the winner canvas at the start
        isGameStarted = true;
    }

    private void FirstPieceInZone()
    {
        int[] initialIndexes = { 0, 1, 2, 3 };

        for (int i = 0; i < 4; i++)
        {
            PuzzlePiece puzzlePiece = (PuzzlePiece)i;
            int pieceIndex = UnityEngine.Random.Range(0, initialIndexes.Length);
            InstantiatePuzzlePieces(initialIndexes[pieceIndex], puzzlePiece);
            zones[initialIndexes[pieceIndex]].AddPiece(puzzlePiece);
            initialIndexes = initialIndexes.Where((val, idx) => idx != pieceIndex).ToArray();
        }
        // Instantiate the first puzzle piece in the zone
        OthersPieceInZone();
    }

    private void OthersPieceInZone()
    {
        PuzzlePiece[] restPieces = {
            PuzzlePiece.Red,
            PuzzlePiece.Green,
            PuzzlePiece.Yellow,
            PuzzlePiece.Purple,
            PuzzlePiece.Red,
            PuzzlePiece.Green,
            PuzzlePiece.Yellow,
            PuzzlePiece.Purple
        };
        // Instantiate the other puzzle pieces in the zones
        for (int i = 4; i < (puzzlePiecePositions.Length - 2); i++)
        {
            int pieceIndex = UnityEngine.Random.Range(0, restPieces.Length);
            InstantiatePuzzlePieces(i, restPieces[pieceIndex]);
            switch (i)
            {
                case 4 or 5:
                    zones[0].AddPiece(restPieces[pieceIndex]);
                    break;
                case 6 or 7:
                    zones[1].AddPiece(restPieces[pieceIndex]);
                    break;
                case 8 or 9:
                    zones[2].AddPiece(restPieces[pieceIndex]);
                    break;
                case 10 or 11:
                    zones[3].AddPiece(restPieces[pieceIndex]);
                    break;
            }
            restPieces = restPieces.Where((val, idx) => idx != pieceIndex).ToArray();
        }

        // Log the initial state of the zones
        DebugManager.instance.MyLOG("Initial state of the zones:");
        DebugManager.instance.MyLOG(zones[0].printZone());
        DebugManager.instance.MyLOG(zones[1].printZone());
        DebugManager.instance.MyLOG(zones[2].printZone());
        DebugManager.instance.MyLOG(zones[3].printZone());
        DebugManager.instance.MyLOG("Game started");
    }

    private void InstantiatePuzzlePieces(int index, PuzzlePiece puzzlePiece)
    {

        GameObject piece = Instantiate(puzzlePiecePrefab, puzzlePiecePositions[index].transform.position, puzzlePiecePrefab.transform.rotation);
        piece.transform.SetParent(piecesZone.transform);
        MeshRenderer meshRenderer = piece.GetComponentInChildren<MeshRenderer>();

        switch (puzzlePiece)
        {
            case PuzzlePiece.Red:
                meshRenderer.material.color = redColor;
                piece.tag = StringManager.instance.pieceRedTag;

                break;
            case PuzzlePiece.Green:
                meshRenderer.material.color = greenColor;
                piece.tag = StringManager.instance.pieceGreenTag;
                break;
            case PuzzlePiece.Yellow:
                meshRenderer.material.color = yellowColor;
                piece.tag = StringManager.instance.pieceYellowTag;
                break;
            case PuzzlePiece.Purple:
                meshRenderer.material.color = purpleColor;
                piece.tag = StringManager.instance.piecePurpleTag;
                break;
        }
        puzzlePieces.Add(piece);

        int zoneIndex = 0;
        switch (index)
        {
            case < 4:
                zoneIndex = index; // First four pieces go to zone 0
                break;
            case 4 or 5:
                zoneIndex = 0; // Pieces 4 and 5 go to zone 0
                break;
            case 6 or 7:
                zoneIndex = 1; // Pieces 6 and 7 go to zone 1
                break;
            case 8 or 9:
                zoneIndex = 2; // Pieces 8 and 9 go to zone 2
                break;
            case 10 or 11:
                zoneIndex = 3; // Pieces 10 and 11 go to zone 3
                break;
        }

        piece.GetComponent<SlidePuzzlePiece>().InitialValues(puzzlePiece, zoneIndex);
    }
    private void DestroyPuzzlePieces()
    {
        // Destroy all instantiated puzzle pieces
        foreach (var piece in puzzlePieces)
        {
            Destroy(piece);
        }
        puzzlePieces.Clear();
    }
    private void OnDestroy()
    {
        // Clean up the puzzle pieces when the game object is destroyed
        DestroyPuzzlePieces();
    }

    // ******* Game logic
    private void CheckWinCondition()
    {
        if (zones[0].CheckPuzzlePiecesInZone() && zones[1].CheckPuzzlePiecesInZone() && zones[2].CheckPuzzlePiecesInZone() && zones[3].CheckPuzzlePiecesInZone())
        {
            winnerCanvas.SetActive(true);
            isGameStarted = false; // Stop the game
        }
    }

    // ******* External methods
    public void AddPieceToZone(PuzzlePiece piece, int zoneNumber)
    {
        zones[zoneNumber].AddPiece(piece);
        DebugManager.instance.MyLOG(zones[zoneNumber].printZone());
    }
    public void RemovePieceFromZone(PuzzlePiece piece, int zoneNumber)
    {
        zones[zoneNumber].RemovePiece(piece);
        DebugManager.instance.MyLOG(zones[zoneNumber].printZone());
    }
    public void ResetGame()
    {
        // Reset the game by reinitializing the zones and shuffling the pieces
        InitializeZones();
        DestroyPuzzlePieces();
        ShufflePuzzlePieces();
    }
    
    // Update the availability of the position based on the pieces in it
    public void UpdatePositionAvailability(Vector3 position, bool isAvailable)
    {
        DebugManager.instance.MyLOG("***** Update availability of position: " + position + " to " + isAvailable);

        // Find the position GameObject in the puzzlePiecePositions array
        GameObject positionObject = Array.Find(puzzlePiecePositions, p => p.transform.position == position);
        if (positionObject == null)
        {
            DebugManager.instance.MyLOG("Position not found: " + position);
            return;
        }
        SlidePuzzleZonePosition zonePosition = positionObject.GetComponent<SlidePuzzleZonePosition>();
        if (zonePosition != null)
        {
            zonePosition.SetAvailable(isAvailable);
            DebugManager.instance.MyLOG("***** Set availability of position: " + position + " to " + isAvailable);
        }
    }
}

class Zone
{
    public List<PuzzlePiece> pieces = new List<PuzzlePiece>();
    public int zoneNumber;

    public void AddPiece(PuzzlePiece piece)
    {
        if (pieces.Count >= 3)
            return;
        pieces.Add(piece);
    }
    public void RemovePiece(PuzzlePiece piece)
    {
        if (pieces.Count == 0)
            return;
        pieces.Remove(piece);
    }

    public bool CheckPuzzlePiecesInZone()
    {
        if (pieces.Count < 3)
            return false;
        // Check if the puzzle pieces in the zone are correct
        if (pieces[0] == pieces[1] && pieces[1] == pieces[2])
            return true;

        return false;
    }
    public void ClearZone()
    {
        pieces.Clear();
    }

    public string printZone()
    {
        string piecesString = string.Join(", ", pieces.Select(p => p.ToString()));
        Debug.Log("Zone " + zoneNumber + ": " + piecesString);
        return "Zone " + zoneNumber + ": " + piecesString;
    }
}

/*
    - check the reason why the pieces are not moved to the correct position
    - check the reason why the pieces are not added to the zone when they are moved
    - check the piece touch area
*/