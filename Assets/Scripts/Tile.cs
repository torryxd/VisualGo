using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum TileType
{
    Border      = -1,
    Liberty     =  0,
    Black       =  1,
    White       =  2,
}

public class Tile : MonoBehaviour
{
    public TileType type;

    [SerializeField]
    private GameObject _highlight;
    [SerializeField]
    private Transform _stonesParent;
    [SerializeField]
    private Transform _tilesParent;
    [SerializeField]
    private Transform _outBordersParent;

    private BoardController _board;
    private Vector2 _boardPos;


    public void Init(BoardController board, Vector2 boardPos)
    {
        _board = board;
        _boardPos = boardPos;

        CheckTileBorder();
        PlaceStone();
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
        if(PlaceStone())
        {
            _board.NextTurn();
        }
    }

    public bool PlaceStone()
    {
        bool nextTurn = false;

        if(type == TileType.Black || type == TileType.White)
        {
            type = TileType.Liberty;
        }
        else if (type == TileType.Liberty)
        {
            type = _board.turnType;
            nextTurn = true;
        }

        for (int i = 0; i <= 2; i++)
        {
            _stonesParent.GetChild(i).gameObject.SetActive(i == (int)type);
        }

        return nextTurn;
    }

    private void CheckTileBorder()
    {
        if (_boardPos.x > _board.size || _boardPos.y > _board.size || _boardPos.x < 1 || _boardPos.y < 1)
        {
            type = TileType.Border;
            int borderPosition = GetOutBorderPosition(_boardPos, 0, _board.size + 1);
            for (int i = 1; i <= 9; i++)
            {
                _outBordersParent.GetChild(i - 1).gameObject.SetActive(i == borderPosition);
            }
        }
        else
        {
            type = TileType.Liberty;
            _outBordersParent.gameObject.SetActive(false);

            int tilePosition = GetOutBorderPosition(_boardPos, 1, _board.size);
            for (int i = 1; i <= 9; i++)
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
}