using UnityEngine;

// Sprite button with a Collider2D. For a UI Button, call Press() from onClick instead.
public class IngredientButton : MonoBehaviour
{
    public Ingredient ingredient;
    public CoffeeMachine machine;

    void OnMouseDown() => Press();

    public void Press()
    {
        bool counted = machine.TryAddIngredient(ingredient);
        SfxBank.Play(counted ? SfxId.IngredientClick : SfxId.IngredientRejected);
    }
}