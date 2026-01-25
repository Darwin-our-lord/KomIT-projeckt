using Alteruna;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum miniGames
{
    //colorCount,
    Keypads



}


public class GameManager : AttributesSync
{
    [Header("UI References")]
    public GameObject alterunaMenu;

    [Header("Game Data")]

    [SynchronizableField]
    public int currentMinigame = 0;

    [SynchronizableField]
    public int minigamesCompleted = 0;

    [SynchronizableField]
    public string TargetWord = "Press Space to Start";

    [SynchronizableField] 
    public bool minigameRunning = false;


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
            if (Multiplayer.Me.Index == 0)
            {
                
                currentMinigame = 0;
                miniGames minigame = RandomEnumValue<miniGames>();
                switch (minigame)
                {
                    /*case miniGames.colorCount:
                       currentMinigame = 1;
                        break;*/

                    case miniGames.Keypads:
                        currentMinigame = 2;
                        break;

                    default:
                        Debug.LogError("no minigame was selected - you're a dumbass, dumbass");
                        return;
                }
                Commit();
            }
            Debug.LogWarning("waiting");
            if (currentMinigame == 0) return;

            switch (currentMinigame)
            {
                case 1:
                    PlayColorCount();
                    break;

                case 2:
                    Debug.Log("done!!!");
                    PlayKeypads();
                    break;

                case 0:
                    Debug.LogError("no minigame was selected - you're a dumbass, dumbass\n ohh and its in like the second thing btw<3");
                    break;

            }
            minigameRunning = true;

            if (alterunaMenu.activeSelf) alterunaMenu.SetActive(false);
        }
        else
        {
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

    public List<Sprite> allSprites = new List<Sprite>();


    public GameObject player1UI;

    public GameObject[] player1SpritesOBJ;


    public GameObject player2UI;

    public GameObject[] player2SpritesOBJ;

    [SynchronizableField]
    bool needsWait = true;

    [SynchronizableField]
    private List<int> answerSpritesID = new List<int>();

    [SynchronizableField] 
    private List<int> orderSpritesID = new List<int>();


    [SynchronizableField]
    private int orderSpriteAmount = 6;

    [SynchronizableField]
    private int answerSpriteAmount = 4;

    private int currentStageIndex = 0;
    private int lossesAmount = 0;

    public void PlayKeypads()
    {
        if (Multiplayer.Me.Index == 0)
        {
            needsWait = true;
            currentStageIndex = 0;
            answerSpritesID.Clear();



            while (answerSpritesID.Count < answerSpriteAmount)
            {
                int s = UnityEngine.Random.Range(0, allSprites.Count);
                if (!answerSpritesID.Contains(s)) answerSpritesID.Add(s);
            }

            orderSpritesID = new List<int>(new int[orderSpriteAmount]);

            //makes a list of 6 numbers, randomly shuffles them, picks the first 4, puts them in order from smallest to largest
            var slots = Enumerable.Range(0, orderSpriteAmount).OrderBy(_ => UnityEngine.Random.value).Take(answerSpriteAmount).OrderBy(x => x).ToList();

            int ansIndex = 0;
            for (int i = 0; i < orderSpriteAmount; i++)
            {
                if (slots.Contains(i))
                {
                    orderSpritesID[i] = answerSpritesID[ansIndex];
                    ansIndex++;
                }
                else
                {
                    do { orderSpritesID[i] = UnityEngine.Random.Range(0, allSprites.Count); }
                    while (answerSpritesID.Contains(orderSpritesID[i]) || orderSpritesID.IndexOf(orderSpritesID[i]) != i);
                }
            }
            needsWait = false;
            Commit();

        }
        Debug.LogWarning("waiting2---");
        if (needsWait) { StartCoroutine(WaitThenRestart()); return; }
        Debug.Log("done2");

        //answer guy-------------------------------------------------------------------------------------------------
        if (Multiplayer.Instance.Me.Index == 0)
        {
            player1UI.SetActive(true);

            for (int i = 0; i < answerSpriteAmount; i++) 
            {
                player1SpritesOBJ[i].GetComponent<Image>().sprite = allSprites[answerSpritesID[i]];
                player1SpritesOBJ[i].GetComponent<KeypadButton>().SetIndex(i);
            } 

        }




        //order guy--------------------------------------------------------------------------------------------------
        else if (Multiplayer.Instance.Me.Index == 1)
        {
            player2UI.SetActive(true);

            for (int i = 0; i < orderSpriteAmount; i++)
            {
                player2SpritesOBJ[i].GetComponent<Image>().sprite = allSprites[orderSpritesID[i]];
            }

        }


    }

    public void keyPadMinigameButton(int index)
    {
        if(index == currentStageIndex)
        {
            currentStageIndex++;
            player1SpritesOBJ[index].GetComponent<Image>().color = Color.green;

            if (currentStageIndex >= answerSpriteAmount)
            {
                minigamesCompleted++;
                Debug.LogError("GAME WON______");
            }
        }
        else
        {
            lossesAmount++;
            if(lossesAmount < 3)
            {
                currentStageIndex = 0;
                for (int i = 0; i < player1SpritesOBJ.Length; i++)
                {
                    player1SpritesOBJ[i].GetComponent<Image>().color = Color.white;
                }
            }
            else
            {
                Debug.LogError("GAME LOST!!!");

            }
        }
    }

    public IEnumerator WaitThenRestart()
    {
        yield return new WaitForSeconds(0.1f);
        PlayKeypads();
    }

    #endregion
}
