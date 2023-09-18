using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public float moveSpeed = 2;
    public float fadeSpeed = 5;
    [HideInInspector]
    public Tile targetTile;
    public SpriteRenderer cursorSprite;
    public SpriteRenderer denySprite;
    private bool hideCursor = true;
    
    private Color transparentColor;

    // Start is called before the first frame update
    void Start()
    {
        transparentColor = Color.white; transparentColor.a = 0;
        cursorSprite.color = transparentColor;
        denySprite.color = transparentColor;
    }

    // Update is called once per frame
    void Update()
    {
        if(targetTile != null)
            transform.position = Vector3.Lerp(transform.position, targetTile.transform.position, Time.deltaTime * moveSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * moveSpeed);

        cursorSprite.color = Color.Lerp(cursorSprite.color, hideCursor ? transparentColor : Color.white, Time.deltaTime * fadeSpeed);
        denySprite.color = Color.Lerp(denySprite.color, transparentColor, Time.deltaTime * fadeSpeed);
    }

    // Update is called once per frame
    public void ClickAnimation()
    {
        hideCursor = false;
        cursorSprite.color = Color.white;
        denySprite.color = transparentColor;
        transform.localScale = Vector3.one * 1.2f;
    }

    // Update is called once per frame
    public void HideCursor()
    {
        hideCursor = true;
    }

    public void ClickAnimationDeny()
    {
        denySprite.color = Color.white;
        cursorSprite.color = transparentColor;
        transform.localScale = Vector3.one * 1.2f;
    }
}
