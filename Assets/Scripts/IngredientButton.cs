using UnityEngine;

// Sprite button with a Collider2D. For a UI Button, call Press() from onClick instead.
public class IngredientButton : MonoBehaviour
{
    public Ingredient ingredient;
    public CoffeeMachine machine;

    void OnMouseDown() => Press();
    public void Press() => machine.TryAddIngredient(ingredient);
}
