using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomerData", menuName = "Bangungot Barista/Customer Data")]
public class CustomerData : ScriptableObject
{
    [Header("Customer Identification")]
    public string customerName;

    [Header("Patience Configuration (Seconds)")]
    public float minPatience = 40f;
    public float maxPatience = 60f;

    [Header("Sprites")]
    public Sprite awakeSprite;
    public Sprite awakeBubbleSprite;
    
    public Sprite slightlySleepySprite;
    public Sprite slightlySleepyBubbleSprite;
    
    public Sprite reallySleepySprite;
    public Sprite reallySleepyBubbleSprite;
}