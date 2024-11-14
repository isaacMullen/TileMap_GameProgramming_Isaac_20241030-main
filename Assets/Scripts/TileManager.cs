using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using System.Text;
using System.Runtime.InteropServices;
using Unity.Collections;


public class TileManager : MonoBehaviour
{

    public TextMeshProUGUI welcomeText;
    bool hasPressedAnyKey;

    public TextMeshProUGUI fishText;
    private int fishCount = 0;

    string file = "Assets/TextFiles/MapTextFile.txt";
    string sampleFile = "Assets/TextFiles/SampleInput.txt";     
    
    public Tilemap tileMap;
    public Tilemap playerMap;

    private Vector3Int playerPosition;
    
    //Tiles to be spawned
    public TileBase borderBase;


    public TileBase seaBase;
    public List<TileBase> seaBases = new List<TileBase>();

    public TileBase rockBase;
    public TileBase cornerBase;
    public TileBase smallFish;
    public TileBase defaultTile;

    //Player
    public TileBase playerBase;

    int rowIndex;
    int columnIndex;

    public int columnLength;
    public int rowLength;

    public Vector3Int offset = new Vector3Int(0, 0, 0);
    public Vector2 gridOrigin = Vector2.zero;  // Bottom-left corner of the grid
    
    public float cellSize = 1f;

    private readonly Matrix4x4 flipMatrix = Matrix4x4.Scale(new Vector3(-1, 1, 1));
    private readonly Matrix4x4 normalMatrix = Matrix4x4.identity;
    bool facingRight = true;

    //Vector3Int newPosition = new Vector3Int(0, 0, 0);

    void Collect(TileBase tile)
    {
        TileBase currentTile = tileMap.GetTile(playerPosition);

        if(currentTile == tile)
        {
            Debug.Log("DETECTED IN ARRAY");
            fishCount++;
            ReplaceTile(tileMap, playerPosition, seaBase);
            SetText(fishText, $"Fish: {fishCount}", true);    

        }
    }

    void SetText(TextMeshProUGUI text, string contents, bool toggle)
    {
        if(toggle)
        {
            text.SetText(contents);
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

    void Start()
    {              
        string mapData = GenerateMapString(file, 25, 10);
        string sampleInputData = File.ReadAllText(sampleFile);

        //CHANGE TO SAMPLE_INPUT_DATA OR CHANGE SAMPLE_FILE ITSELF(CLASS LEVEL) TO TEST DIFFERENT MAP DATA
        ConvertMapToTileMap(sampleInputData);

        playerPosition = new Vector3Int(0, 0, 0);

        ReplaceTile(playerMap, playerPosition, playerBase);

        SetText(welcomeText, "Collect The Fish!", true);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKeyDown && !hasPressedAnyKey)
        {
            //Disables itself
            SetText(welcomeText, welcomeText.ToString(), false);
            hasPressedAnyKey = true;
        }

        Collect(smallFish);
        //Getting the mouse position as a Vector3INT
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 gridPosition = mouseWorldPos - (Vector3)gridOrigin;

        int column = Mathf.FloorToInt(gridPosition.x / cellSize);
        int row = Mathf.FloorToInt(gridPosition.y / cellSize);

        //Debug.Log($"Column: {column}, Row: {row}");

        if (Input.GetKeyDown(KeyCode.W))
        {
            MovePlayer(Vector3Int.up);  // Move up
            FlipTile(facingRight, playerPosition);  // Keep the same facing direction
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            MovePlayer(Vector3Int.down);  // Move down
            FlipTile(facingRight, playerPosition);  // Keep the same facing direction
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            facingRight = false;  // Update facing direction to left
            MovePlayer(Vector3Int.left);  // Move left
            FlipTile(facingRight, playerPosition);  // Flip sprite horizontally
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            facingRight = true;  // Update facing direction to right
            MovePlayer(Vector3Int.right);  // Move right
            FlipTile(facingRight, playerPosition);  // Flip sprite horizontally
        }


    }

    void ReplaceTile(Tilemap map, Vector3Int tileToReplace, TileBase tile)
    {
        map.SetTile(tileToReplace, tile);
    }

    bool IsValidMove(Vector3Int position)
    {
        TileBase tileAtPosition = tileMap.GetTile(position);

        Debug.Log(tileAtPosition);

        if (tileAtPosition == seaBase)
        {
            return true;
        }
        else if (tileAtPosition == borderBase)
        {
            return false;
        }
        else if(tileAtPosition == rockBase)
        {
            return false;
        }
        return true;
    }

    Vector3Int MovePlayer(Vector3Int direction)
    {
        Vector3Int newPosition = playerPosition + direction;
        


        if(IsValidMove(newPosition))
        {
            TileBase tileToReplace = tileMap.GetTile(playerPosition);

            playerMap.SetTransformMatrix(playerPosition, Matrix4x4.identity);

            ReplaceTile(playerMap, playerPosition, tileToReplace);
            
            playerPosition = newPosition;

            ReplaceTile(playerMap, playerPosition, playerBase);

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

    string GenerateMapString(string filename, int width, int height)
    {

        StringBuilder mapData = new StringBuilder();
        using StreamWriter writer = new StreamWriter(file);
        //Each row
        for(int y = 0; y < height; y++)
        {
            Debug.Log(y);
            //Random Rock
            int randomRock = UnityEngine.Random.Range(1, width - 1);
            int randomPickup = UnityEngine.Random.Range(1, width - 1);
            //Each column
            for(int x = 0; x < width; x++)
            {                
                //Checking corners
                if((x == 1 && y == 1) || (x == 1 && y == height - 2) || (x == width - 2 && y == 1) || (x == width - 2 && y == height - 2))
                {
                    //Chests
                    int randomCorner = UnityEngine.Random.Range(0, 2);                    
                    
                    if(randomCorner == 1)
                    {
                        writer.Write('C');
                        mapData.Append('C');
                    }
                    else
                    {
                        writer.Write('~');
                        mapData.Append('~');
                    }
                    
                }
                //Rocks
                else if(x == randomRock && y < height - 1 && y != 0)
                {
                    writer.Write('R');
                    mapData.Append('R');
                }
                //Pickups
                else if (x == randomPickup && y < height - 1 && y != 0 && x != 'R')
                {
                    writer.Write('P');
                    mapData.Append('P');
                }
                //Border check
                else if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    writer.Write('#');
                    mapData.Append('#');
                }
                else
                {
                    writer.Write('~');
                    mapData.Append('~');
                }
                
            }
            writer.WriteLine();
            mapData.AppendLine();
        }
        return mapData.ToString();     
    }    


    char[,] ConvertMapToTileMap(string data)
    {

        data = data.Replace(' ', '~');

    string[] lines = data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);



    int height = lines.Length;
    int width = lines[0].Length;


    char[,] grid = new char[height, width];
        
    for (int i = 0; i < height; i++)
    {
        for(int t = 0; t < width; t++)
        {
            grid[i, t] = lines[i][t];

            Vector3Int tilePosition = offset + new Vector3Int(t, i, 0);

            Debug.Log(grid[i, t].ToString());

            switch(grid[i, t])
            {
                case 'C' or 'O':
                    ReplaceTile(tileMap, tilePosition, cornerBase);
                    break;
                case 'R' or '*':
                    ReplaceTile(tileMap, tilePosition, rockBase);
                    break;
                case '#':
                    ReplaceTile(tileMap, tilePosition, borderBase);
                    break;
                //I needed variation in my base ground tiles so I seperated defaultTiles for your sample input with my own list of tiles
                case '~':
                    TileBase seaBaseToDraw = seaBases[UnityEngine.Random.Range(0, seaBases.Count)];
                    Debug.Log(UnityEngine.Random.Range(0, seaBases.Count));
                    ReplaceTile(tileMap, tilePosition, seaBaseToDraw);
                    break;
                case 'P' or '$':
                    ReplaceTile(tileMap, tilePosition, smallFish);
                    break;
                case ' ':
                    ReplaceTile(tileMap, tilePosition, seaBase);
                    break;
                default:
                    ReplaceTile(tileMap, tilePosition, defaultTile);
                    break;

            }                    
        }
    }                                
        return grid;
    }    
}
