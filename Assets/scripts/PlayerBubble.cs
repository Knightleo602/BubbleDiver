using System;
using UnityEngine;

public class PlayerBubble : MonoBehaviour
{
    [Header("Components")]
    public GameObject bubbleModel;
    
    [Header("Values")]
    [SerializeField] public float bubbleReduceRate;
    [SerializeField] public float bubbleStrength;
    [SerializeField] public float bubbleMaxStrength;
    [SerializeField] public float bubbleMinStrength = 0.8f;
    [SerializeField] public float onDashReduceAmount = 1.5f;
    [SerializeField] public float onSprintReduceAmount = 0.5f;
    
    [NonSerialized] public bool IsProtected;
    
    private float _bubbleStrengthBase;
    private float _bubbleReduceRateBase;

    private void Reset()
    {
        bubbleStrength = _bubbleStrengthBase;
        SetBubbleSize();
        SetIsProtected(bubbleStrength > bubbleMinStrength);
    }

    private void Start()
    {
        _bubbleReduceRateBase = bubbleReduceRate;
        _bubbleStrengthBase = bubbleStrength;
        ItemBubble.OnBubbleCollected += IncreaseBubbleStrength;
        Player.TakeHit += ReduceStrength;
        WorldLogic.OnGameReset += Reset;
        Player.IsDashing += OnDash;
        Player.IsSprinting += OnSprint;
        SetBubbleSize();
        SetIsProtected(bubbleStrength > bubbleMinStrength);
    }

    private void Update()
    {
        if (bubbleStrength <= 0) return;
        ReduceStrength();
    }

    private void SetBubbleSize()
    {
        bubbleModel.transform.localScale = new Vector3(bubbleStrength, bubbleStrength, 1);
    }
    
    private void SetIsProtected(bool protect)
    {
        if (protect == IsProtected) return;
        bubbleModel.SetActive(protect);
        IsProtected = protect;
    }
    
    private void IncreaseBubbleStrength(float amount)
    {
        bubbleStrength = Mathf.Min(bubbleStrength + amount, bubbleMaxStrength);
    }
    
    private void ReduceStrength(float extraAmount = 0)
    {
        bubbleStrength -= extraAmount;
        bubbleStrength -= Mathf.Max(bubbleReduceRate * Time.deltaTime, 0);
        SetBubbleSize();
        SetIsProtected(bubbleStrength > bubbleMinStrength);
    }
    
    private void OnDash()
    {
        ReduceStrength(onDashReduceAmount);
    }

    private void OnSprint(bool isSprinting)
    {
        bubbleReduceRate = isSprinting ? onSprintReduceAmount : _bubbleReduceRateBase;
    }
}
