using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyController : MonoBehaviour
{
    public PlayerController playerController;
    public TileManager tileManager;

    public Tilemap enemyTileMap;
    public TileBase enemyTileBase;

    public TextMeshProUGUI enemyHealthText;
    public int enemyHealth;

    public Vector3Int enemySpawnLocation;

    Vector3Int enemyCurrentPosition;

    // Start is called before the first frame update
    void Start()
    {
        enemyCurrentPosition = enemySpawnLocation;
        SpawnEnemy();

        foreach (Vector3Int rock in tileManager.obstacles)
        {
            Debug.Log(rock);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //CORRECTING FOR DIFFERENT TILEMAP COORDINATES BY CONVERTING FROM WORLD TO CELL THEN CONVERTING BACK TO WORLD PS
        Vector3Int normalizedEnemyPosition = tileManager.tileMap.WorldToCell(enemyTileMap.CellToWorld(enemyCurrentPosition));
        Vector3Int normalizedPlayerPosition = tileManager.tileMap.WorldToCell(playerController.playerMap.CellToWorld(playerController.playerPosition));

        //CALCULATING DIFFERENCE BETWEEN PLAYER AND ENEMY (WILL BE USED FOR ENEMY AI IN NEAR FUTURE)
        Vector3Int gridDifference = normalizedPlayerPosition - tileManager.offset - enemyCurrentPosition;

        //Debug.Log($"enemyPos: {enemyCurrentPosition} | PlayerPos: {normalizedPlayerPosition - tileManager.offset} | Difference: {gridDifference}");
    }

    /* IEnumerator ChaseEnemy()
     {

     }*/

    TileBase GetUnderlyingTile(Tilemap topLayerTilemap, Tilemap bottomLayerTilemap, Vector3Int topLayerCellPosition)
    {
        // Convert the top-layer cell position to world space
        Vector3 worldPosition = topLayerTilemap.CellToWorld(topLayerCellPosition);

        // Convert the world position to cell space in the bottom layer
        Vector3Int bottomLayerCellPosition = bottomLayerTilemap.WorldToCell(worldPosition);

        // Get the tile from the bottom layer
        return bottomLayerTilemap.GetTile(bottomLayerCellPosition);
    }

    void SpawnEnemy()
    {
        //DETERMINING THE VALID BOUNDS OF THE MAP THE ENEMY CAN SPAWN IN
        Vector3Int environmentMapSize = tileManager.tileMap.size;
        Vector3Int excludeBorder = new Vector3Int(1, 1, 0);

        Vector3Int validSpawnBounds = environmentMapSize - excludeBorder;        

        //DETERMINING A RANDOM SPAWN POSITION BASED ON THE VALID BOUNDS
        enemySpawnLocation = new Vector3Int((Random.Range(1, validSpawnBounds.x)), Random.Range(1, validSpawnBounds.y), 0);

        //SETTING THE CURRENT LOCATION FOR LATER USE IN SIMPLE AI
        enemyCurrentPosition = enemySpawnLocation;

        //REGENERATING A RANDOM SPAWN LOCATION IF THE FIRST ONE CONTAINS A ROCK
        while (tileManager.obstacles.Contains((enemySpawnLocation)))
        {
            enemySpawnLocation = new Vector3Int((Random.Range(1, validSpawnBounds.x)), Random.Range(1, validSpawnBounds.y), 0);
        }

        enemyTileMap.SetTile(enemySpawnLocation + tileManager.offset, enemyTileBase);
    }
}
