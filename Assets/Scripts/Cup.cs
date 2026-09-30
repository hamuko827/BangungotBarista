using UnityEngine;

// Put on both cup base prefabs (hot and iced). Set 'type' in the inspector.
public class Cup : Draggable
{
    public CupType type;
    public readonly int[] counts = new int[3]; // indexed by (int)Ingredient

    public bool IsBrewing { get; set; }
    public int Total => counts[0] + counts[1] + counts[2];

    protected override bool CanDrag => !IsBrewing;

    public void Add(Ingredient ing) => counts[(int)ing]++;
}
