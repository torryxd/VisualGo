using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudsController : MonoBehaviour
{
    public SpriteRenderer[] clouds;
    private bool[] goingRight;
    private float[] speed;

    public float xLimitMin, xLimitMax;
    public float moveSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        goingRight = new bool[clouds.Length];
        speed = new float[clouds.Length];

        for (int i = 0; i < clouds.Length; i++)
        {
            goingRight[i] = i % 2 == 0;
            Vector2 startPos = clouds[i].transform.position;
            startPos.x = goingRight[i] ? xLimitMin : xLimitMax;
            clouds[i].transform.position = startPos;
            speed[i] = Random.Range(moveSpeed, moveSpeed * 3f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < clouds.Length; i++)
        {
            clouds[i].transform.Translate(Vector2.right * Time.deltaTime * speed[i] * (goingRight[i] ? 1 : -1));

            if (clouds[i].transform.position.x > xLimitMax)
            {
                goingRight[i] = false;
                clouds[i].flipX = true;
                clouds[i].GetComponentInChildren<SpriteRenderer>().flipX = true;
            }
            else if (clouds[i].transform.position.x < xLimitMin)
            {
                goingRight[i] = true;
                clouds[i].flipX = false;
                clouds[i].GetComponentInChildren<SpriteRenderer>().flipX = false;
            }
        }
    }
}
