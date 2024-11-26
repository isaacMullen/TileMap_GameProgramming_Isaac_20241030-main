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
        SpawnEnemy();

        enemyCurrentPosition = enemySpawnLocation;
    }

    // Update is called once per frame
    void Update()
    {               
        //CORRECTING FOR DIFFERENT TILEMAP COORDINATES BY CONVERTING FROM WORLD TO CELL THEN CONVERTING BACK TO WORLD PS
        Vector3Int normalizedEnemyPosition = tileManager.tileMap.WorldToCell(enemyTileMap.CellToWorld(enemyCurrentPosition));
        Vector3Int normalizedPlayerPosition = tileManager.tileMap.WorldToCell(playerController.playerMap.CellToWorld(playerController.playerPosition));

        //CALCULATING DIFFERENCE BETWEEN PLAYER AND ENEMY (WILL BE USED FOR ENEMY AI IN NEAR FUTURE)
        Vector3Int gridDifference = normalizedPlayerPosition - tileManager.offset - enemyCurrentPosition;

        Debug.Log($"enemyPos: {enemyCurrentPosition} | PlayerPos: {normalizedPlayerPosition - tileManager.offset} | Difference: {gridDifference}");
    }
        
   /* IEnumerator ChaseEnemy()
    {
        
    }*/

    void SpawnEnemy()
    {
        enemySpawnLocation = new Vector3Int(Random.Range(1, 9), Random.Range(1, 9), 0);

        enemyTileMap.SetTile(enemySpawnLocation + tileManager.offset, enemyTileBase);
        Debug.Log($"Enemy Location: {enemySpawnLocation}");
    }
}
