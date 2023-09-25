using UnityEngine;
using System;

public class ConfirmWindow : MonoBehaviour
{
    public static ConfirmWindow Instance { get; private set; }
    private Action confirmAction;

    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }

        this.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void Call(Action action)
    {
        this.transform.GetChild(0).gameObject.SetActive(true);
        confirmAction = action;
    }

    public void Confirm(bool yesOrNo)
    {
        if(yesOrNo)
        {
            confirmAction();
        }
        this.transform.GetChild(0).gameObject.SetActive(false);
    }
}
