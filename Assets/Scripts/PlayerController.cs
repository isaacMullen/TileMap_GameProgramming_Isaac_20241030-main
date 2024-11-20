using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    bool acceptingInput = true;
    
    public int fishCount = 0;
    public int fishToCollect;

    public TextMeshProUGUI fishText;
    public TextMeshProUGUI centerText;
    bool hasPressedAnyKey;

    public TileManager tileManager;

    private Vector3Int playerPosition;
    public Tilemap playerMap;

    public TileBase playerBase;

    bool facingRight = true;
    private readonly Matrix4x4 flipMatrix = Matrix4x4.Scale(new Vector3(-1, 1, 1));
    private readonly Matrix4x4 normalMatrix = Matrix4x4.identity;

    // Start is called before the first frame update
    void Start()
    {
        playerPosition = new Vector3Int(0, 0, 0);

        //Refernce TileManager Script
        tileManager.ReplaceTile(playerMap, playerPosition, playerBase);

        SetText(centerText, "Collect The Fish!", true);
    }

    // Update is called once per frame
    void Update()
    {
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

        if (Input.GetKeyDown(KeyCode.W) && acceptingInput)
        {
            MovePlayer(Vector3Int.up);  // Move up
            FlipTile(facingRight, playerPosition);  // Keep the same facing direction
        }
        if (Input.GetKeyDown(KeyCode.S) && acceptingInput)
        {
            MovePlayer(Vector3Int.down);  // Move down
            FlipTile(facingRight, playerPosition);  // Keep the same facing direction
        }
        if (Input.GetKeyDown(KeyCode.A) && acceptingInput)
        {
            facingRight = false;  // Update facing direction to left
            MovePlayer(Vector3Int.left);  // Move left
            FlipTile(facingRight, playerPosition);  // Flip sprite horizontally
        }
        if (Input.GetKeyDown(KeyCode.D) && acceptingInput)
        {
            facingRight = true;  // Update facing direction to right
            MovePlayer(Vector3Int.right);  // Move right
            FlipTile(facingRight, playerPosition);  // Flip sprite horizontally
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

        Debug.Log(tileAtPosition);

        if (tileAtPosition == tileManager.seaBase)
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
        return true;
    }

    Vector3Int MovePlayer(Vector3Int direction)
    {
        Vector3Int newPosition = playerPosition + direction;



        if (IsValidMove(newPosition))
        {
            TileBase tileToReplace = tileManager.tileMap.GetTile(playerPosition);

            playerMap.SetTransformMatrix(playerPosition, Matrix4x4.identity);

            tileManager.ReplaceTile(playerMap, playerPosition, tileToReplace);

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
