using System;
using UnityEngine;

public class KeypadButton : MonoBehaviour
{
    public int index;
    public GameManager gameManager;
    public void SetIndex(int index)
    {
        this.index = index;

    }
    public void ButtonDown()
    {
        if (gameManager != null)
        {
            gameManager.keyPadMinigameButton(index);
        }
    }
}
