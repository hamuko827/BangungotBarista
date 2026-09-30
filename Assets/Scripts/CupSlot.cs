using UnityEngine;

// Needs a Collider2D (trigger is fine). Make 1 Tray slot, 3 Machine slots
// (one per ingredient) and 1 Output slot.
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
            case SlotKind.Tray:    return d is Cup;
            case SlotKind.Machine: return d is Cup && machine.IsEmpty; // one cup in the machine at a time
            case SlotKind.Output:  return d is Drink;
        }
        return false;
    }
}
