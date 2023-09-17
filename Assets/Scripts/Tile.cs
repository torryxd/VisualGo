using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum TileType
{
    //WhiteLiberty    =  -2,
    //BlackLiberty    =  -1,
    Liberty         =   0,
    Black           =   1,
    White           =   2,
    Border          =   8,
}

// CALCULAR 3 TIPOS DE GRUPO BLANCAS, NEGRAS, LIBERTADES BLANCAS, LIBERTADES NEGRAS || SI DENTRO DE UN GRUPO DE LIBERTADES NEGRAS HAY UNA LIBERTAD BLANCA, PARA DE SER GRUPO?

public class Tile : MonoBehaviour
{
    public TileType type;
    public bool hasLiberty;

    [SerializeField]
    private GameObject _highlight;
    [SerializeField]
    private Transform _stonesParent;
    [SerializeField]
    private Transform _tilesParent;
    [SerializeField]
    private Transform _outBordersParent;
    [SerializeField]
    private Collider2D _clickCollider;

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

        for (int i = -2; i <= 2; i++)
        {
            _stonesParent.GetChild(i+2).gameObject.SetActive(i == (int)tileType);
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
}