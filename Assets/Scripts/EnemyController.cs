using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public PlayerController playerController;
    public TileManager tileManager;

    public Tilemap enemyTileMap;
    public TileBase enemyTileBase;

    public TextMeshProUGUI enemyHealthText;
    public int enemyHealth;
    public int enemyMaxHealth;
    public Slider enemyHealthBar;

    public Vector3Int enemySpawnLocation;    
            
    Vector3Int enemyGridDifference;
    public Vector3Int enemyCurrentPosition;

    bool chasingPlayer = false;
    public bool defeated;

    public int enemyCount = 0;

    public float moveInterval;
    public float startingMoveInterval;


    // Start is called before the first frame update
    void Awake()
    {
        startingMoveInterval = moveInterval;
        enemyCurrentPosition = enemySpawnLocation;
        enemyCount = 0;
        SpawnEnemy();      
    }

    // Update is called once per frame
    void Update()
    {                
        enemyHealthBar.value = enemyHealth;

        moveInterval = Mathf.Clamp(moveInterval, .5f, startingMoveInterval);
        
        //CORRECTING FOR DIFFERENT TILEMAP COORDINATES BY CONVERTING FROM WORLD TO CELL THEN CONVERTING BACK TO WORLD PS
        //Vector3Int normalizedEnemyPosition = tileManager.tileMap.WorldToCell(enemyTileMap.CellToWorld(enemyCurrentPosition));
        Vector3Int normalizedPlayerPosition = tileManager.tileMap.WorldToCell(playerController.playerMap.CellToWorld(playerController.playerPosition));
        Vector3Int normalizedEnemyPosition = tileManager.tileMap.WorldToCell(enemyTileMap.CellToWorld(enemyCurrentPosition));

        //CALCULATING DIFFERENCE BETWEEN PLAYER AND ENEMY (WILL BE USED FOR ENEMY AI IN NEAR FUTURE)        
        enemyGridDifference = normalizedEnemyPosition - normalizedPlayerPosition;

        //Debug.Log($"enemyPos: {enemyCurrentPosition} | PlayerPos: {normalizedPlayerPosition - tileManager.offset} | Difference: {enemyGridDifference}");

        if(!chasingPlayer && !playerController.inCombat && !defeated && !playerController.isLoading)
        {
            StartCoroutine(ChaseEnemy());
        }
        else if(enemyTileMap.isActiveAndEnabled && defeated)
        {
            Debug.Log("Enemy Map Gone");
            enemyTileMap.enabled = false;
            enemyTileMap.SetTile(enemyCurrentPosition, null);

        }
        
    }    
        
    IEnumerator ChaseEnemy()
    {
        chasingPlayer = true;        

        //Debug.Log($"Enemy Grid Difference {enemyGridDifference}");
        
        if(!playerController.inCombat && enemyTileBase != null)
        {
            MoveEnemy();
            yield return new WaitForSeconds(moveInterval);
        }

        chasingPlayer = false;
    }

    List<Vector3Int> CheckDirectionToMove()
    {
        List<Vector3Int> directions = new List<Vector3Int>();

        if (enemyGridDifference.x > 0) // Player is to the left
        {
            if (Mathf.Abs(enemyGridDifference.y) <= enemyGridDifference.x)
            {
                directions.Add(Vector3Int.left);
            }
        }
        else if (enemyGridDifference.x < 0) // Player is to the right
        {
            if (Mathf.Abs(enemyGridDifference.y) <= Mathf.Abs(enemyGridDifference.x))
            {
                directions.Add(Vector3Int.right);
            }
        }

        if (enemyGridDifference.y > 0) // Player is below
        {
            if (Mathf.Abs(enemyGridDifference.x) <= enemyGridDifference.y)
            {
                directions.Add(Vector3Int.down);
            }
        }
        else if (enemyGridDifference.y < 0) // Player is above
        {
            if (Mathf.Abs(enemyGridDifference.x) <= Mathf.Abs(enemyGridDifference.y))
            {
                directions.Add(Vector3Int.up);
            }
        }
        return directions;
    }
    
    void MoveEnemy()
    {
        List<Vector3Int> possibleMoves = CheckDirectionToMove();        
        
        foreach(Vector3Int direction in possibleMoves)
        {
            Vector3Int newPosition = enemyCurrentPosition + direction;
            if (IsValidMove(newPosition))
            {
                enemyTileMap.ClearAllTiles();

                enemyCurrentPosition = newPosition;

                tileManager.ReplaceTile(enemyTileMap, enemyCurrentPosition, enemyTileBase);                                                             

                return;
            }
            
        }               
    }

    bool IsValidMove(Vector3Int position)
    {
        Vector3Int normalizedTileAtPositon = (tileManager.tileMap.WorldToCell(tileManager.tileMap.CellToWorld(position)));

        TileBase tileAtPosition = tileManager.tileMap.GetTile(normalizedTileAtPositon);
        TileBase playerAtPosition = playerController.playerMap.GetTile(position);        
                
        if (tileAtPosition == tileManager.seaBase && playerAtPosition != playerController.playerBase)
        {
            return true;
        }
        else if (tileAtPosition == tileManager.borderBase)
        {
            return false;
        }       
        else if (playerAtPosition == playerController.playerBase && !defeated)
        {
            playerController.inCombat = true;
            Debug.Log("Combat Started");
            return false;
        }
        return true;
    }    

    public void SpawnEnemy()
    {
        enemyTileMap.enabled = true;
        defeated = false;
        
        //Debug.Log(moveInterval);
        
        enemyHealthBar.maxValue = enemyMaxHealth;
        enemyHealth = enemyMaxHealth;
        
        //DETERMINING THE VALID BOUNDS OF THE MAP THE ENEMY CAN SPAWN IN
        Vector3Int environmentMapSize = tileManager.tileMap.size;
        Vector3Int excludeBorder = new Vector3Int(2, 1, 0);

        Vector3Int validSpawnBounds = environmentMapSize - excludeBorder + tileManager.offset;
        Vector3Int validSpawnMin = new Vector3Int(1, 1, 0) + tileManager.offset; // Minimum bounds considering the offset
        Vector3Int validSpawnMax = validSpawnBounds; // Maximum bounds considering the offset

        // Random spawn position within the valid bounds range
        enemySpawnLocation = new Vector3Int(Random.Range(validSpawnMin.x, validSpawnMax.x), Random.Range(validSpawnMin.y, validSpawnMax.y), 0);             

        //SETTING THE CURRENT LOCATION FOR LATER USE IN SIMPLE AI
        enemyCurrentPosition = enemySpawnLocation;

        //REGENERATING A RANDOM SPAWN LOCATION IF THE FIRST ONE CONTAINS A ROCK
        while (tileManager.obstacles.Contains((enemySpawnLocation)))
        {
            enemySpawnLocation = new Vector3Int(Random.Range(validSpawnMin.x, validSpawnMax.x), Random.Range(validSpawnMin.y, validSpawnMax.y), 0);
        }

        enemyTileMap.SetTile(enemySpawnLocation + tileManager.offset, enemyTileBase);
        Debug.Log($"Enemy Spawn Location {enemySpawnLocation}");
    }
}
