using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public float moveSpeed = 2;
    public float fadeSpeed = 5;
    [HideInInspector]
    public Tile targetTile = null;
    [HideInInspector]
    public Tile overTile = null;
    public SpriteRenderer cursorSprite;
    public SpriteRenderer denySprite;
    public AudioSource cursorSound;
    public AudioSource denySound;
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
        transform.localScale = Vector3.one * 1.5f;
        if(cursorSound != null)
        {
            cursorSound.pitch = Random.Range(1.2f, 1.6f);
            cursorSound.Play();
        }
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
        transform.localScale = Vector3.one * 2f;
        if(denySound != null)
        {
            denySound.pitch = Random.Range(1.2f, 1.4f);
            denySound.Play();
        }
    }
}
