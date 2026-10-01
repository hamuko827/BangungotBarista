using UnityEngine;

// Put on the clickable coffee base sprite (needs a Collider2D). One for hot, one for iced.
public class CupSpawner : MonoBehaviour
{
    public Cup cupPrefab;        // the hot or iced base prefab
    public CupSlot[] traySlots;  // the 4 order slots, in fill order (same array on both spawners)

    void OnMouseDown()
    {
        CupSlot free = FirstFreeSlot();
        if (free == null)
        {
            SfxBank.Play(SfxId.TrayFull);
            return; // all 4 slots are full
        }

        Cup cup = Instantiate(cupPrefab);
        cup.PlaceIn(free);

        // Shift spawned cup -1 on Z axis to render in front
        Vector3 pos = cup.transform.position;
        pos.z -= 1f;
        cup.transform.position = pos;

        SfxBank.Play(SfxId.BaseClick);
    }

    CupSlot FirstFreeSlot()
    {
        foreach (var s in traySlots)
            if (s.Occupant == null) return s;
        return null;
    }
}