using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SavesLoader : MonoBehaviour
{
    private List<string> gameSave = new List<string>();
    [SerializeField]
    private BoardController _board;

    public void AddMoveToSave(TileType type, Vector2 pos)
    {
        string move = string.Empty;

        if (type == TileType.Black)
            move += "B[";
        else if (type == TileType.White)
            move += "W[";

        move += NumberToLetter((int)pos.x) + "" + NumberToLetter(1 + (_board.size - (int)pos.y)) + "]";

        gameSave.Add(move);
    }

    public void ClearGameSave()
    {
        gameSave.Clear();
    }

    public void SaveGame(TMP_InputField input = null)
    {
        SaveGameFunction(input);
    }
    public string SaveGameFunction(TMP_InputField input = null)
    {
        string save = "(;FF[4]\nCA[UTF-8]\nGM[1]\n";
        save += "DT[" + System.DateTime.Now.ToString("yyyy-MM-dd") + "]\n";
        save += "PC[Visual Goban by TorryDEV]\nGN[Visual Goban Game]\nPB[Red]\nPW[Blue]\nBR[?]\nWR[?]\nRE[?]\n";
        save += "SZ[" + _board.size + "]\n";

        foreach (string move in gameSave)
        {
            save += "(;" + move + "\n";
        }
        save += "(;B[]\n(;W[]\n";

        for (int i = 0; i < save.Count(x => x == '('); i++)
        {
            save += ")";
        }

        if (input != null)
        {
            input.text = save;
        }
        //System.IO.File.WriteAllText(Application.dataPath + "/game.sgf", save);

        return save;
    }

    public void LoadGame(TMP_InputField input)
    {
        ConfirmWindow.Instance.Call(() => LoadGameFunction(input.text));
    }
    private void LoadGameFunction(string input)
    {
        bool iaEnabled = _board.iaEnabled;
        _board.iaEnabled = false;

        string save = input;

        string boardSizeStr = save.Substring(save.IndexOf("SZ[") + 3, 3);
        boardSizeStr = boardSizeStr.Substring(0, boardSizeStr.IndexOf("]"));
        int newBoardSize = int.Parse(boardSizeStr);
        _board.CreateBoard(newBoardSize);

        int indexMoves = save.IndexOf(";B");
        int indexEnd = save.IndexOf(")");
        save = save.Substring(indexMoves, indexEnd - indexMoves);

        string[] moves = save.Split(";");

        List<Tile> tiles = new List<Tile>();

        foreach (string move in moves)
        {
            if (!string.IsNullOrEmpty(move))
            {
                TileType type = move[0].Equals('B') ? TileType.Black : TileType.White;

                if (char.IsLetter(move[2]) && char.IsLetter(move[3]))
                {
                    Vector2 pos = new Vector2(
                        LetterToNumber(move[2]),
                        1 + newBoardSize - LetterToNumber(move[3])
                    );
                    Tile tile = _board.tiles[pos];

                    Debug.Log(type);
                    _board.PlaceAndDrawStone(tile, type, false);

                    tiles.Add(tile);
                }
            }
        }

        foreach (Tile t in tiles)
        {
            t.UpdateSprites(t);
        }

        ShakeController.Instance.Shake();

        _board.iaEnabled = iaEnabled;
    }

    public void GoBack()
    {
        ConfirmWindow.Instance.Call(GoBackFunction);
    }
    private void GoBackFunction()
    {
        bool iaEnabled = _board.iaEnabled;
        _board.iaEnabled = false;
        
        gameSave.RemoveAt(gameSave.Count - 1);
        LoadGameFunction(SaveGameFunction());

        _board.iaEnabled = iaEnabled;
    }

    public char NumberToLetter(int i)
    {
        return Convert.ToChar(96 + i);
    }

    public int LetterToNumber(char c)
    {
        return Convert.ToInt32(c) - 96;
    }
}
