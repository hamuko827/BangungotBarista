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
}
