using System;
using UnityEngine;

public class GameEndDetector : MonoBehaviour
{
    
    public static event Action OnReachFinishLine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            OnReachFinishLine?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
