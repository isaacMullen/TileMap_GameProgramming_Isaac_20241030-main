using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyController : MonoBehaviour
{
    public PlayerController playerController;
    public TileManager tileManager;
    
    public Tilemap enemyTileMap;
    public TileBase enemyTileBase;

    private Vector3Int enemySpawnLocation;
    // Start is called before the first frame update
    void Start()
    {               
        SpawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SpawnEnemy()
    {
        enemySpawnLocation = new Vector3Int(Random.Range(1, 9), Random.Range(1, 9), 0);

        enemyTileMap.SetTile(enemySpawnLocation + tileManager.offset, enemyTileBase);
        Debug.Log($"Enemy Location: {enemySpawnLocation}");
    }
}