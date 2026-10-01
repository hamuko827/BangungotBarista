using System;
using System.Collections;
using UnityEngine;

public class CoffeeMachine : MonoBehaviour
{
    [Header("Slots")]
    public CupSlot[] machineSlots;   // 3, one per ingredient

    [Header("Drinks")]
    public DrinkRecipe[] recipes;    // the 5 menu items
    public GameObject failedHotPrefab;   // used when the 3 ingredients match no recipe
    public GameObject failedIcedPrefab;

    [Header("Brewing")]
    public float brewTime = 0.4f;
    public event Action<Cup> OnBrewStarted;   // hook particles / animated sprite here later
    public event Action<Drink> OnBrewFinished;

    [Header("Effects")]
    public GameObject brewEffectPrefab; // Drag your animated explosion/puff prefab here

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
        
        // 1. Spawn the visual effect over the cup
        GameObject activeEffect = null;
        if (brewEffectPrefab != null)
        {
            activeEffect = Instantiate(brewEffectPrefab, cup.transform.position, Quaternion.identity);
        }

        // 2. Wait for the animation to play
        yield return new WaitForSeconds(brewTime);

        // 3. Clean up the effect right before the drink appears
        if (activeEffect != null)
        {
            Destroy(activeEffect);
        }

        DrinkRecipe match = null;
        foreach (var r in recipes)
            if (r != null && r.Matches(cup.counts)) { match = r; break; }

        GameObject prefab;
        if (match != null) prefab = match.PrefabFor(cup.type);
        else prefab = cup.type == CupType.Hot ? failedHotPrefab : failedIcedPrefab;

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

        SfxBank.Play(match != null ? SfxId.DrinkFinished : SfxId.BrewFailed);
        OnBrewFinished?.Invoke(drink);
    }
}