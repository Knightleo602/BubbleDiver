using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public Camera mainCamera; // Referência à câmera principal
    public Transform targetPosition; // Posição final da câmera no mapa
    public float cameraSpeed = 2f; // Velocidade do movimento da câmera
    private bool startGame = false;

    void Update()
    {
        // Se o jogo começar, mover a câmera
        if (startGame)
        {
            var target = new Vector3(targetPosition.position.x, targetPosition.position.y, mainCamera.transform.position.z);
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, 
                target, 
                cameraSpeed * Time.deltaTime
            );

            // Parar o movimento quando a câmera atingir a posição final
            if (Vector3.Distance(mainCamera.transform.position, targetPosition.position) < 0.1f)
            {
                startGame = false;
                // Aqui você pode habilitar elementos do jogo, se necessário
            }
        }
    }

    public void StartGame()
    {
        // Esconder o Canvas (Menu) e iniciar o movimento
        GameObject menu = GameObject.Find("Canvas");
        if (menu != null) menu.SetActive(false);
        startGame = true;
        GameManager.Instance.StartGame(); // Inicia o jogo
    }
}
