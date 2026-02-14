using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SlidePuzzlePiece : MonoBehaviour
{
    private Vector3 lastPostion;
    private int lastZonePosition;
    private Vector3 currentPostion;
    private int currentZonePostion;
    private XRGrabInteractable grabInteractable;
    private SlidePuzzleGame slidePuzzleGameScript;
    private PuzzlePiece pieceType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnReleased);
        slidePuzzleGameScript = GameObject.Find("SlidePuzzle").GetComponent<SlidePuzzleGame>();
    }

    void Start()
    {
        lastPostion = transform.position;
        currentPostion = transform.position;
    }

    void OnDestroy()
    {
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    // ******* Public methods
    public void SetCurrentPosition(Vector3 position, int zone)
    {
        currentPostion = position;
        currentZonePostion = zone;
    }

    public void InitialValues(PuzzlePiece pieceType, int zone)
    {
        this.pieceType = pieceType;
        // currentPostion = position;
        // lastPostion = position;
        lastZonePosition = zone;
        currentZonePostion = zone;
    }

    // ******* Privates methods
    private void MoveToCurrentPosition()
    {
        if (currentPostion != null)
        {
            transform.position = currentPostion;
            DebugManager.instance.MyLOG("Piece released and moved to position: " + currentPostion);
        }
    }

    private void OnReleased(SelectExitEventArgs arg0)
    {
        UpdatePieceValues();
    }

    private void UpdatePieceValues()
    {
        if (currentPostion == lastPostion)
        {
            MoveToCurrentPosition();
            return;
        }

        if (lastZonePosition >= 0)
            slidePuzzleGameScript.RemovePieceFromZone(pieceType, lastZonePosition);

        if (currentZonePostion >= 0)
            slidePuzzleGameScript.AddPieceToZone(pieceType, currentZonePostion);
        
        slidePuzzleGameScript.UpdatePositionAvailability(lastPostion, true);
        slidePuzzleGameScript.UpdatePositionAvailability(currentPostion, false);
        lastPostion = currentPostion;
        lastZonePosition = currentZonePostion;
        MoveToCurrentPosition();
    }
}
