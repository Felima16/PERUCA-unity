using UnityEngine;

public class SlidePuzzleZonePosition : MonoBehaviour
{
    [SerializeField] private int zone;
    [SerializeField] private Vector3 postion;
    [SerializeField] private bool isAvailable = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isAvailable)
        {
            other.gameObject.GetComponent<SlidePuzzlePiece>().SetCurrentPosition(transform.position, zone);
            DebugManager.instance.MyLOG("#### OnTriggerEnter: " + other.tag + " in zone " + zone);
        }
    }
    
    public void SetAvailable(bool available)
    {
        isAvailable = available;
    }
}