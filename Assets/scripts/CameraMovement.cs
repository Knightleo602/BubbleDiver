using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    
    public GameObject player;

    private void Update()
    {
        transform.position = player.transform.position;
    }
}
