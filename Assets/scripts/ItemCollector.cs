using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var item = other.GetComponent<IItem>();
        if (item == null) return;
        item.Collect();
        Debug.Log("Item collected");
    }
}
