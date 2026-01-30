using Alteruna;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SynchronizableField] public int currentMinigame = 0;
    [SynchronizableField] public static int minigamesCompleted = 0;

    public bool minigameRunning = false;

    [SynchronizableField] private bool triggerReset = false;
    private bool triggerResetplayer0 = false;
    [SynchronizableField] private bool minigameChosen = false;

    static System.Random _R = new System.Random();
    static miniGames RandomEnumValue<miniGames>()
    {
        var v = Enum.GetValues(typeof(miniGames));
        return (miniGames)v.GetValue(_R.Next(v.Length));
    }

    private void Update()
    {
        // 1. If a reset is triggered, clean up and STOP here for this frame.
        if (triggerReset)
        {
            if(Multiplayer.Me.Index == 0 && triggerResetplayer0)ResetAllMinigames();

            if (Multiplayer.Me.Index == 1)
            {
                ResetAllMinigames();
                triggerReset = false;
                Commit();
            }
            return;
        }

        // 2. If a game is currently active, do not allow rerolling or picking.
        if (minigameRunning) return;

        if (Multiplayer.GetUsers().Count == 2)
        {
            if (Multiplayer.Me.Index == 0)
            {
                // 3. HOST ONLY: Only pick if a game hasn't been chosen yet.
                // This 'minigameChosen' flag acts as our lock.
                if (!minigameChosen)
                {
                    miniGames minigame = RandomEnumValue<miniGames>();

                    switch (minigame)
                    {
                        case miniGames.Keypads: currentMinigame = 2; break;
                        case miniGames.ColorPick: currentMinigame = 3; break;
                    }

                    minigameChosen = true; // LOCK the choice
                    Commit();
                }
            }

            // 4. NETWORK GATE: Don't let anyone start until the Host's choice arrives.
            if (!minigameChosen || currentMinigame == 0) return;

            // 5. START THE GAME
            // This only runs once because minigameRunning becomes true immediately.
            switch (currentMinigame)
            {
                case 2:
                    minigameRunning = true;
                    PlayKeypads();
                    break;
                case 3:
                    minigameRunning = true;
                    PlayColorPickerGame();
                    break;
            }

            if (alterunaMenu.activeSelf) alterunaMenu.SetActive(false);
        }
        else
        {
            // Reset if someone leaves
            if (minigameChosen || minigameRunning)
            {
                minigameChosen = false;
                minigameRunning = false;
                currentMinigame = 0;
            }
            if (!alterunaMenu.activeSelf) alterunaMenu.SetActive(true);
        }
    }

    private void ResetAllMinigames()
    {
        minigameRunning = false;
        minigameChosen = false;
        currentMinigame = 0;
        needsWait = true;

        player1UI.SetActive(false);
        player2UI.SetActive(false);
        player1UI_CP.SetActive(false);
        player2UI_CP.SetActive(false);

        answerSpritesID.Clear();
        orderSpritesID.Clear();
        answerSpritesID_CP = new List<int>();
        answerSpritesIDColor_CP = new List<int>();

        currentStageIndex = 0;
        lossesAmount = 0;
        completedAmount_CP = 0;
        lossesAmount_CP = 0;
        if (Multiplayer.Me.Index == 0) triggerResetplayer0 = false;

        // Reset Keypad Buttons
        foreach (var obj in player1SpritesOBJ)
        {
            if (obj != null) obj.GetComponent<Image>().color = Color.white;
        }

        // Reset Color Picker Drag Objects
        foreach (var obj in player1SpritesOBJ_CP)
        {
            if (obj != null)
            {
                UIDrag dragScript = obj.GetComponent<UIDrag>();
                if (dragScript != null)
                {
                    dragScript.ResetDragObject();
                }
            }
        }

        foreach (var obj in player1ColorOBJ_CP)
        {
            if (obj != null) obj.GetComponent<Image>().color = Color.white;
        }
    }

    #region keypadsGame
    [Header("--KeypadsGame--")]
    public List<Sprite> allSprites = new List<Sprite>();
    public GameObject player1UI;
    public GameObject[] player1SpritesOBJ;
    public GameObject player2UI;
    public GameObject[] player2SpritesOBJ;

    [SynchronizableField] bool needsWait = true;
    [SynchronizableField] private List<int> answerSpritesID = new List<int>();
    [SynchronizableField] private List<int> orderSpritesID = new List<int>();
    [SynchronizableField] private int orderSpriteAmount = 6;
    [SynchronizableField] private int answerSpriteAmount = 4;

    private int currentStageIndex = 0;
    private int lossesAmount = 0;

    public void PlayKeypads()
    {
        if (Multiplayer.Me.Index == 0)
        {
            needsWait = true;
            currentStageIndex = 0;

            // Host generates the correct IDs in the correct sequence
            List<int> tempAns = new List<int>();
            while (tempAns.Count < answerSpriteAmount)
            {
                int s = UnityEngine.Random.Range(0, allSprites.Count);
                if (!tempAns.Contains(s)) tempAns.Add(s);
            }
            answerSpritesID = tempAns;

            // Host generates the grid for Player 2 (Order Guy)
            List<int> tempOrder = new List<int>(new int[orderSpriteAmount]);
            var slots = Enumerable.Range(0, orderSpriteAmount).OrderBy(_ => UnityEngine.Random.value).Take(answerSpriteAmount).OrderBy(x => x).ToList();

            int ansIndex = 0;
            for (int i = 0; i < orderSpriteAmount; i++)
            {
                if (slots.Contains(i))
                {
                    tempOrder[i] = answerSpritesID[ansIndex];
                    ansIndex++;
                }
                else
                {
                    int randS;
                    do { randS = UnityEngine.Random.Range(0, allSprites.Count); }
                    while (answerSpritesID.Contains(randS) || tempOrder.Contains(randS));
                    tempOrder[i] = randS;
                }
            }
            orderSpritesID = tempOrder;
            needsWait = false;
            Commit();
        }

        // Network safety check
        if (needsWait || answerSpritesID.Count < answerSpriteAmount)
        {
            StartCoroutine(WaitThenRestart());
            return;
        }

        // --- PLAYER 1 (ANSWER GUY) LOGIC ---
        if (Multiplayer.Instance.Me.Index == 0)
        {
            player1UI.SetActive(true);

            // Shuffle the physical layout slots [0, 1, 2, 3]
            // This ensures the first button to press isn't always the first one in the UI array.
            List<int> layoutOrder = Enumerable.Range(0, answerSpriteAmount)
                                              .OrderBy(x => UnityEngine.Random.value)
                                              .ToList();

            for (int i = 0; i < answerSpriteAmount; i++)
            {
                // We map the i-th correct answer to a random button slot
                int buttonSlot = layoutOrder[i];

                // Apply visual and logic to the randomized button slot
                player1SpritesOBJ[buttonSlot].GetComponent<Image>().sprite = allSprites[answerSpritesID[i]];
                player1SpritesOBJ[buttonSlot].GetComponent<KeypadButton>().SetIndex(i);

                // Ensure color is reset
                player1SpritesOBJ[buttonSlot].GetComponent<Image>().color = Color.white;
            }
        }
        // --- PLAYER 2 (ORDER GUY) LOGIC ---
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
                if (Multiplayer.Me.Index == 0)
                {
                    minigamesCompleted++;
                    triggerReset = true;
                    triggerResetplayer0 = true;
                    Commit();
                }
            }
        }
        else
        {
            lossesAmount++;
            if (lossesAmount < 3)
            {
                currentStageIndex = 0;
                foreach (var obj in player1SpritesOBJ) obj.GetComponent<Image>().color = Color.white;
            }
        }
    }

    public IEnumerator WaitThenRestart() { yield return new WaitForSeconds(0.1f); PlayKeypads(); }
    #endregion

    #region colorpickergame
    [Header("--ColorPickerGame--")]
    public List<Color> allColors = new List<Color>();
    public GameObject player1UI_CP;
    public GameObject[] player1SpritesOBJ_CP;
    public GameObject[] player1ColorOBJ_CP;
    public GameObject player2UI_CP;
    public GameObject[] player2SpritesOBJ_CP;

    [SynchronizableField] public List<int> answerSpritesID_CP = new List<int>();
    [SynchronizableField] public List<int> answerSpritesIDColor_CP = new List<int>();
    [SynchronizableField] private int answerSpriteAmount_CP = 4;

    private int completedAmount_CP = 0;
    private int lossesAmount_CP = 0;

    public void PlayColorPickerGame()
    {
        if (Multiplayer.Me.Index == 0)
        {
            needsWait = true;
            completedAmount_CP = 0;
            List<int> tempS = new List<int>();
            List<int> tempC = new List<int>();

            while (tempS.Count < answerSpriteAmount_CP)
            {
                int s = UnityEngine.Random.Range(0, allSprites.Count);
                if (!tempS.Contains(s)) tempS.Add(s);
            }
            while (tempC.Count < answerSpriteAmount_CP)
            {
                int c = UnityEngine.Random.Range(0, allColors.Count);
                if (!tempC.Contains(c)) tempC.Add(c);
            }

            answerSpritesID_CP = tempS;
            answerSpritesIDColor_CP = tempC;
            needsWait = false;
            Commit();
        }

        if (needsWait || answerSpritesID_CP.Count < answerSpriteAmount_CP)
        {
            StartCoroutine(WaitThenRestartColorPicker());
            return;
        }

        if (Multiplayer.Instance.Me.Index == 0)
        {
            player1UI_CP.SetActive(true);
            for (int i = 0; i < answerSpriteAmount_CP; i++)
            {
                player1SpritesOBJ_CP[i].GetComponent<Image>().sprite = allSprites[answerSpritesID_CP[i]];
                player1ColorOBJ_CP[i].GetComponent<Image>().color = allColors[answerSpritesIDColor_CP[i]];
                player1ColorOBJ_CP[i].GetComponent<ColorZone>().SetAnswerId(i);
                player1SpritesOBJ_CP[i].GetComponent<UIDrag>().SetAnswerId(i);
            }
        }
        else if (Multiplayer.Instance.Me.Index == 1)
        {
            player2UI_CP.SetActive(true);
            List<int> p2Sprites = new List<int>(answerSpritesID_CP);
            List<int> p2Colors = new List<int>(answerSpritesIDColor_CP);

            while (p2Sprites.Count < answerSpriteAmount_CP + 1)
            {
                int s = UnityEngine.Random.Range(0, allSprites.Count);
                if (!p2Sprites.Contains(s)) p2Sprites.Add(s);
            }
            while (p2Colors.Count < answerSpriteAmount_CP + 1)
            {
                int c = UnityEngine.Random.Range(0, allColors.Count);
                if (!p2Colors.Contains(c)) p2Colors.Add(c);
            }

            for (int i = 0; i < p2Sprites.Count; i++)
            {
                if (i < player2SpritesOBJ_CP.Length)
                {
                    player2SpritesOBJ_CP[i].GetComponent<Image>().sprite = allSprites[p2Sprites[i]];
                    player2SpritesOBJ_CP[i].GetComponent<Image>().color = allColors[p2Colors[i]];
                }
            }
        }
    }

    public void CheckRightColorMatch(int objNR, int colorNR)
    {
        if (objNR == colorNR)
        {
            completedAmount_CP++;
            if (completedAmount_CP >= answerSpriteAmount_CP)
            {
                if (Multiplayer.Me.Index == 0)
                {
                    minigamesCompleted++;
                    triggerReset = true;
                    triggerResetplayer0 = true; 
                    Commit();
                }
            }
        }
        else { lossesAmount_CP++; }
    }

    public IEnumerator WaitThenRestartColorPicker() { yield return new WaitForSeconds(0.1f); PlayColorPickerGame(); }
    #endregion
}