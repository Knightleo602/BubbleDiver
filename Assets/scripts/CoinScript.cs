using System;
using UnityEngine;

public class CoinScript : MonoBehaviour, IItem
{
    
    public static event Action OnGetTreasure; 
    
    public void Collect()
    {
        OnGetTreasure?.Invoke();
        Destroy(gameObject);
    }
}
