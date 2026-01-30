using Alteruna;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum miniGames
{
    Keypads,
    ColorPick



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

    public bool minigameRunning = false;

    [SynchronizableField]
    private bool updateMiniGameRunning = false;

    [SynchronizableField]
    private bool minigameChosen = false;

    static System.Random _R = new System.Random();
    static miniGames RandomEnumValue<miniGames>()
    {
        var v = Enum.GetValues(typeof(miniGames));
        return (miniGames)v.GetValue(_R.Next(v.Length));
    }

    private void Update()
    {
        if ( Multiplayer.Me.Index == 1 && updateMiniGameRunning)
        {
            updateMiniGameRunning = false;
            minigameRunning = false;
            minigameChosen = false;
            player1UI.SetActive(false);
            player2UI.SetActive(false);
            player1UI_CP.SetActive(false);
            player2UI_CP.SetActive(false);
        }
        if (minigameRunning) return;
        if (Multiplayer.GetUsers().Count == 2)
        {
            if (Multiplayer.Me.Index == 0)
            {
                if (!minigameChosen)
                {
                    currentMinigame = 0;
                    miniGames minigame = RandomEnumValue<miniGames>();
                    switch (minigame)
                    {

                        case miniGames.Keypads:
                            currentMinigame = 2;
                            break;
                        case miniGames.ColorPick:
                            currentMinigame = 3;
                            break;
                        default:
                            Debug.LogError("no minigame was selected - you're a dumbass, dumbass");
                            return;
                    }
                    minigameChosen = true;
                    Commit();
                }
            }
            else
            {
                if (currentMinigame == 0)
                {
                    Debug.LogWarning("waiting for minigame sync...");
                    return;
                }

            }
            Debug.LogError("FUCK YOU  -  "+currentMinigame);

            switch (currentMinigame)
            {
                case 2:
                    Debug.LogError("done!!!");
                    minigameRunning = true;
                    Commit();
                    PlayKeypads();
                   
                    break;
                case 3:
                    Debug.LogError("done!!!");
                    minigameRunning = true;
                    Commit();
                    PlayColorPickerGame();
                    break;

                case 0:
                    Debug.LogError("no minigame was selected - you're a dumbass, dumbass\n ohh and its in like the second thing btw<3");
                    break;

            }


            if (alterunaMenu.activeSelf) alterunaMenu.SetActive(false);
        }
        else
        {
            if (!alterunaMenu.activeSelf) alterunaMenu.SetActive(true);
        }


    }

    #region keypadsGame

    [Header("--KeypadsGame--")]

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
        if (index == currentStageIndex)
        {
            currentStageIndex++;
            player1SpritesOBJ[index].GetComponent<Image>().color = Color.green;

            if (currentStageIndex >= answerSpriteAmount)
            {
                minigamesCompleted++;
                Debug.LogError("GAME WON______");
                minigameRunning = false;
                updateMiniGameRunning = true;
                minigameChosen = false;
                player1UI.SetActive(false);
                player2UI.SetActive(false);
                Commit();
            }
        }
        else
        {
            lossesAmount++;
            if (lossesAmount < 3)
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


    #region colorpickergame

    [Header("--ColorPickerGame--")]

    public List<Color> allColors = new List<Color>();

    public GameObject player1UI_CP;

    public GameObject[] player1SpritesOBJ_CP;
    public GameObject[] player1ColorOBJ_CP;

    public GameObject player2UI_CP;

    public GameObject[] player2SpritesOBJ_CP;

    [SynchronizableField]
    public List<int> answerSpritesID_CP = new List<int>();

    [SynchronizableField]
    public List<int> answerSpritesIDColor_CP = new List<int>();

    [SynchronizableField]
    private int answerSpriteAmount_CP = 4;

    private int completedAmount_CP = 0;
    private int lossesAmount_CP = 0;

    public void PlayColorPickerGame()
    {
        if (Multiplayer.Me.Index == 0)
        {
            needsWait = true;
            Commit();

            completedAmount_CP = 0;
            answerSpritesID_CP.Clear();



            while (answerSpritesID_CP.Count < answerSpriteAmount_CP)
            {
                int s = UnityEngine.Random.Range(0, allSprites.Count);
                if (!answerSpritesID_CP.Contains(s)) answerSpritesID_CP.Add(s);
            }
            while (answerSpritesIDColor_CP.Count < answerSpriteAmount_CP)
            {
                int s = UnityEngine.Random.Range(0, allColors.Count);
                if (!answerSpritesIDColor_CP.Contains(s)) answerSpritesIDColor_CP.Add(s);
            }

            needsWait = false;
            Commit();

        }

        if (needsWait) { StartCoroutine(WaitThenRestartColorPicker()); return; }


        if (Multiplayer.Instance.Me.Index == 0)
        {
            player1UI_CP.SetActive(true);

            for (int i = 0; i < answerSpriteAmount; i++)
            {
                player1SpritesOBJ_CP[i].GetComponent<Image>().sprite = allSprites[answerSpritesID_CP[i]];
                player1ColorOBJ_CP[i].GetComponent<Image>().color = allColors[answerSpritesIDColor_CP[i]];

                player1ColorOBJ_CP[i].GetComponent<ColorZone>().SetAnswerId(i);
                player1SpritesOBJ_CP[i].GetComponent<UIDrag>().SetAnswerId(i);
                Commit();
            }
        }

        if(answerSpritesIDColor_CP.Count == 0) { StartCoroutine(WaitThenRestartColorPicker()); return; }

        if (Multiplayer.Instance.Me.Index == 1)
        {
            player2UI_CP.SetActive(true);

            List<int> answerSpritesWithExtra =  new List<int>(answerSpritesID_CP);
            List<int> answerSpritesColorWithExtra = new List<int>(answerSpritesIDColor_CP);
            
            while (answerSpritesWithExtra.Count < answerSpriteAmount_CP+1)
            {
                int s = UnityEngine.Random.Range(0, allSprites.Count);
                if (!answerSpritesWithExtra.Contains(s)) answerSpritesWithExtra.Add(s);
            }
            while (answerSpritesColorWithExtra.Count < answerSpriteAmount_CP+1)
            {
                int s = UnityEngine.Random.Range(0, allColors.Count);
                if (!answerSpritesColorWithExtra.Contains(s)) answerSpritesColorWithExtra.Add(s);
            }
            

            for (int i = 0; i < answerSpritesWithExtra.Count; i++)
            {
                //player2SpritesOBJ_CP[i].GetComponent<Image>().sprite = allSprites[answerSpritesWithExtra[i]];
                //player2SpritesOBJ_CP[i].GetComponent<Image>().color = allColors[answerSpritesColorWithExtra[i]];
                player2SpritesOBJ_CP[i].GetComponent<Image>().sprite = allSprites[answerSpritesID_CP[i]];
                player2SpritesOBJ_CP[i].GetComponent<Image>().color = allColors[answerSpritesIDColor_CP[i]];
            }
        }
    }
    public void CheckRightColorMatch(int objNR, int colorNR)
    {
        Debug.LogError("checking...");
        if (objNR == colorNR)
        {
            completedAmount_CP++;
            Debug.LogError("win");
            if (completedAmount_CP == answerSpriteAmount)
            {

            }


        }
        else
        {
            lossesAmount_CP++;
            Debug.LogError("loss");
            if (lossesAmount_CP == 3)
            {

            }

        }
    }
    public IEnumerator WaitThenRestartColorPicker()
    {
        Debug.LogError("waiting..");
        yield return new WaitForSeconds(0.1f);
        PlayColorPickerGame();
    }

    #endregion

}
