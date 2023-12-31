using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BoardController : MonoBehaviour
{
    public int size;
    public TileType turnType;
    public CursorController cursor;
    public TextMeshProUGUI capturedBlackText;
    public TextMeshProUGUI capturedWhiteText;
    public TextMeshProUGUI scoreBlackText;
    public TextMeshProUGUI scoreWhiteText;
    public TextMeshProUGUI AIText;

    [SerializeField]
    private Tile _tilePrefab;
    [SerializeField]
    private CamController _cam;
    [SerializeField]
    private SavesLoader _savesLoader;
    [SerializeField]
    private Image[] _turnIndicator;
    [SerializeField]
    private GameAI ai;

    [HideInInspector]
    public Tile downTile;
    [HideInInspector]
    public Dictionary<Vector2, Tile> tiles;
    [HideInInspector]
    public List<Tile> turnCapturedTiles;
    public int capturedBlack;
    public int capturedWhite;

    private Dictionary<Vector2, TileType> tileMap;
    [HideInInspector]
    public bool iaEnabled = false;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ConfirmWindow.Instance.Call(() => Application.Quit());
        }
    }

    public void ResetButton(int size)
    {
        ConfirmWindow.Instance.Call(() => CreateBoard(size));
    }

    public void ResetGrid()
    {
        if(tiles != null)
        {
            foreach(Tile tile in tiles.Values)
            {
                Destroy(tile.gameObject);
            }
        }
        tiles = new Dictionary<Vector2, Tile>();
        turnCapturedTiles = new List<Tile>();
        tileMap = new Dictionary<Vector2, TileType>();

        capturedBlack = 0;
        capturedWhite = 0;
    }

    public void UpdateScore()
    {
        Dictionary<Vector2, TileType> actualTileMap = new Dictionary<Vector2, TileType>();
        foreach (KeyValuePair<Vector2, Tile> t in tiles)
        {
            actualTileMap[t.Key] = t.Value.type;
        }

        var puntuacion = ai.CalcularPuntuacion(actualTileMap);

        scoreBlackText.text = puntuacion.puntuacionBlack.ToString();
        scoreBlackText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = puntuacion.puntuacionBlack.ToString();
        scoreWhiteText.text = puntuacion.puntuacionWhite.ToString();
        scoreWhiteText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = puntuacion.puntuacionWhite.ToString();
    }

    public void CreateBoard(int givenSize = 0)
    {
        if(givenSize != 0)
            size = givenSize;

        NextTurn(true);

        ResetGrid();
        _savesLoader.ClearGameSave();

        for (int x = 0; x <= (size + 1); x++)
        {
            for (int y = 0; y <= (size + 1); y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile[{x},{y}]";

                Vector2 boardPosition = new Vector2(x, y);
                tiles[boardPosition] = spawnedTile;

                spawnedTile.Init(this, boardPosition);
            }
        }

        _cam.transform.position = new Vector3((float)size / 2f + 0.5f, (float)size / 2f + 0.3f, -10);
        _cam.SetBoardCamera();
    }

    public void DownTile(Tile tile)
    {
        if(!IsPointerOverUIObject())
        {
            downTile = tile;
        }
    }
    public void UpTile(Tile tile)
    {
        if(tile.Equals(downTile) && !IsPointerOverUIObject())
        {
            SelectTile(tile);
        }
    }
    public void SelectTile(Tile tile)
    {
        cursor.targetTile = tile;
        cursor.ClickAnimation();
    }

    public void Confirm()
    {
        if(cursor.targetTile != null)
        {
            PlaceAndDrawStone(cursor.targetTile, turnType);
            cursor.HideCursor();
            cursor.targetTile = null;
        }
    }

    public void NextTurnButton()
    {
        ConfirmWindow.Instance.Call(() => NextTurn());
    }

    public void EnableIA(bool enable)
    {
        iaEnabled = enable;
        AIText.gameObject.SetActive(enable);
    }

    private void NextTurn(bool firstTime = false)
    {
        if (turnType == TileType.White || firstTime)
        {
            turnType = TileType.Black;
            _turnIndicator[0].gameObject.SetActive(true);
            _turnIndicator[1].gameObject.SetActive(false);
        }
        else if (turnType == TileType.Black)
        {
            turnType = TileType.White;
            _turnIndicator[0].gameObject.SetActive(false);
            _turnIndicator[1].gameObject.SetActive(true);

            if(iaEnabled)
            {
                Vector2 aiPlaceStone = ai.MakeMove(tileMap);
                if(aiPlaceStone == Vector2.zero)
                {
                    NextTurn();
                }
                else
                {
                    PlaceAndDrawStone(tiles[aiPlaceStone], turnType);
                }
            }
        }
    }

    public void PlaceAndDrawStone(Tile tile, TileType moveType, bool drawSprites = true)
    {
        string placeStoneError = CanPlaceStone(tile, moveType);

        if (placeStoneError == string.Empty)
        {
            if (drawSprites)
            {
                tile.ChangeType(moveType);

                if (turnCapturedTiles?.Count > 0)
                {
                    foreach (Tile t in turnCapturedTiles)
                    {
                        if(!t.tileEscombrada)
                        {
                            t.ChangeType(TileType.Liberty);

                            if(moveType == TileType.Black)
                            {
                                capturedBlack ++;
                            }
                            else if(moveType == TileType.White)
                            {
                                capturedWhite ++;
                            }
                            t.tileEscombrada = true;
                        }
                    }
                }
            }
            else
            {
                tile.type = moveType;

                foreach (Tile t in turnCapturedTiles)
                {
                    if(!t.tileEscombrada)
                    {
                        t.ChangeType(TileType.Liberty);

                        if(moveType == TileType.Black)
                        {
                            capturedBlack ++;
                        }
                        else if(moveType == TileType.White)
                        {
                            capturedWhite ++;
                        }
                        t.tileEscombrada = true;
                    }
                }
            }

            if(moveType == TileType.Black)
            {
                capturedBlackText.text = capturedBlack.ToString();
                capturedBlackText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = capturedBlack.ToString();
            }
            else if(moveType == TileType.White)
            {
                capturedWhiteText.text = capturedWhite.ToString();
                capturedWhiteText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = capturedWhite.ToString();
            }

            foreach (Tile t in turnCapturedTiles)
            {
                t.tileEscombrada = false;
            }

            turnCapturedTiles?.Clear();

            NextTurn();
            _savesLoader.AddMoveToSave(tile.type, tile.boardPos);
        }
        else
        {
            turnCapturedTiles?.Clear();
            Debug.Log(tile.boardPos + " | " + placeStoneError);
            cursor.ClickAnimationDeny();
        }
    }

    public string CanPlaceStone(Tile tile, TileType moveType)
    {
        Dictionary<Vector2, TileType> lastTileMap = tileMap;

        Dictionary<Vector2, TileType> actualTileMap = new Dictionary<Vector2, TileType>();
        foreach (KeyValuePair<Vector2, Tile> t in tiles)
        {
            actualTileMap[t.Key] = t.Value.type;
        }

        if (tile.type != TileType.Liberty)
            return "You need to place a stone on a empty space!";

        tile.type = moveType;

        Tile[] adjacentTiles =
        {
            tiles[new Vector2(tile.boardPos.x, tile.boardPos.y + 1)], // UP
            tiles[new Vector2(tile.boardPos.x + 1, tile.boardPos.y)], // RIGHT
            tiles[new Vector2(tile.boardPos.x, tile.boardPos.y - 1)], // DOWN
            tiles[new Vector2(tile.boardPos.x - 1, tile.boardPos.y)]  // LEFT
        };
        foreach (Tile t in adjacentTiles)
        {
            if ((moveType == TileType.White && t.type == TileType.Black)
            || (moveType == TileType.Black && t.type == TileType.White))
            {
                TilesHaveLiberties(t, t.type);
            }
        }

        if (turnCapturedTiles?.Count > 0 || TilesHaveLiberties(tile, moveType)) // Si se captura alguna alrededor o tiene espacio
        {
            Dictionary<Vector2, TileType> newTileMap = new Dictionary<Vector2, TileType>();
            foreach (KeyValuePair<Vector2, Tile> t in tiles)
            {
                newTileMap[t.Key] = turnCapturedTiles.Contains(tiles[t.Key]) ? TileType.Liberty : t.Value.type;
            }

            bool equalTileMaps = true;
            foreach (KeyValuePair<Vector2, TileType> t in newTileMap)
            {
                if (lastTileMap.ContainsKey(t.Key))
                {
                    if (t.Value != lastTileMap[t.Key])
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

            if (equalTileMaps) // KO RULE
            {
                tile.type = TileType.Liberty;
                return "KO: You can't repeat a previous position!";
            }
            else
            {
                tileMap = actualTileMap;

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
        foreach (Tile t in groupTiles)
        {
            if (t.hasLiberty)
                hasAnyLiberty = true;
        }

        if (!hasAnyLiberty)
        {
            foreach (Tile t in groupTiles)
            {
                turnCapturedTiles.Add(t);
            }
        }

        //Debug.Log(tile.boardPos + " " + hasAnyLiberty);

        foreach (Tile t in groupTiles) // Reset has liberty for next group
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
                if (TileHasLiberty(t))
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
            if (tiles.ContainsKey(adjacentTiles[i]))
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

    public bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }
}