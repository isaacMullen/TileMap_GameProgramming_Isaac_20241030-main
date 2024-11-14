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

    string file = "Assets/TextFiles/MapTextFile.txt";    
    
    public Tilemap tileMap;
    public Tilemap playerMap;

    private Vector3Int playerPosition;
    
    //Tiles to be spawned
    public TileBase borderBase;
    public TileBase seaBase;
    public TileBase rockBase;
    public TileBase cornerBase;

    //Player
    public TileBase playerBase;

    int rowIndex;
    int columnIndex;

    public int columnLength;
    public int rowLength;

    public Vector3Int offset = new Vector3Int(0, -5, 0);
    public Vector2 gridOrigin = Vector2.zero;  // Bottom-left corner of the grid
    
    public float cellSize = 1f;   



    void Start()
    {              
        string mapData = GenerateMapString(file, 25, 10);

        ConvertMapToTileMap(mapData);

        Vector3Int playerPosition = new Vector3Int(0, 0, 0);

        ReplaceTile(playerMap, playerPosition, playerBase);


    }

    // Update is called once per frame
    void Update()
    {
        //Getting the mouse position as a Vector3INT
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 gridPosition = mouseWorldPos - (Vector3)gridOrigin;

        int column = Mathf.FloorToInt(gridPosition.x / cellSize);
        int row = Mathf.FloorToInt(gridPosition.y / cellSize);

        //Debug.Log($"Column: {column}, Row: {row}");

        if (Input.GetKeyDown(KeyCode.W))
        {
            MovePlayer(Vector3Int.up);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            MovePlayer(Vector3Int.down);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            MovePlayer(Vector3Int.left);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            MovePlayer(Vector3Int.right);
        }


    }

    void ReplaceTile(Tilemap map, Vector3Int tileToReplace, TileBase tile)
    {
        map.SetTile(tileToReplace, tile);
    }

    bool IsValidMove(Vector3Int position)
    {
            

        Vector3Int gridPosition = position;

        

        TileBase tileAtPosition = tileMap.GetTile(gridPosition);

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
        else if (tileAtPosition == cornerBase)
        {
            return false;
        }
        return true;
    }

    void MovePlayer(Vector3Int direction)
    {
        Vector3Int newPosition = playerPosition + direction;
        


        if(IsValidMove(newPosition))
        {
            TileBase tileToReplace = tileMap.GetTile(playerPosition);

            ReplaceTile(playerMap, playerPosition, tileToReplace);
            
            playerPosition = newPosition;

            ReplaceTile(playerMap, playerPosition, playerBase);
        }
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
            //Each column
            for(int x = 0; x < width; x++)
            {                
                if((x == 1 && y == 1) || (x == 1 && y == height - 2) || (x == width - 2 && y == 1) || (x == width - 2 && y == height - 2))
                {
                    //Chests
                    int randomChest = UnityEngine.Random.Range(0, 2);                    
                    
                    if(randomChest == 1)
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

            //Debug.Log(grid[i, t].ToString());

            switch(grid[i, t])
            {
                case 'C':
                    ReplaceTile(tileMap, tilePosition, cornerBase);
                    break;
                case 'R':
                    ReplaceTile(tileMap, tilePosition, rockBase);
                    break;
                case '#':
                    ReplaceTile(tileMap, tilePosition, borderBase);
                    break;
                case '~':
                    ReplaceTile(tileMap, tilePosition, seaBase);
                    break;
            }                    
        }
    }                                
        return grid;
    }    
}
