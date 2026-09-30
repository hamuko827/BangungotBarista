using UnityEngine;

// Create via: Assets > Create > Bangungot > Drink Recipe (make 5, one per menu item)
[CreateAssetMenu(menuName = "Bangungot/Drink Recipe")]
public class DrinkRecipe : ScriptableObject
{
    public string displayName;
    public int coffee, milk, cream;

    [Header("Finished drink prefabs (must have a Drink component)")]
    public GameObject hotPrefab;
    public GameObject icedPrefab;

    public bool Matches(int[] counts) =>
        counts[(int)Ingredient.Coffee] == coffee &&
        counts[(int)Ingredient.Milk] == milk &&
        counts[(int)Ingredient.Cream] == cream;

    public GameObject PrefabFor(CupType type) =>
        type == CupType.Hot ? hotPrefab : icedPrefab;
}
