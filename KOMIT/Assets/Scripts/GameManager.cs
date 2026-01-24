using Alteruna;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum miniGames
{
    colorCount,
    Keypads



}


public class GameManager : AttributesSync
{
    [Header("UI References")]
    public TMP_Text StatusText;
    public GameObject canvas;
    public GameObject alterunaMenu;

    [Header("Game Data")]

    [SynchronizableField]
    public string TargetWord = "Press Space to Start";

    [SynchronizableField] public bool minigameRunning = false;

    static System.Random _R = new System.Random();
    static miniGames RandomEnumValue<miniGames>()
    {
        var v = Enum.GetValues(typeof(miniGames));
        return (miniGames)v.GetValue(_R.Next(v.Length));
    }

    private void Update()
    {
        if (minigameRunning) return;
        if(Multiplayer.GetUsers().Count == 2)
        {
            minigameRunning =true;
            miniGames minigame = RandomEnumValue<miniGames>();
            switch (minigame)
            {
                case miniGames.colorCount:
                    PlayColorCount();
                    break;

                case miniGames.Keypads:
                    PlayKeypads();
                    break;

                default:
                    Debug.LogError("no minigame was selected");
                    return;
            }
            Commit();

            if(!canvas.activeSelf) canvas.SetActive(true);
            if (alterunaMenu.activeSelf) alterunaMenu.SetActive(false);
        }
        else
        {
            if (canvas.activeSelf) canvas.SetActive(false);
            if (!alterunaMenu.activeSelf) alterunaMenu.SetActive(true);
        }
        

    }


    #region colorCountGame

    [Header("ColorCountGame")]

    [SynchronizableField]
    public int greenAmmount = 0;
    [SynchronizableField]
    public int redAmmount = 0;
    [SynchronizableField]
    public int yellowAmmount = 0;
    [SynchronizableField]
    public int pinkAmmount = 0;

    public void PlayColorCount() 
    { 
        
        


    }

    #endregion

    #region keypadsGame

    [Header("KeypadsGame")]

    [SynchronizableField]
    public List<Sprite> allSprites = new List<Sprite>();

    [SynchronizableField]
    public GameObject player1UI;
    [SynchronizableField]
    public GameObject[] player1SpritesOBJ;


    [SynchronizableField]
    public GameObject player2UI;
    [SynchronizableField]
    public GameObject[] player2SpritesOBJ;


    [SynchronizableField]
    private List<Sprite> answerSprites = new List<Sprite>();

    [SynchronizableField]
    private List<Sprite> orderSprites = new List<Sprite>();


    [SynchronizableField]
    private int orderSpriteAmount = 6;

    [SynchronizableField]
    private int answerSpriteAmount = 4;

    private int currentStageIndex = 0;
    private int lossesAmount = 0;
    private bool gameWon = false;


    public void PlayKeypads()
    {
        currentStageIndex = 0;
        gameWon = false;
        answerSprites.Clear();

        while (answerSprites.Count < answerSpriteAmount)
        {
            Sprite s = allSprites[UnityEngine.Random.Range(0, allSprites.Count)];
            if (!answerSprites.Contains(s)) answerSprites.Add(s);
        }

        orderSprites = new List<Sprite>(new Sprite[orderSpriteAmount]);

        //makes a list of 6 numbers, randomly shuffles them, picks the first 4, puts them in order from smallest to largest
        var slots = Enumerable.Range(0, orderSpriteAmount).OrderBy(_ => UnityEngine.Random.value).Take(answerSpriteAmount).OrderBy(x => x).ToList();

        int ansIndex = 0;
        for (int i = 0; i < orderSpriteAmount; i++)
        {
            if (slots.Contains(i))
            {
                orderSprites[i] = answerSprites[ansIndex];
                ansIndex++;
            }
            else
            {
                do { orderSprites[i] = allSprites[UnityEngine.Random.Range(0, allSprites.Count)]; }
                while (answerSprites.Contains(orderSprites[i]) || orderSprites.IndexOf(orderSprites[i]) != i);
            }
        }
        Commit();

        //answer guy-------------------------------------------------------------------------------------------------
        if (Multiplayer.Instance.Me.Index == 0)
        {
            player1UI.SetActive(true);

            for (int i = 0; i < answerSpriteAmount; i++) 
            {
                player1SpritesOBJ[i].GetComponent<SpriteRenderer>().sprite = answerSprites[i];
                player1SpritesOBJ[i].GetComponent<KeypadButton>().SetIndex(i);
            } 

        }




        //order guy--------------------------------------------------------------------------------------------------
        else if (Multiplayer.Instance.Me.Index == 1)
        {
            player2UI.SetActive(true);

            for (int i = 0; i < orderSpriteAmount; i++)
            {
                player2SpritesOBJ[i].GetComponent<SpriteRenderer>().sprite = orderSprites[i];
            }

        }


    }

    public void keyPadMinigameButton(int index)
    {
        if(index == currentStageIndex)
        {
            currentStageIndex++;
            player1SpritesOBJ[index].GetComponent<SpriteRenderer>().color = Color.green;

            if (currentStageIndex <= 5)
            {
                gameWon = true;
            }
        }
        else
        {
            lossesAmount++;
            if(lossesAmount > 3)
            {
                //kill everyone!!!!!
            }
        }
    }

    #endregion
}
