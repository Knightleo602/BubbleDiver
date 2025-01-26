using UnityEngine;

public class NuvensMove : MonoBehaviour
{
    public float speed = 2f; // Velocidade de movimento
    public float resetPosition = -20f; // Posição onde a nuvem será resetada
    public float startPosition = 20f; // Posição inicial da nuvem ao resetar

    void Update()
    {
        // Move a nuvem para a esquerda ao longo do eixo X
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Verifica se a nuvem saiu da tela
        if (transform.position.x <= resetPosition)
        {
            // Reposiciona a nuvem no início
            Vector3 newPosition = transform.position;
            newPosition.x = startPosition;
            transform.position = newPosition;
        }
    }
}
