using UnityEngine;

// Shared drag logic for Cup and Drink. Needs a Collider2D on the prefab.
// Uses OnMouse* callbacks. Camera must be tagged MainCamera.
[RequireComponent(typeof(Collider2D))]
public abstract class Draggable : MonoBehaviour
{
    CupSlot home;
    Vector3 grabOffset;
    bool dragging;

    protected virtual bool CanDrag => true;

    static Vector3 MouseWorld()
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0f;
        return p;
    }

    public void PlaceIn(CupSlot slot)
    {
        if (home != null) home.Vacate(this);
        home = slot;
        slot.Occupy(this);
        transform.position = slot.SnapPoint.position;
    }

    void OnMouseDown()
    {
        if (!CanDrag) return;
        dragging = true;
        grabOffset = transform.position - MouseWorld();
        if (home != null) home.Vacate(this); // slot is free while held; home is remembered for snap-back
        SfxBank.Play(SfxId.CupPickUp);
    }

    void OnMouseDrag()
    {
        if (dragging) transform.position = MouseWorld() + grabOffset;
    }

    void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;

        foreach (Collider2D hit in Physics2D.OverlapPointAll(MouseWorld()))
        {
            if (hit.gameObject == gameObject) continue;
            if (TryDrop(hit)) return;
        }
        PlaceIn(home); // nothing valid under the pointer: snap back
    }

    // Override in Drink later to also accept Customers.
    protected virtual bool TryDrop(Collider2D hit)
    {
        if (hit.TryGetComponent(out TrashCan _))
        {
            SfxBank.Play(SfxId.Trashed);
            Destroy(gameObject);
            return true;
        }
        if (hit.TryGetComponent(out CupSlot slot) && slot.CanAccept(this))
        {
            PlaceIn(slot);
            SfxBank.Play(SfxId.CupPlaced);
            return true;
        }
        return false;
    }
}