using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldLogic : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] public Camera playerCamera;
    [SerializeField] public GameObject water;
    
    [Header("Bubble Item")]
    [SerializeField] public GameObject bubblePrefab;
    [SerializeField] public float bubbleSpawnRate = 1f;
    [SerializeField] public int maxBubbleAmount = 20;
    [SerializeField] public float maxBubbleSpawnOffScreenOffsetX = 20;
    [SerializeField] public float maxBubbleSpawnOffScreenOffsetY = 20;
    
    private int _bubbleCount;
    private bool _isGameRunning = true;
    private bool _playerIsHoldingTreasure;

    public static event Action OnGameReset;
    public static event Action OnGameFinished;
    public static event Action OnGameStart;

    private void Start()
    {
        ItemBubble.OnBubbleCollected += OnBubbleDestroyed;
        Player.PlayerHasDied += OnPlayerDeath;
        GameEndDetector.OnReachFinishLine += PlayerReachedFinishLine;
        CoinScript.OnGetTreasure += OnPlayerGetTreasure;
        GameManager.Instance.OnGameStart += StartGame;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStart -= StartGame;
    }

    private void Reset()
    {
        if(_bubbleCount > 0)
        {
            foreach (var o in GameObject.FindGameObjectsWithTag("GetBubble"))
            {
                Destroy(o);
            }
        }
        _isGameRunning = true;
        _bubbleCount = 0;
        OnGameReset?.Invoke();
        StartSummoningBubbles();
    }

    private void SpawnBubble()
    {
        if (_bubbleCount >= maxBubbleAmount) return;
        while (_isGameRunning)
        {
            var randomX = Random.Range(-maxBubbleSpawnOffScreenOffsetX, Screen.width + maxBubbleSpawnOffScreenOffsetX);
            var randomY = Random.Range(-maxBubbleSpawnOffScreenOffsetY, Screen.height + maxBubbleSpawnOffScreenOffsetY);
            var screenPosition = playerCamera.ScreenToWorldPoint(new Vector3(randomX, randomY, playerCamera.farClipPlane / 2));
            var pos = new Vector3(screenPosition.x, screenPosition.y + water.transform.position.y, 0);
            if (Physics2D.OverlapCircle(pos, 1f)) continue;
            _bubbleCount++;
            Instantiate(bubblePrefab, pos, Quaternion.identity);
            StartSummoningBubbles();
            break;
        }
    }
    
    private void OnPlayerGetTreasure()
    {
        _playerIsHoldingTreasure = true;
    }
    
    private void PlayerReachedFinishLine()
    {
        if (!_playerIsHoldingTreasure) return;
        Debug.Log("Player reached finish line with treasure");
        _isGameRunning = false;
        OnGameFinished?.Invoke();
    }

    private void OnBubbleDestroyed(float amount)
    {
        _bubbleCount--;
        if(_bubbleCount < maxBubbleAmount) return;
        StartSummoningBubbles();
    }

    private void StartSummoningBubbles()
    {
        Invoke(nameof(SpawnBubble), bubbleSpawnRate);
    }
    
    private void OnPlayerDeath()
    {
        _isGameRunning = false;
    }

    private void StartGame()
    {
        StartSummoningBubbles();
        _isGameRunning = true;
        Debug.Log("Jogo iniciado!");
        OnGameStart?.Invoke();
    }
}