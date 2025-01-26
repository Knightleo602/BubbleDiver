using UnityEngine;

public class VerticalCameraFollow : MonoBehaviour
{
    public Transform player; // Referência ao Transform do jogador
    public float upperLimit = 0f; // Limite superior da câmera
    public float lowerLimit = -10f; // Limite inferior da câmera
    private bool canFollow = false; // Controla se a câmera deve seguir o jogador
    private bool reachedUpperLimit = false; // Verifica se a câmera atingiu o upperLimit

    private void Start()
    {
        // Inscreve-se no evento do GameManager
        WorldLogic.OnGameStart += EnableCameraFollow;
    }

    private void LateUpdate()
    {
        // Só altera a posição da câmera se o jogo começou e ela alcançou o upperLimit
        if (!canFollow || !player || !reachedUpperLimit) return;

        // Pega a posição atual da câmera
        Vector3 cameraPosition = transform.position;

        // Atualiza apenas o eixo Y para seguir o jogador, mantendo dentro dos limites
        cameraPosition.y = Mathf.Clamp(player.position.y, lowerLimit, upperLimit);

        // Atualiza a posição da câmera (sem alterar X e Z)
        transform.position = cameraPosition;
    }

    private void FixedUpdate()
    {
        // Antes de permitir a movimentação, checa se a câmera alcançou o upperLimit
        if (!reachedUpperLimit && canFollow)
        {
            Vector3 cameraPosition = transform.position;

            // Move a câmera em direção ao upperLimit
            cameraPosition.y = Mathf.MoveTowards(cameraPosition.y, upperLimit, Time.deltaTime * 2f); // Velocidade ajustável
            transform.position = cameraPosition;

            // Se a câmera alcançou o upperLimit, ativa o flag
            if (Mathf.Approximately(cameraPosition.y, upperLimit))
            {
                reachedUpperLimit = true;
            }
        }
    }

    private void EnableCameraFollow()
    {
        canFollow = true; // Ativa o movimento da câmera
    }
}
