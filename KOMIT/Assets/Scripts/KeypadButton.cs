using System;
using UnityEngine;

public class KeypadButton : MonoBehaviour
{
    public int index;
    public int visualNR;
    public GameManager gameManager;
    public void SetIndex(int index, int visualNR)
    {
        this.index = index;
        this.visualNR = visualNR;
    }
    public void ButtonDown()
    {
        if (gameManager != null)
        {
            gameManager.keyPadMinigameButton(index, visualNR);
        }
    }
}
