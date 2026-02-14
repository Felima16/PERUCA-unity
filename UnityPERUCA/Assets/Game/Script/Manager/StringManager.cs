using UnityEngine;

public class StringManager : MonoBehaviour
{
    public static StringManager instance;

    public string pieceGreenTag = "PieceGreen";
    public string pieceRedTag = "PieceRed";
    public string pieceYellowTag = "PieceYellow";
    public string piecePurpleTag = "PiecePurple";
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        instance = null;
    }
}
