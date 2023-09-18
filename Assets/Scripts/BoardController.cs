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

    private Dictionary<Vector2, Tile> tiles;
    private List<Tile> turnCapturedTiles = new List<Tile>();

    //private List<Dictionary<Vector2, TileType>> tileMaps = new List<Dictionary<Vector2, TileType>>();
    Dictionary<Vector2, TileType> tileMap = new Dictionary<Vector2, TileType>();


    private void Start()
    {
        CreateBoard();

        turnType = TileType.Black;
    }

    private void Update()
    {
        
    }

    private void CreateBoard()
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

    public char NumberToLetter(int i)
    {
        return Convert.ToChar(64 + i);
    }

    public void NextTurn()
    {
        if (turnType == TileType.Black)
        {
            turnType = TileType.White;
        }
        else if (turnType == TileType.White)
        {
            turnType = TileType.Black;
        }
    }

    public void ClickTile(Tile tile)
    {
        PlaceAndDrawStone(tile);
    }

    private void PlaceAndDrawStone(Tile tile)
    {
        string placeStoneError = PlaceStone(tile);

        if (placeStoneError == string.Empty)
        {
            //CAMBIAR SPRITES
            tile.ChangeType(turnType);
        
            //CAPTURED STONES
            if(turnCapturedTiles?.Count > 0)
            {
                foreach(Tile t in turnCapturedTiles)
                {
                    t.ChangeType(TileType.Liberty);
                }
            }
        }
        else
        {
            Debug.Log(placeStoneError);
        }
        
        turnCapturedTiles?.Clear();
    }

    public string PlaceStone(Tile tile)
    {
        Dictionary<Vector2, TileType> lastTileMap = tileMap;

        Dictionary<Vector2, TileType> actualTileMap = new Dictionary<Vector2, TileType>();
        foreach(KeyValuePair<Vector2, Tile> t in tiles)
        {
            actualTileMap[t.Key] = t.Value.type;
        }

        if(tile.type != TileType.Liberty)
            return "You need to place a stone on a empty space!";

        tile.type = turnType;

        Tile[] adjacentTiles =
        {
            tiles[new Vector2(tile.boardPos.x, tile.boardPos.y + 1)], // UP
            tiles[new Vector2(tile.boardPos.x + 1, tile.boardPos.y)], // RIGHT
            tiles[new Vector2(tile.boardPos.x, tile.boardPos.y - 1)], // DOWN
            tiles[new Vector2(tile.boardPos.x - 1, tile.boardPos.y)]  // LEFT
        };
        foreach(Tile t in adjacentTiles)
        {
            if((turnType == TileType.White && t.type == TileType.Black)
            || (turnType == TileType.Black && t.type == TileType.White))
            {
                TilesHaveLiberties(t, t.type);
            }
        }
        
        if (turnCapturedTiles?.Count > 0 || TilesHaveLiberties(tile, turnType)) // Si se captura alguna alrededor o tiene espacio
        {
            Dictionary<Vector2, TileType> newTileMap = new Dictionary<Vector2, TileType>();
            foreach(KeyValuePair<Vector2, Tile> t in tiles)
            {
                newTileMap[t.Key] = turnCapturedTiles.Contains(tiles[t.Key]) ? TileType.Liberty : t.Value.type;
            }

            bool equalTileMaps = true;
            foreach(KeyValuePair<Vector2, TileType> t in newTileMap)
            {
                if(lastTileMap.ContainsKey(t.Key))
                {
                    if(t.Value != lastTileMap[t.Key])
                    {
                        equalTileMaps = false;
                        break;
                    }
                }
                else
                {
                    equalTileMaps = false;
                    break;
                }
            }

            if(equalTileMaps) // KO RULE
            {
                tile.type = TileType.Liberty;
                return "KO: You can't repeat a previous position!";
            }
            else
            {
                tileMap = actualTileMap;
                Debug.Log("Placed stone");
                
                return string.Empty;
            }
        }
        else
        {
            tile.type = TileType.Liberty;
            return "Suicidal moves are not permitted!";
        }
    }

    public bool TileHasLiberty(Tile tile)
    {
        bool hasLiberty = false;

        Tile[] adjacentTiles =
        {
            tiles[new Vector2(tile.boardPos.x, tile.boardPos.y + 1)], // UP
            tiles[new Vector2(tile.boardPos.x + 1, tile.boardPos.y)], // RIGHT
            tiles[new Vector2(tile.boardPos.x, tile.boardPos.y - 1)], // DOWN
            tiles[new Vector2(tile.boardPos.x - 1, tile.boardPos.y)]  // LEFT
        };

        foreach (Tile t in adjacentTiles)
        {
            if (t.type == TileType.Liberty)
            {
                hasLiberty = true;
            }
        }

        return hasLiberty;
    }

    private bool TilesHaveLiberties(Tile tile, TileType floodType)
    {
        List<Tile> groupTiles = new List<Tile>();

        Flood((int)tile.boardPos.x, (int)tile.boardPos.y, floodType, groupTiles);

        bool hasAnyLiberty = false;
        foreach(Tile t in groupTiles)
        {
            if(t.hasLiberty)
                hasAnyLiberty = true;
        }

        if(!hasAnyLiberty)
        {
            foreach(Tile t in groupTiles)
            {
                turnCapturedTiles.Add(t);
            }
        }

        //Debug.Log(tile.boardPos + " " + hasAnyLiberty);
        
        foreach(Tile t in groupTiles) // Reset has liberty for next group
        {
            t.hasLiberty = false;
        }

        return hasAnyLiberty;
    }

    private void Flood(int x, int y, TileType flowType, List<Tile> groupTiles)
    {
        if (x > 0 && x <= size && y > 0 && y <= size)
        {
            Tile t = tiles[new Vector2(x, y)];
            if (t.type == flowType && !groupTiles.Contains(t))
            {
                if(TileHasLiberty(t))
                    t.hasLiberty = true;
                groupTiles.Add(t);

                Flood(x, y + 1, flowType, groupTiles); // UP
                Flood(x + 1, y, flowType, groupTiles); // RIGHT
                Flood(x, y - 1, flowType, groupTiles); // DOWN
                Flood(x - 1, y, flowType, groupTiles); // LEFT
            }
        }
    }

    public Tile[] GetColindantTiles(Tile tile)
    {
        Vector2 tilePos = tile.boardPos;
        Vector2[] adjacentTiles =
        {
            new Vector2(tilePos.x - 1, tilePos.y + 1), // UL
            new Vector2(tilePos.x, tilePos.y + 1), // UP
            new Vector2(tilePos.x + 1, tilePos.y + 1), // UR

            new Vector2(tilePos.x + 1, tilePos.y), // RIGHT
            tilePos, // THIS
            new Vector2(tilePos.x - 1, tilePos.y),  // LEFT

            new Vector2(tilePos.x - 1, tilePos.y - 1), // DL
            new Vector2(tilePos.x, tilePos.y - 1), // DOWN
            new Vector2(tilePos.x + 1, tilePos.y - 1)  // DR
        };

        Tile[] colindantTiles = new Tile[9];
        for (int i = 0; i < colindantTiles.Length; i++)
        {
            if(tiles.ContainsKey(adjacentTiles[i]))
            {
                colindantTiles[i] = tiles[adjacentTiles[i]];
            }
            else
            {
                colindantTiles[i] = tile;
            }
        }

        return colindantTiles;
    }

}