using Alteruna;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : AttributesSync
{
    [Header("UI References")]
    public TMP_Text StatusText;
    public GameObject canvas;
    public GameObject alterunaMenu;

    [Header("Game Data")]
    public List<string> WordList = new List<string> { "Apple", "Banana", "Dragon", "Ghost", "Unity", "Pizza" };
    public List<string> EmojiList = new List<string> { "gang gang" };

    // Synchronize the target word across all clients:
    [SynchronizableField]
    public string TargetWord = "Press Space to Start";

    private void Update()
    {
        if(Multiplayer.GetUsers().Count == 2)
        {
            // Only the host (player index 0) picks a new word:
            if (Input.GetKeyDown(KeyCode.Space)
                && Multiplayer.Instance != null
                && Multiplayer.Instance.Me.Index == 0)
            {
                PickNewWord();
            }
            UpdateUI();
            if(!canvas.activeSelf) canvas.SetActive(true);
            if (alterunaMenu.activeSelf) alterunaMenu.SetActive(false);
        }
        else
        {
            if (canvas.activeSelf) canvas.SetActive(false);
            if (!alterunaMenu.activeSelf) alterunaMenu.SetActive(true);
        }
        

    }

    private void PickNewWord()
    {
        if (WordList.Count > 0)
        {
            TargetWord = WordList[Random.Range(0, WordList.Count)];
            // Inform Alteruna to sync our changed data to all clients:
            Commit();  // triggers network sync:contentReference[oaicite:8]{index=8}:contentReference[oaicite:9]{index=9}
        }
    }

    private void UpdateUI()
    {
        if (StatusText != null)
        {
            StatusText.text = "Target Word: " + TargetWord;
        }
    }
}
