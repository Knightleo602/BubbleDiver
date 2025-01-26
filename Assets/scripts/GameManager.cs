using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton para fácil acesso
    public event Action OnGameStart; // Evento para notificar quando o jogo começar

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Não destrua ao carregar uma nova cena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        Debug.Log("Jogo iniciado!");
        OnGameStart?.Invoke(); // Dispara o evento OnGameStart
    }
}
