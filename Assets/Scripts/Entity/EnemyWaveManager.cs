using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class EnemyWaveManager : GenericSingleton<EnemyWaveManager>
{
    private bool isInitialized;

    private int areaWidth;
    private int areaHeight;
    public Vector3 defaultDestination;
    private bool waveStarted = false;

    public List<EntityController2D> enemies = new List<EntityController2D>();
    public int waveNumber = 0;

    [SerializeField] private GameObject enemyPrefab;


    public static event Action OnWaveCleared;


    public void Initialize(int width, int height, float cellSize, Vector3 defaultDestination)
    {
        if (isInitialized)
            return;

        areaWidth = Mathf.FloorToInt(width * cellSize);
        areaHeight = Mathf.FloorToInt(height * cellSize);
        this.defaultDestination = defaultDestination;

        isInitialized = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TextDisplayManager.NewUI(new Vector2(750, -450), 1.5f)
                .WithDraggable()
                .WithTrackedProvider(() => EntityController2D.ENTITY_COUNT.ToString())
                .Build();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) { 
            StartNextWave();
        }
    }

    public void StartNextWave()
    {
        //if (waveStarted)
            //return;
        
        waveStarted = true;

        waveNumber++;
        Debug.Log($"Starting wave {waveNumber}");

        for (int i = 0; i < waveNumber * 10; i++)
        {
            GameObject e = Instantiate(enemyPrefab, GetEdgeSpawn(), enemyPrefab.transform.rotation);
            enemies.Add(e.GetComponent<EntityController2D>());
        }
        
    }

    public void EnemyDefeated(GameObject enemy)
    {
        enemies.Remove(enemy.GetComponent<EntityController2D>());
        Destroy(enemy);

        if(enemies.Count == 0)
        {
            waveStarted = false;
            Debug.Log($"Wave {waveNumber} cleared!");
            OnWaveCleared?.Invoke();
        }
    }

    private Vector3 GetEdgeSpawn()
    {
        int edge = UnityEngine.Random.Range(0, 4);
        int x, y;

        switch (edge)
        {
            case 0: // Left column
                x = 0;
                y = UnityEngine.Random.Range(0, areaHeight);
                break;
            case 1: // Right column
                x = areaWidth - 1;
                y = UnityEngine.Random.Range(0, areaHeight);
                break;
            case 2: // Top row
                x = UnityEngine.Random.Range(0, areaWidth);
                y = areaHeight - 1;
                break;
            default: // Bottom row
                x = UnityEngine.Random.Range(0, areaWidth);
                y = 0;
                break;
        }

        return new Vector3(x, y);
    }
}
