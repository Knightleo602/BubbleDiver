using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ItemBubble : MonoBehaviour, IItem
{
    [SerializeField] public BubbleSize bubbleSize = BubbleSize.Random;
    
    [Header("Sprites")]
    [SerializeField] public Sprite bigBubble;
    [SerializeField] public Sprite mediumBubble;
    [SerializeField] public Sprite smallBubble;
    
    [Header("Size value increase")]
    [SerializeField] public float bubbleBigSizeValue = 2f;
    [SerializeField] public float bubbleMediumSizeValue = 1f;
    [SerializeField] public float bubbleSmallSizeValue = 0.5f;
    
    public static event Action<float> OnBubbleCollected;
    
    private float _bubbleSizeValue;
    
    private void Start()
    {
        while (true)
        {
            switch (bubbleSize)
            {
                case BubbleSize.Big:
                    _bubbleSizeValue = bubbleBigSizeValue;
                    GetComponent<SpriteRenderer>().sprite = bigBubble == null ? Resources.Load<Sprite>("Sprites/Bolha_grande.png") : bigBubble;
                    break;
                case BubbleSize.Medium:
                    _bubbleSizeValue = bubbleMediumSizeValue;
                    GetComponent<SpriteRenderer>().sprite = bigBubble == null ? Resources.Load<Sprite>("Sprites/Bolha_media.png") : mediumBubble;
                    break;
                case BubbleSize.Small:
                    _bubbleSizeValue = bubbleSmallSizeValue;
                    GetComponent<SpriteRenderer>().sprite = bigBubble == null ? Resources.Load<Sprite>("Sprites/Bolha_pequena.png") : smallBubble;
                    break;
                default:
                    bubbleSize = BubbleEnumExtensions.GetRandomBubbleSize();
                    continue;
            }
            break;
        }
    }

    public void Collect()
    {
        Destroy(gameObject);
        OnBubbleCollected?.Invoke(_bubbleSizeValue);
    }
}
