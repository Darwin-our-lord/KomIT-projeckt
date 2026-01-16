using Alteruna;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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

    // Synchronize the target word across all clients:
    [SynchronizableField]
    public string TargetWord = "Press Space to Start";


    static System.Random _R = new System.Random();
    static miniGames RandomEnumValue<miniGames>()
    {
        var v = Enum.GetValues(typeof(miniGames));
        return (miniGames)v.GetValue(_R.Next(v.Length));
    }

    private void Update()
    {
        if(Multiplayer.GetUsers().Count == 2)
        {
            miniGames minigame = RandomEnumValue<miniGames>();
            switch (minigame)
            {
                case miniGames.colorCount:
                    PlayColorCount();
                    break;


                default:
                    Debug.LogError("no minigame was selected");
                    
                    break;
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
    public List<Sprite> answerSprites = new List<Sprite>();

    [SynchronizableField]
    public List<Sprite> orderSprites = new List<Sprite>();

    [SynchronizableField]
    public GameObject player1UI;
    public void PlayKeypads()
    {
        answerSprites.Clear();
        while (answerSprites.Count < 4)
        {
            Sprite s = allSprites[UnityEngine.Random.Range(0, allSprites.Count)];
            if (!answerSprites.Contains(s)) answerSprites.Add(s);
        }

        orderSprites = new List<Sprite>(new Sprite[6]);

        var slots = Enumerable.Range(0, 6).OrderBy(_ => UnityEngine.Random.value).Take(4).OrderBy(x => x).ToList();

        int ansIndex = 0;
        for (int i = 0; i < 6; i++)
        {
            if (slots.Contains(i))
            {
                orderSprites[i] = answerSprites[ansIndex++];
            }
            else
            {
                do { orderSprites[i] = allSprites[UnityEngine.Random.Range(0, allSprites.Count)]; }
                while (answerSprites.Contains(orderSprites[i]) || orderSprites.IndexOf(orderSprites[i]) != i);
            }
        }
        Commit();

        if (Multiplayer.Instance.Me.Index == 0)
        {




        }
        else if (Multiplayer.Instance.Me.Index == 1)
        {



            
                

        }


    }

    #endregion
}
