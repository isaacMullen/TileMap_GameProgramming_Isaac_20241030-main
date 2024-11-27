using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public float health;
    public Slider healthBar;    
    public float maxHealth;    
    
    public EnemyController enemyController;

    

    bool inCombat;
    int turn = 0;
    bool acceptingInput = true;
    
    public int fishCount = 0;
    public int fishToCollect;

    public TextMeshProUGUI fishText;
    public TextMeshProUGUI centerText;
    bool hasPressedAnyKey;

    public TileManager tileManager;

    public Vector3Int playerPosition;
    public Tilemap playerMap;


    public TileBase playerBase;

    bool facingRight = true;
    private readonly Matrix4x4 flipMatrix = Matrix4x4.Scale(new Vector3(-1, 1, 1));
    private readonly Matrix4x4 normalMatrix = Matrix4x4.identity;

    // Start is called before the first frame update
    void Start()
    {        
        SetText(centerText, "Collect The Fish!", true);

        healthText.enabled = false;
        enemyController.enemyHealthText.enabled = false;

        healthBar.maxValue = maxHealth;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
        //HEALTH BAR IS UPDATING TO REPRESENT CURRENT HEALTH
        healthBar.value = health;
        Mathf.Clamp(health, 0, maxHealth);
        
                
        
        if (fishCount == fishToCollect)
        {
            StartCoroutine(ReloadMap());
        }

        if (Input.anyKeyDown && !hasPressedAnyKey)
        {
            //Disables itself
            SetText(centerText, centerText.ToString(), false);
            hasPressedAnyKey = true;
        }

        Collect(tileManager.smallFish);
        
        if(!inCombat)
        {
            HandlePlayerInput();
            UpdateHealthText();
        }      
        else if (!combatRoutineRunning)
        {
            StartCombat();                     
        }
    }

   
    void UpdateHealthText()
    {
        if(inCombat)
        {
            SetText(healthText, health.ToString(), true);
            SetText(enemyController.enemyHealthText, $"Enemy HP: {enemyController.enemyHealth}", true);            
        }
        else
        {
            healthText.enabled = false;
            enemyController.enemyHealthText.enabled = false;
        }
        
        
    }

    void EndCombatCondition()
    {
        if(health <= 0)
        {
            Debug.Log("You Died");            
            inCombat = false;
        }
        else if(enemyController.enemyHealth <= 0)
        {
            Debug.Log("Enemy Died");
            inCombat = false;
        }
    }
    
    bool combatRoutineRunning = false;
    void StartCombat()
    {
        combatRoutineRunning = true;
        StartCoroutine(CombatRoutine());                
    }
    

    IEnumerator CombatRoutine()
    {                                                
        bool playerTurn = false;

        Debug.Log($"Starting Turn {turn}");
        while(inCombat)
        {
            if (!playerTurn)
            {
                int randomAttackValue = Random.Range(3, 8);
                
                health -= randomAttackValue;
                UpdateHealthText();

                Debug.Log($"Enemy Attacked for {randomAttackValue} damage!");
                yield return new WaitForSeconds(1);
                playerTurn = true;
            }



            if (playerTurn)
            {
                Debug.Log("Your turn, Press 1 to attack.");
                yield return WaitForPlayerAction();
                Debug.Log("Player Attacked the enemy!");


                yield return new WaitForSeconds(2);
                turn++;
                playerTurn = false;
            }            
        }        
        combatRoutineRunning = false;
    }
        
    IEnumerator WaitForPlayerAction()
    {
        bool actionTaken = false;
        while(!actionTaken)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                actionTaken = true;
            }
            
            yield return null;
        }
    }

    private void HandlePlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && acceptingInput)
        {
            MovePlayer(Vector3Int.up);  // Move up
            FlipTile(facingRight, playerPosition);  // Keep the same facing direction
            //Debug.Log(playerPosition);
        }
        if (Input.GetKeyDown(KeyCode.S) && acceptingInput)
        {
            MovePlayer(Vector3Int.down);  // Move down
            FlipTile(facingRight, playerPosition);  // Keep the same facing direction
            //Debug.Log(playerPosition);
        }
        if (Input.GetKeyDown(KeyCode.A) && acceptingInput)
        {
            facingRight = false;  // Update facing direction to left
            MovePlayer(Vector3Int.left);  // Move left
            FlipTile(facingRight, playerPosition);  // Flip sprite horizontally
            //Debug.Log(playerPosition);
        }
        if (Input.GetKeyDown(KeyCode.D) && acceptingInput)
        {
            facingRight = true;  // Update facing direction to right
            MovePlayer(Vector3Int.right);  // Move right
            FlipTile(facingRight, playerPosition);  // Flip sprite horizontally
            //Debug.Log(playerPosition);
        }
    }

    bool isLoading = false;
    
    IEnumerator ReloadMap()
    {
        if (isLoading) yield break;

        isLoading = true;

        acceptingInput = false;
        Debug.Log("You Win!");
        SetText(centerText, "You Win!", true);

        yield return new WaitForSeconds(2);

        tileManager.tileMap.ClearAllTiles();

        //Regenerating map... And resetting win condition.
        fishCount = 0;
        fishToCollect = 0;
        string mapData = tileManager.GenerateMapString(tileManager.file, 25, 10);
        tileManager.ConvertMapToTileMap(mapData);
        acceptingInput = true;                
    }
    
    bool IsValidMove(Vector3Int position)
    {
        TileBase tileAtPosition = tileManager.tileMap.GetTile(position);
        TileBase enemyAtPosition = enemyController.enemyTileMap.GetTile(position);

        //Debug.Log(tileAtPosition);

        if (tileAtPosition == tileManager.seaBase && enemyAtPosition != enemyController.enemyTileBase)
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
        else if(enemyAtPosition == enemyController.enemyTileBase)
        {
            inCombat = true;
            Debug.Log("Combat Started");
            return false;
        }
        return true;
    }

    Vector3Int MovePlayer(Vector3Int direction)
    {
        Vector3Int newPosition = playerPosition + direction;


        if (IsValidMove(newPosition))
        {
            TileBase tileToReplace = tileManager.tileMap.GetTile(playerPosition);

            playerMap.SetTransformMatrix(playerPosition, Matrix4x4.identity);

            //CLEARING THE PLAYER MAP EVERY TIME HE MOVES SO IT DOESN'T HAVE TO DRAW TILES TO OVERRIDE THE PREVIOUS SHARK POSITION
            //THIS WAS CAUSING A BUG WHERE WHEN I REGENERATE THE MAP IT WAS HIDING SOME ENVRIONMENT TILES BELOW THE PLAYERMAP THAT WAS GENERATED 
            //AS THE PLAYER MOVED (!!!AWESOME SIMPLE FIX!!!)
            playerMap.ClearAllTiles();

            tileManager.ReplaceTile(tileManager.tileMap, playerPosition, tileToReplace);

            playerPosition = newPosition;

            tileManager.ReplaceTile(playerMap, playerPosition, playerBase);

            playerMap.SetTransformMatrix(playerPosition, Matrix4x4.identity);

            if (direction == Vector3Int.left)
            {
                facingRight = true;
                FlipTile(facingRight, playerPosition);
            }
            else if (direction == Vector3Int.right)
            {
                facingRight = false;
                FlipTile(facingRight, playerPosition);
            }
        }

        playerMap.SetTransformMatrix(newPosition, Matrix4x4.identity);
        return newPosition;
    }

    void Collect(TileBase tile)
    {
        TileBase currentTile = tileManager.tileMap.GetTile(playerPosition);

        if (currentTile == tile)
        {
            Debug.Log("DETECTED IN ARRAY");
            fishCount++;
            tileManager.ReplaceTile(tileManager.tileMap, playerPosition, tileManager.seaBase);
            SetText(fishText, $"Fish: {fishCount}", true);

        }
    }

    void SetText(TextMeshProUGUI text, string contents, bool toggle)
    {
        text.SetText(contents);
        
        if (toggle)
        {
            text.enabled = true;
        }
        else
        {
            text.enabled = false;
        }
    }

    void FlipTile(bool facingRight, Vector3Int position)
    {

        Matrix4x4 matrixToUse = facingRight ? flipMatrix : normalMatrix;
        playerMap.SetTransformMatrix(position, matrixToUse);

    }

    
}
