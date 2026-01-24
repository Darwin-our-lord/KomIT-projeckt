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
    private void OnMouseDown()
    {
        if (gameManager != null)
        {
            gameManager.keyPadMinigameButton(index);
        }
    }
}
