using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// CALCULAR 3 TIPOS DE GRUPO BLANCAS, NEGRAS, LIBERTADES BLANCAS, LIBERTADES NEGRAS || SI DENTRO DE UN GRUPO DE LIBERTADES NEGRAS HAY UNA LIBERTAD BLANCA, PARA DE SER GRUPO?
[RequireComponent(typeof(Image))]
public class ButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float scaleIncrease = 1.1f;
    public AudioSource sound;

    private Vector3 defaultScale;

    private void Start()
    {
        defaultScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = defaultScale * scaleIncrease;
        if(sound != null)
        {
            sound.pitch = Random.Range(0.8f, 1.2f);
            sound.Play();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = defaultScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = defaultScale;
    }
}