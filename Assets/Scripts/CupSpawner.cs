using UnityEngine;

// Put on the clickable coffee base sprite (needs a Collider2D). Make one for hot, one for iced.
public class CupSpawner : MonoBehaviour
{
    public Cup cupPrefab;     // the hot or iced base prefab
    public CupSlot traySlot;  // slot 1

    void OnMouseDown()
    {
        if (traySlot.Occupant != null) return; // tray is full
        Cup cup = Instantiate(cupPrefab);
        cup.PlaceIn(traySlot);
    }
}
