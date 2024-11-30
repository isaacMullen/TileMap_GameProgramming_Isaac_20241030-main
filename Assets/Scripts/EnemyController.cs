using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.U2D.Aseprite;
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
    Vector3Int gridDifference;
    Vector3Int enemyGridDifference;
    Vector3Int enemyCurrentPosition;

    bool chasingPlayer = false;

    // Start is called before the first frame update
    void Awake()
    {
        enemyCurrentPosition = enemySpawnLocation;
        SpawnEnemy();        
    }

    // Update is called once per frame
    void Update()
    {
        //CORRECTING FOR DIFFERENT TILEMAP COORDINATES BY CONVERTING FROM WORLD TO CELL THEN CONVERTING BACK TO WORLD PS
        //Vector3Int normalizedEnemyPosition = tileManager.tileMap.WorldToCell(enemyTileMap.CellToWorld(enemyCurrentPosition));
        Vector3Int normalizedPlayerPosition = tileManager.tileMap.WorldToCell(playerController.playerMap.CellToWorld(playerController.playerPosition));
        Vector3Int normalizedEnemyPosition = tileManager.tileMap.WorldToCell(enemyTileMap.CellToWorld(enemyCurrentPosition));

        //CALCULATING DIFFERENCE BETWEEN PLAYER AND ENEMY (WILL BE USED FOR ENEMY AI IN NEAR FUTURE)        
        enemyGridDifference = normalizedEnemyPosition - normalizedPlayerPosition;

        //Debug.Log($"enemyPos: {enemyCurrentPosition} | PlayerPos: {normalizedPlayerPosition - tileManager.offset} | Difference: {enemyGridDifference}");

        if(!chasingPlayer && !playerController.inCombat)
        {
            StartCoroutine(ChaseEnemy());
        }
        
    }

    IEnumerator ChaseEnemy()
    {
        chasingPlayer = true;
        /*Debug.Log("Inside Enumerator");
        Debug.Log(CheckDirectionToMove());*/

        Debug.Log($"Enemy Grid Difference {enemyGridDifference}");
        
        if(!playerController.inCombat)
        {
            MoveEnemy();
            yield return new WaitForSeconds(.5f);
        }

        chasingPlayer = false;
    }

    List<Vector3Int> CheckDirectionToMove()
    {
        List<Vector3Int> directions = new List<Vector3Int>();
        
        if (enemyGridDifference.x > 0 && enemyGridDifference.y <= Mathf.Abs(enemyGridDifference.x))
        {
            directions.Add(Vector3Int.left);
        }
        else if (enemyGridDifference.x < 0 && enemyGridDifference.y >= enemyGridDifference.x)
        {
            directions.Add(Vector3Int.right);
        }
        if (enemyGridDifference.y > 0 && enemyGridDifference.x <= Mathf.Abs(enemyGridDifference.y))
        {
            directions.Add(Vector3Int.down);
        }
        else if (enemyGridDifference.y < 0 && enemyGridDifference.x >= enemyGridDifference.y)
        {
            directions.Add(Vector3Int.up);
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
        else if (tileAtPosition == tileManager.rockBase)
        {
            return false;
        }
        else if (playerAtPosition == playerController.playerBase)
        {
            playerController.inCombat = true;
            Debug.Log("Combat Started");
            return false;
        }
        return true;
    }

    /*TileBase GetUnderlyingTile(Tilemap topLayerTilemap, Tilemap bottomLayerTilemap, Vector3Int topLayerCellPosition)
    {
        // Convert the top-layer cell position to world space
        Vector3 worldPosition = topLayerTilemap.CellToWorld(topLayerCellPosition);

        // Convert the world position to cell space in the bottom layer
        Vector3Int bottomLayerCellPosition = bottomLayerTilemap.WorldToCell(worldPosition);

        // Get the tile from the bottom layer
        return bottomLayerTilemap.GetTile(bottomLayerCellPosition);
    }*/

    void SpawnEnemy()
    {
        //DETERMINING THE VALID BOUNDS OF THE MAP THE ENEMY CAN SPAWN IN
        Vector3Int environmentMapSize = tileManager.tileMap.size;
        Vector3Int excludeBorder = new Vector3Int(1, 1, 0);

        Vector3Int validSpawnBounds = environmentMapSize - excludeBorder + tileManager.offset;        

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
