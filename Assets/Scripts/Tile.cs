using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TileType
{
    Liberty         =   -1,
    Black           =   0,
    White           =   1,
    //WhiteLiberty    =  2,
    //BlackLiberty    =  3,
    Border          =   8,
}

[System.Serializable]
public class TileSprites
{
    public SpriteRenderer castle;
    public SpriteRenderer tower;
    public SpriteRenderer house;
    public SpriteRenderer bridgeDown, bridgeLeft, bridgeRight;
    public SpriteRenderer pathUL, pathUR, pathDL, pathDR;
}

// CALCULAR 3 TIPOS DE GRUPO BLANCAS, NEGRAS, LIBERTADES BLANCAS, LIBERTADES NEGRAS || SI DENTRO DE UN GRUPO DE LIBERTADES NEGRAS HAY UNA LIBERTAD BLANCA, PARA DE SER GRUPO?

public class Tile : MonoBehaviour
{
    public TileType type;
    public bool hasLiberty;

    [SerializeField]
    private GameObject _highlight;
    [SerializeField]
    private Transform _tilesParent;
    [SerializeField]
    private Transform _outBordersParent;
    [SerializeField]
    private Collider2D _clickCollider;

    public TileSprites[] sprites;

    [HideInInspector]
    public Vector2 boardPos;
    private BoardController _board;


    public void Init(BoardController board, Vector2 boardPos)
    {
        _board = board;
        this.boardPos = boardPos;

        CheckTileBorder();
        ChangeType(type);
    }

    private void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        _highlight.SetActive(false);
    }

    private void OnMouseDown()
    {
        _board.ClickTile(this);
    }

    public void ChangeType(TileType tileType)
    {
        type = tileType;
        
        // 1 2 3
        // 4 5 6
        // 7 8 9

        Tile[] colindantTiles = _board.GetColindantTiles(this);
        foreach(Tile t in colindantTiles)
        {
            t?.UpdateSprites(t);
        }
    }

    private void CheckTileBorder()
    {
        if (boardPos.x > _board.size || boardPos.y > _board.size || boardPos.x < 1 || boardPos.y < 1)
        {
            type = TileType.Border;

            int borderPosition = GetOutBorderPosition(boardPos, 0, _board.size + 1);
            for (int i = 1; i <= 9; i++)
            {
                _outBordersParent.GetChild(i - 1).gameObject.SetActive(i == borderPosition);
            }

            Destroy(_clickCollider);
        }
        else
        {
            type = TileType.Liberty;

            _outBordersParent.gameObject.SetActive(false);

            int tilePosition;
            if(CheckHoshi(_board.size, boardPos))
            {
                tilePosition = 10;
            }
            else
            {
                tilePosition = GetOutBorderPosition(boardPos, 1, _board.size);
            }

            for (int i = 1; i <= 10; i++)
            {
                _tilesParent.GetChild(i - 1).gameObject.SetActive(i == tilePosition);
            }
        }
    }

    private int GetOutBorderPosition(Vector2 pos, int boardStart, int boardEnd)
    {
        // 1 2 3
        // 4 5 6
        // 7 8 9

        if (pos.x <= boardStart)
        {
            if (pos.y <= boardStart)
                return 7;
            else if (pos.y >= boardEnd)
                return 1;
            else
                return 4;
        }
        else if (pos.x >= boardEnd)
        {
            if (pos.y <= boardStart)
                return 9;
            else if (pos.y >= boardEnd)
                return 3;
            else
                return 6;
        }
        else
        {
            if (pos.y <= boardStart)
                return 8;
            else if (pos.y >= boardEnd)
                return 2;
            else
                return 5;
        }
    }

    Vector2[] hoshiPoints9 = {new Vector2(3,3), new Vector2(3,7), new Vector2(5,5), new Vector2(7,3), new Vector2(7,7)};
    private bool CheckHoshi(int boardSize, Vector2 pos)
    {
        if(boardSize == 9)
        {
            if(!hoshiPoints9.Contains(pos))
                Debug.Log(pos);
            return hoshiPoints9.Contains(pos);
        }

        return false;
    }

    private void UpdateSprites(Tile t)
    {
        foreach (TileSprites tileSprites in sprites)
        {
            tileSprites.bridgeDown.enabled = false;
            tileSprites.bridgeRight.enabled = false;
            tileSprites.bridgeLeft.enabled = false;
            tileSprites.pathUL.enabled = false;
            tileSprites.pathUR.enabled = false;
            tileSprites.pathDL.enabled = false;
            tileSprites.pathDR.enabled = false;
            tileSprites.castle.enabled = false;
            tileSprites.tower.enabled = false;
            tileSprites.house.enabled = false;
        }

        if(type == TileType.Black || type == TileType.White)
        {
            Tile[] colindantTiles = _board.GetColindantTiles(t);
            int tileColor = (int)type; // 0 = Black, 1 = White
            
            int wallLevel = 0;

            if(colindantTiles[0]?.type == this.type) // UP LEFT
            {
                wallLevel = 1;
                sprites[tileColor].pathUL.enabled = true;
            }
            if(colindantTiles[2]?.type == this.type) // UP RIGHT
            {
                wallLevel = 1;
                sprites[tileColor].pathUR.enabled = true;
            }

            if(colindantTiles[6]?.type == this.type)  // DOWN LEFT
            {
                wallLevel = 1;
                sprites[tileColor].pathDL.enabled = true;
            }
            if(colindantTiles[8]?.type == this.type)  // DOWN RIGHT
            {
                wallLevel = 1;
                sprites[tileColor].pathDR.enabled = true;
            }
            if(colindantTiles[1]?.type == this.type) // UP
            {
                wallLevel = 2;
            }
            if(colindantTiles[7]?.type == this.type) // DOWN
            {
                wallLevel = 2;
                sprites[tileColor].bridgeDown.enabled = true;
            }
            if(colindantTiles[5]?.type == this.type) // RIGHT
            {
                wallLevel = 2;
                sprites[tileColor].bridgeLeft.enabled = true;
            }
            if(colindantTiles[3]?.type == this.type)  // LEFT
            {
                wallLevel = 2;
                sprites[tileColor].bridgeRight.enabled = true;
            }

            if(wallLevel == 2)
            {
                sprites[tileColor].castle.enabled = true;
            }
            else if(wallLevel == 1)
            {
                sprites[tileColor].tower.enabled = true;
            }
            else
            {
                sprites[tileColor].house.enabled = true;
            }
        }
    }
}