using UnityEngine;

// Needs a Collider2D (trigger is fine). Make 4 Tray slots and 3 Machine slots
// (one per ingredient, with 'machine' assigned).
public class CupSlot : MonoBehaviour
{
    public SlotKind kind;
    public Ingredient ingredient;   // Machine slots only
    public CoffeeMachine machine;   // Machine slots only
    public Transform snapPoint;     // optional, defaults to this transform

    public Draggable Occupant { get; private set; }
    public Transform SnapPoint => snapPoint != null ? snapPoint : transform;

    public void Occupy(Draggable d) => Occupant = d;
    public void Vacate(Draggable d) { if (Occupant == d) Occupant = null; }

    public bool CanAccept(Draggable d)
    {
        if (Occupant != null) return false;
        switch (kind)
        {
            case SlotKind.Tray:    return true;                           // cups and finished drinks
            case SlotKind.Machine: return d is Cup && machine.IsEmpty;    // cups only, one at a time
        }
        return false; // SlotKind.Output is no longer used
    }
}