using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.IO;


public class PlayerController : MonoBehaviour
{
    string file;
    
    public TextMeshProUGUI combatInstructions;
    
    public UIHandler UIhandler;
    
    public int randomWidth;
    public int randomHeight;

    public Camera mainCamera;
    
    bool startWaving = true;
    
    public FishTracker fishTracker;
    
    //public int autoAttack = UnityEngine.Random.Range(0, 0);
    public bool isLoading = false;
    public Tilemap enemy;
    
    public GameObject combatPanel;
    public GameObject overWorldPanel;
    
    public TextMeshProUGUI healthText;
    public float health;
    public Slider healthBar;    
    public float maxHealth;    
    
    public EnemyController enemyController;
   
    public bool inCombat;
    int turn = 0;
    bool acceptingInput = true;
    
    public int fishCount = 0;
    public int fishToCollect;

    public TextMeshProUGUI fishText;
    public TextMeshProUGUI centerText;
    public TextMeshProUGUI turnText;
    
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
        FitCameraToMapSize();

        file = Path.Combine(Application.streamingAssetsPath, "MapTextFile.txt");

        SetText(combatInstructions, "<color=green>Auto Attack:</color> Press 1\n<color=orange>Special Attack:</color> Press 2", true);

        combatPanel.SetActive(false);
        
        SetText(centerText, "Survive as long as you can.\r\nCatch as many fish as you can.\r\nGoodLuck!", true);
        
        enemyController.enemyHealthText.enabled = false;

        healthBar.maxValue = maxHealth;
        health = maxHealth;

        SetText(healthText, health.ToString(), true);
    }

    // Update is called once per frame
    void Update()
    {                        
        
        
        if(startWaving)
        {
            StartCoroutine(tileManager.SimulateWaves());
        }
        
        //HEALTH BAR IS UPDATING TO REPRESENT CURRENT HEALTH
        healthBar.value = health;        
                               
        if (fishCount == fishToCollect)
        {
            Debug.Log("COLLECTED ALL FISH");
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
            //UpdateHealthText();
        }      
        //BOOL EXPRESSION TO STOP THE COROUTINE FROM BEING CALLED EACH FRAME
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
            SetText(enemyController.enemyHealthText, enemyController.enemyHealth.ToString(), true);            
        }
        else
        {            
            enemyController.enemyHealthText.enabled = false;
        }                
    }

    void EndCombatCondition()
    {
        if(health <= 0)
        {
            Debug.Log("You Died");            
            inCombat = false;

            UIhandler.EndGameDisplayUI();


        }
        else if(enemyController.enemyHealth <= 0)
        {
            Debug.Log($"Enemy Died | Enemy Health {enemyController.enemyHealth}");
            inCombat = false;            

            enemyController.defeated = true;

            combatPanel.SetActive(false);
            overWorldPanel.SetActive(true);

            enemyController.enemyCount--;

            
        }        
    }
    
    bool combatRoutineRunning = false;
    void StartCombat()
    {
        overWorldPanel.SetActive(false);
        combatPanel.SetActive(true);

        
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
                SetText(turnText, "<color=red>Enemy</color> Turn", true);

                EndCombatCondition();                

                int randomAttackValue = UnityEngine.Random.Range(4, 6);
                
                health -= randomAttackValue;
                health = Mathf.Clamp(health, 0 , maxHealth);

                UpdateHealthText();

                Debug.Log($"Enemy Attacked for {randomAttackValue} damage!");
                yield return new WaitForSeconds(1);
                SetText(turnText, "<color=green>Your</color> Turn", true);
                playerTurn = true;
            }



            if (playerTurn)
            {                
                EndCombatCondition();
                Debug.Log("Your turn, Press 1 to attack.");
                
                yield return PlayerAttackAction();
                
                SetText(turnText, "<color=red>Enemy</color> Turn", true);
                yield return new WaitForSeconds(1);
                
                turn++;
                playerTurn = false;
            }            
        }        
        combatRoutineRunning = false;
    }
        
    IEnumerator PlayerAttackAction()
    {
        bool actionTaken = false;
        while(!actionTaken)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {                
                int randomAttackValue = UnityEngine.Random.Range(8, 14);
                
                enemyController.enemyHealth -= randomAttackValue;
                enemyController.enemyHealth = Mathf.Clamp(enemyController.enemyHealth, 0, enemyController.enemyMaxHealth);
                UpdateHealthText();
                EndCombatCondition();
                
                Debug.Log($"Player Auto Attacked for {randomAttackValue} damage.");
                actionTaken = true;                
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                int randomAttackValue = UnityEngine.Random.Range(20, 25);

                enemyController.enemyHealth -= randomAttackValue;
                enemyController.enemyHealth = Mathf.Clamp(enemyController.enemyHealth, 0, enemyController.enemyMaxHealth);
                UpdateHealthText();
                EndCombatCondition();

                Debug.Log($"Player Special Attacked for {randomAttackValue} damage.");
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

    
    
    IEnumerator ReloadMap()
    {
        if (isLoading) yield break;
                
        //SETTING NEW RANDOM DIMENSIONS
        randomWidth = UnityEngine.Random.Range(7, 15);
        randomHeight = UnityEngine.Random.Range(7, 15);

        Debug.Log("INSIDE RELOADMAP");

        isLoading = true;

        acceptingInput = false;       
        SetText(centerText, "You Win!", true);        

        //STOPS CALLING THE COROUTINE
        tileManager.seaTiles.Clear();
        startWaving = false;

        yield return new WaitForSeconds(2);

        centerText.enabled = false;

        tileManager.tileMap.ClearAllTiles();

        //Regenerating map... And resetting win condition.
        fishCount = 0;
        fishToCollect = 0;

        file = Path.Combine(Application.streamingAssetsPath, "MapTextFile.txt");
        string mapData = tileManager.GenerateMapString(file, randomWidth, randomHeight);
        
        tileManager.ConvertMapToTileMap(mapData);        
        
        acceptingInput = true;
        
        enemyController.SpawnEnemy();

        enemyController.moveInterval -= .2f;

        FitCameraToMapSize();
        startWaving = true;
        isLoading = false;
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
            playerMap.SetTransformMatrix(playerPosition, Matrix4x4.identity);

            //CLEARING THE PLAYER MAP EVERY TIME HE MOVES SO IT DOESN'T HAVE TO DRAW TILES TO OVERRIDE THE PREVIOUS SHARK POSITION
            //THIS WAS CAUSING A BUG WHERE WHEN I REGENERATE THE MAP IT WAS HIDING SOME ENVRIONMENT TILES BELOW THE PLAYERMAP THAT WAS GENERATED 
            //AS THE PLAYER MOVED (!!!AWESOME SIMPLE FIX!!!)
            playerMap.ClearAllTiles();            

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
            fishTracker.totalFish++;
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

    public void FitCameraToMapSize()
    {
        Bounds bounds = tileManager.tileMap.localBounds;

        float mapWidth = bounds.size.x;
        float mapHeight= bounds.size.y;

        mainCamera.orthographicSize = mapHeight / 2f;

        // Calculate the screen's aspect ratio (width-to-height ratio of the display)
        float aspectRatio = (float)Screen.width / Screen.height;

        // Check if the width constraint is more limiting than the height constraint
        if (mapWidth / aspectRatio > mapHeight)
        {
            // Adjust orthographic size to ensure the width fits within the camera's view
            mainCamera.orthographicSize = ((mapWidth + 4f) / 2f) / aspectRatio;
        }
        mainCamera.transform.position = new Vector3(bounds.center.x + .5f, bounds.center.y, mainCamera.transform.position.z);

        playerPosition = playerMap.WorldToCell(new Vector3(bounds.center.x, bounds.center.y, mainCamera.transform.position.z)) - new Vector3Int(1, 0, 0);

        tileManager.ReplaceTile(playerMap, playerPosition, playerBase);
    }    
}
