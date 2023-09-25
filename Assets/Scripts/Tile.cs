using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TileType
{
    Liberty = -1,
    Black = 0,
    White = 1,
    //WhiteLiberty    =  2,
    //BlackLiberty    =  3,
    Border = 8,
}

[System.Serializable]
public class TileSprites
{
    public SpriteRenderer castle;
    public SpriteRenderer tower;
    public SpriteRenderer house;
    public SpriteRenderer bridgeDown, bridgeLeft, bridgeRight;
    public SpriteRenderer pathUL, pathUR, pathDL, pathDR;
    public SpriteRenderer[] escombros;
}

// CALCULAR 3 TIPOS DE GRUPO BLANCAS, NEGRAS, LIBERTADES BLANCAS, LIBERTADES NEGRAS || SI DENTRO DE UN GRUPO DE LIBERTADES NEGRAS HAY UNA LIBERTAD BLANCA, PARA DE SER GRUPO?

public class Tile : MonoBehaviour
{
    public TileType type;
    public bool hasLiberty;

    [SerializeField]
    private Transform _tilesParent;
    [SerializeField]
    private Transform _outBordersParent;
    [SerializeField]
    private Collider2D _clickCollider;
    [SerializeField]
    private ParticleSystem _smokeParticle;
    [SerializeField]
    private ParticleSystem _explosionParticle;
    [SerializeField]
    private ParticleSystem _markParticle;
    [SerializeField]
    private AudioSource _smokeSound;
    [SerializeField]
    private AudioSource _explosionSound;
    [SerializeField]
    private Animator _structureAnim;

    public TileSprites[] sprites;

    [HideInInspector]
    public Vector2 boardPos;
    [HideInInspector]
    public bool tileEscombrada = false;
    private BoardController _board;
    private int structureLevel;


    public void Init(BoardController board, Vector2 boardPos)
    {
        _board = board;
        this.boardPos = boardPos;

        CheckTileBorder();
        ChangeType(type);
    }

    private void OnMouseDown()
    {
        _board.DownTile(this);
        _markParticle.Play();
    }

    private void OnMouseUp()
    {
        _board.UpTile(this);
    }

    public void ChangeType(TileType tileType)
    {
        TileType lastType = type;
        type = tileType;
        int upgradedSprites = 0;

        Tile[] colindantTiles = _board.GetColindantTiles(this);
        foreach (Tile t in colindantTiles)
        {
            if (t != null && t.UpdateSprites(t))
            {
                upgradedSprites++;
            }
        }
        if (upgradedSprites > 0)
        {
            ShakeController.Instance.Shake(0.075f, 0.1f);
            if (!_smokeSound.isPlaying)
            {
                _smokeSound.pitch = Random.Range(1.2f, 1.5f);
                _smokeSound.Play();
            }
        }
        
        UpdateEscombros(this, lastType);
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
            structureLevel = 0;

            _outBordersParent.gameObject.SetActive(false);

            int tilePosition;
            if (CheckHoshi(_board.size, boardPos))
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

    Vector2[] hoshiPoints9 = { new Vector2(3, 3), new Vector2(3, 7), new Vector2(5, 5), new Vector2(7, 3), new Vector2(7, 7) };
    Vector2[] hoshiPoints13 = { new Vector2(4, 4), new Vector2(4, 10), new Vector2(7, 7), new Vector2(10, 4), new Vector2(10, 10) };
    Vector2[] hoshiPoints19 = { new Vector2(4, 4), new Vector2(4, 10), new Vector2(4, 16), new Vector2(10, 4), new Vector2(10, 10), new Vector2(10, 16), new Vector2(16, 4), new Vector2(16, 10), new Vector2(16, 16) };
    private bool CheckHoshi(int boardSize, Vector2 pos)
    {
        if (boardSize == 9)
        {
            return hoshiPoints9.Contains(pos);
        }
        else if (boardSize == 13)
        {
            return hoshiPoints13.Contains(pos);
        }
        else if (boardSize == 19)
        {
            return hoshiPoints19.Contains(pos);
        }

        return false;
    }

    private void UpdateEscombros(Tile t, TileType lastType)
    {
        bool atLeastOneCapture = false;
        if (t.type == TileType.Liberty && lastType != TileType.Liberty)
        {
            structureLevel = 0;
            for (int i = 0; i <= 1; i++)
            {
                t.sprites[(int)lastType].escombros[i].enabled = t.boardPos.x % 2 == i;
                t.sprites[(int)lastType].escombros[i].flipX = t.boardPos.y % 2 == i;
            }

            _explosionParticle.Play();
            atLeastOneCapture = true;
        }
        else
        {
            t.sprites[0].escombros[0].enabled = false;
            t.sprites[0].escombros[1].enabled = false;
            t.sprites[1].escombros[0].enabled = false;
            t.sprites[1].escombros[1].enabled = false;
        }

        if (atLeastOneCapture)
        {
            ShakeController.Instance.Shake(0.175f, 0.15f);
            if (!_explosionSound.isPlaying)
            {
                _smokeSound.pitch = Random.Range(0.9f, 1.1f);
                _explosionSound.Play();
            }
        }
    }

    public bool UpdateSprites(Tile t)
    {
        bool upgradedSprite = false;

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

        if (type == TileType.Black || type == TileType.White)
        {
            Tile[] colindantTiles = _board.GetColindantTiles(t);
            int tileColor = (int)type; // 0 = Black, 1 = White

            int newStructureLevel = 1;

            if (colindantTiles[0]?.type == this.type) // UP LEFT
            {
                newStructureLevel = 2;
                sprites[tileColor].pathUL.enabled = true;
            }
            if (colindantTiles[2]?.type == this.type) // UP RIGHT
            {
                newStructureLevel = 2;
                sprites[tileColor].pathUR.enabled = true;
            }

            if (colindantTiles[6]?.type == this.type)  // DOWN LEFT
            {
                newStructureLevel = 2;
                sprites[tileColor].pathDL.enabled = true;
            }
            if (colindantTiles[8]?.type == this.type)  // DOWN RIGHT
            {
                newStructureLevel = 2;
                sprites[tileColor].pathDR.enabled = true;
            }
            if (colindantTiles[1]?.type == this.type) // UP
            {
                newStructureLevel = 3;
            }
            if (colindantTiles[7]?.type == this.type) // DOWN
            {
                newStructureLevel = 3;
                sprites[tileColor].bridgeDown.enabled = true;
            }
            if (colindantTiles[5]?.type == this.type) // RIGHT
            {
                newStructureLevel = 3;
                sprites[tileColor].bridgeLeft.enabled = true;
            }
            if (colindantTiles[3]?.type == this.type)  // LEFT
            {
                newStructureLevel = 3;
                sprites[tileColor].bridgeRight.enabled = true;
            }

            if (newStructureLevel == 3)
            {
                sprites[tileColor].castle.enabled = true;
            }
            else if (newStructureLevel == 2)
            {
                sprites[tileColor].tower.enabled = true;
            }
            else
            {
                sprites[tileColor].house.enabled = true;
            }

            if (newStructureLevel > structureLevel)
            {
                _smokeParticle.Play();
                _structureAnim.SetTrigger("SquatchStretch");
                upgradedSprite = true;
            }
            structureLevel = newStructureLevel;
        }

        return upgradedSprite;
    }
}