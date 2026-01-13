using Alteruna;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class GameManager : Synchronizable
{
    [Header("UI References")]
    public TMP_Text StatusText;
    public Button[] OptionButtons; // Assign your 3 buttons here in the inspector

    [Header("Game Data")]
    public List<string> WordList = new List<string> { "Apple", "Banana", "Dragon", "Ghost", "Unity", "Pizza" };

    // Data to Sync
    private string _targetWord = "";
    private bool _isRoundActive = false;

    // --- ALTERUNA SYNC METHODS ---

    public override void AssembleData(Writer writer, byte LOD)
    {
        // Host packs the data to send to Client
        writer.Write(_targetWord);
        writer.Write(_isRoundActive);
    }

    public override void DisassembleData(Reader reader, byte LOD)
    {
        // Client unpacks the data
        string newWord = reader.ReadString();
        bool newActiveState = reader.ReadBool();

        // If the word changed, update the local game
        if (newWord != _targetWord || newActiveState != _isRoundActive)
        {
            _targetWord = newWord;
            _isRoundActive = newActiveState;
            UpdateGameState();
        }
    }

    // --- UNITY EVENTS ---

    private void Start()
    {
        // Hook up buttons to the clicking function
        // Avoid closure capture bug by copying 'btn' to a local variable inside the loop
        foreach (var btn in OptionButtons)
        {
            var localBtn = btn;
            localBtn.onClick.AddListener(() => OnOptionClicked(localBtn));
        }

        // Initialize UI
        SetButtonsActive(false);
        StatusText.text = "Waiting for host to start a round...";
    }

    private void Update()
    {
        // Required for Alteruna to work
        SyncUpdate();

        // HOST ONLY: Press Space to start a new round
        // NOTE: Synchronizable does not expose 'Avatar'. Instead we check the local player index
        // (we assume Host is Player index 0). This avoids using base.Avatar and fixes CS0117.
        if (Input.GetKeyDown(KeyCode.Space) && Multiplayer.Me != null && Multiplayer.Me.Index == 0)
        {
            StartNewRound();
        }
    }

    // --- GAME LOGIC ---

    // 1. Host picks a word
    public void StartNewRound()
    {
        if (WordList == null || WordList.Count == 0)
        {
            Debug.LogWarning("WordList is empty! Cannot start a new round.");
            return;
        }

        _targetWord = WordList[Random.Range(0, WordList.Count)];
        _isRoundActive = true;

        Commit(); // Send data to other players
        UpdateGameState();
    }

    // 2. Both players update their UI based on the new data
    void UpdateGameState()
    {
        if (!_isRoundActive)
        {
            StatusText.text = "Round is not active.";
            SetButtonsActive(false);
            return;
        }

        // Everyone resets colors
        foreach (var btn in OptionButtons)
        {
            btn.image.color = Color.white;
        }

        // Show the appropriate UI depending on host vs client
        SetupGuesserUI();
    }

    void SetupGuesserUI()
    {
        // Determine whether local player is Host. (We use index 0 as Host in this example.)
        bool amIHost = Multiplayer.Me != null && Multiplayer.Me.Index == 0;

        if (amIHost)
        {
            StatusText.text = "Tell the other player to guess: " + _targetWord;
            SetButtonsActive(false); // Host doesn't click buttons
        }
        else
        {
            StatusText.text = "Guess the word!";
            SetButtonsActive(true);
            PopulateButtons(_targetWord);
        }
    }

    // 3. Populate buttons with 1 correct word + 2 random wrongs
    void PopulateButtons(string correctWord)
    {
        if (OptionButtons == null || OptionButtons.Length == 0) return;

        List<string> options = new List<string>();
        options.Add(correctWord);

        while (options.Count < 3)
        {
            string random = WordList[Random.Range(0, WordList.Count)];
            if (!options.Contains(random)) options.Add(random);
        }

        // Shuffle (Fisher-Yates)
        for (int i = options.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            string tmp = options[i];
            options[i] = options[rand];
            options[rand] = tmp;
        }

        // Assign to buttons (guard against different button count)
        for (int i = 0; i < OptionButtons.Length; i++)
        {
            string text = (i < options.Count) ? options[i] : "";
            Text txtComp = OptionButtons[i].GetComponentInChildren<Text>();
            if (txtComp != null) txtComp.text = text;
            OptionButtons[i].image.color = Color.white;
            OptionButtons[i].interactable = true;
        }
    }

    void SetButtonsActive(bool isActive)
    {
        if (OptionButtons == null) return;
        foreach (var btn in OptionButtons)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(isActive);
            }
        }
    }

    // 4. Handle Clicking
    void OnOptionClicked(Button btn)
    {
        if (btn == null) return;

        Text txtComp = btn.GetComponentInChildren<Text>();
        if (txtComp == null) return;

        string clickedWord = txtComp.text;

        if (clickedWord == _targetWord)
        {
            // CORRECT!
            // Tell everyone the game is over via an RPC.
            ProcedureParameters args = new ProcedureParameters();
            // Use Multiplayer.Me.Name if available; fallback to a default label
            string playerName = (Multiplayer.Me != null && !string.IsNullOrEmpty(Multiplayer.Me.Name)) ? Multiplayer.Me.Name : "Player";
            args.Set("winnerName", playerName);
            BroadcastRemoteMethod("OnGameWin", args);
        }
        else
        {
            // WRONG - local feedback
            btn.image.color = Color.red;
            btn.interactable = false;
        }
    }

    // --- RPC EVENTS ---

    // This runs on EVERYONE'S computer when someone calls BroadcastRemoteMethod("OnGameWin")
    [SynchronizableMethod]
    public void OnGameWin(string winnerName)
    {
        StatusText.text = $"CORRECT! {winnerName} won. Round Over.";
        _isRoundActive = false;
        SetButtonsActive(false);
    }
}
