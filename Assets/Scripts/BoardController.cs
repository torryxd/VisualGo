using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public int size;
    public TileType turnType;

    [SerializeField]
    private Tile _tilePrefab;
    [SerializeField]
    private Transform _cam;

    public Dictionary<Vector2, Tile> tiles;


    private void Start()
    {
        GenerateGrid();

        turnType = TileType.Black;
    }

    private void GenerateGrid()
    {
        tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x <= (size + 1); x++)
        {
            for (int y = 0; y <= (size + 1); y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile[{NumberToLetter(x)},{NumberToLetter(y)}]";

                Vector2 boardPosition = new Vector2(x, y);
                tiles[boardPosition] = spawnedTile;

                spawnedTile.Init(this, boardPosition);
            }
        }

        _cam.transform.position = new Vector3((float)size / 2 + 0.5f, (float)size / 2 + 0.5f, -10);
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (tiles.TryGetValue(pos, out var tile))
            return tile;
        return null;
    }

    public char NumberToLetter(int i)
    {
        return Convert.ToChar(64 + i);
    }

    public void NextTurn()
    {
        if(turnType == TileType.Black)
        {
            turnType = TileType.White;
        }
        else if(turnType == TileType.White)
        {
            turnType = TileType.Black;
        }
    }
}