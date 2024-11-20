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
    public PlayerController playerController;

    public string file = "Assets/TextFiles/MapTextFile.txt";
    public string sampleFile = "Assets/TextFiles/SampleInput.txt";     
    
    public Tilemap tileMap;       
    
    //Tiles to be spawned
    public TileBase borderBase;


    public TileBase seaBase;
    public List<TileBase> seaBases = new List<TileBase>();

    public TileBase rockBase;
    public TileBase cornerBase;
    public TileBase smallFish;
    public TileBase defaultTile;

    //Player      
    public int columnLength;
    public int rowLength;

    public Vector3Int offset = new Vector3Int(0, 0, 0);
    public Vector2 gridOrigin = Vector2.zero;  // Bottom-left corner of the grid
    
    public float cellSize = 1f;
   
    void Start()
    {       

        string mapData = GenerateMapString(file, 25, 10);
        string sampleInputData = File.ReadAllText(sampleFile);

        //CHANGE TO SAMPLE_INPUT_DATA OR CHANGE SAMPLE_FILE ITSELF(CLASS LEVEL) TO TEST DIFFERENT MAP DATA
        ConvertMapToTileMap(mapData);        
    }

    // Update is called once per frame
    void Update()
    {        
        //Getting the mouse position as a Vector3INT
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 gridPosition = mouseWorldPos - (Vector3)gridOrigin;

        int column = Mathf.FloorToInt(gridPosition.x / cellSize);
        int row = Mathf.FloorToInt(gridPosition.y / cellSize);        
    }

    public void ReplaceTile(Tilemap map, Vector3Int tileToReplace, TileBase tile)
    {
        map.SetTile(tileToReplace, tile);
    }    

    public string GenerateMapString(string filename, int width, int height)
    {

        StringBuilder mapData = new StringBuilder();
        using StreamWriter writer = new StreamWriter(filename);
        //Each row
        for(int y = 0; y < height; y++)
        {
            Debug.Log(y);
            //Random Rock
            int randomRock = UnityEngine.Random.Range(1, width - 1);
            int fishPosition = UnityEngine.Random.Range(1, width - 1);
            //Each column
            for(int x = 0; x < width; x++)
            {                
                //Checking corners                
                if ((x == 1 && y == 1) || (x == 1 && y == height - 2) || (x == width - 2 && y == 1) || (x == width - 2 && y == height - 2))
                {
                    //50% chance of placing each corner
                    int randomCorner = UnityEngine.Random.Range(0, 2);

                    if (randomCorner == 1)
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
                //Rocks (if inside the map and not the top or bottom border)
                else if (x == randomRock && y < height - 1 && y != 0 && (x != 5 && y != 12))
                {
                    writer.Write('R');
                    mapData.Append('R');
                }
                //FISH (if inside the bounds of the map and not the players start position)
                else if (x == fishPosition && y < height - 1 && y != 0 && x != 'R' && (x != 5 && y != 12))
                {
                    writer.Write('F');
                    mapData.Append('F');
                    //Tracking total spawned Fish
                    
                }
                //Otherwise write border
                else if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    writer.Write('#');
                    mapData.Append('#');
                }
                //otherwise write ground tile
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


    public char[,] ConvertMapToTileMap(string data)
    {


        data = data.Replace(' ', '~');

        string[] lines = data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        data = data.Trim();

        int height = lines.Length;
        int width = lines[0].Length - 1;


        char[,] grid = new char[height, width];
        
    
        foreach(var line in lines)
        {
            Debug.Log(line);
            Debug.Log(line.Length);
        }
        for (int i = 0; i < height; i++)
        {
            
            for(int t = 0; t < width; t++)
            {                
                grid[i, t] = lines[i][t];

                Vector3Int tilePosition = offset + new Vector3Int(t, i, 0);
                
                if (i == 4)
                {
                    Debug.Log($"Row: {i}, Col: {t}, Tile: {grid[i, t]}, Position: {tilePosition}");
                }
                //Debug.Log(grid[i, t].ToString());Debug.Log($"Data: {data}");


                switch (grid[i, t])
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
                    case 'F' or '$':
                        ReplaceTile(tileMap, tilePosition, smallFish);
                        playerController.fishToCollect += 1;
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
