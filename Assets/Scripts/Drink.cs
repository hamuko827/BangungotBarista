using UnityEngine;

// Put on every finished-drink prefab (and the failed-brew prefab).
public class Drink : Draggable
{
    public CupType Type { get; private set; }
    public int[] Counts { get; private set; }
    public DrinkRecipe Recipe { get; private set; } // null = failed brew

    public void Init(CupType type, int[] counts, DrinkRecipe recipe)
    {
        Type = type;
        Counts = (int[])counts.Clone();
        Recipe = recipe;
    }

    protected override bool TryDrop(Collider2D hit)
    {
        // 1. Check if we dropped on Customer or inside Customer's Thought Bubble collider
        Customer customer = hit.GetComponent<Customer>();
        if (customer == null) customer = hit.GetComponentInParent<Customer>();

        if (customer != null)
        {
            if (customer.ReceiveDrink(this))
            {
                Destroy(gameObject); // Served successfully: destroy the held drink
                return true;
            }
            return false; // Wrong drink: snap back to previous home slot
        }

        // 2. Check if we dropped it in the Trash
        if (hit.TryGetComponent(out TrashCan _))
        {
            SfxBank.Play(SfxId.Trashed);
            Destroy(gameObject);
            return true;
        }

        // 3. Check if we dropped it on an empty Tray Slot
        if (hit.TryGetComponent(out CupSlot slot) && slot.CanAccept(this))
        {
            PlaceIn(slot);
            SfxBank.Play(SfxId.CupPlaced);
            return true;
        }

        return false;
    }
}