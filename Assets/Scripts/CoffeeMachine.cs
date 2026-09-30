using System;
using System.Collections;
using UnityEngine;

public class CoffeeMachine : MonoBehaviour
{
    [Header("Slots")]
    public CupSlot[] machineSlots;   // 3, one per ingredient

    [Header("Drinks")]
    public DrinkRecipe[] recipes;    // the 5 menu items
    public GameObject failedDrinkPrefab; // used when the 3 ingredients match no recipe

    [Header("Brewing")]
    public float brewTime = 0.4f;
    public event Action<Cup> OnBrewStarted;   // hook particles / animated sprite here later
    public event Action<Drink> OnBrewFinished;

    // Only cups count. A finished drink sitting in a slot doesn't block the machine.
    public bool IsEmpty
    {
        get
        {
            foreach (var s in machineSlots)
                if (s.Occupant is Cup) return false;
            return true;
        }
    }

    // Called by IngredientButton. Returns true if the click counted.
    public bool TryAddIngredient(Ingredient ing)
    {
        CupSlot slot = null;
        foreach (var s in machineSlots)
            if (s.ingredient == ing) slot = s;

        // cup must be sitting on the slot matching this ingredient
        if (slot == null || !(slot.Occupant is Cup cup) || cup.IsBrewing) return false;

        cup.Add(ing);
        if (cup.Total >= 3) StartCoroutine(Brew(cup, slot));
        return true;
    }

    IEnumerator Brew(Cup cup, CupSlot slot)
    {
        cup.IsBrewing = true;
        OnBrewStarted?.Invoke(cup);
        yield return new WaitForSeconds(brewTime); // placeholder for the particle effect

        DrinkRecipe match = null;
        foreach (var r in recipes)
            if (r != null && r.Matches(cup.counts)) { match = r; break; }

        GameObject prefab = match != null ? match.PrefabFor(cup.type) : failedDrinkPrefab;
        if (prefab == null)
        {
            Debug.LogError("No drink prefab assigned for " + (match != null ? match.name : "failed brew") + " (" + cup.type + ")");
            yield break;
        }

        Drink drink = Instantiate(prefab).GetComponent<Drink>();
        if (drink == null)
        {
            Debug.LogError("Drink prefab '" + prefab.name + "' has no Drink component on its root.");
            yield break;
        }
        drink.Init(cup.type, cup.counts, match);

        // Replace the cup in the same slot
        slot.Vacate(cup);
        Destroy(cup.gameObject);
        drink.PlaceIn(slot);
        OnBrewFinished?.Invoke(drink);
    }
}