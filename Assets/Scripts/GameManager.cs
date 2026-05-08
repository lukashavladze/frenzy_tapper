using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public TapButton[] buttons;

    public TMP_Text timerText;
    public TMP_Text scoreText;

    private float timer = 30f;

    private int score = 0;

    private bool gameEnded = false;

    private TapButton currentButton;

    private bool extraButtonActivated = false;

    public GameObject popupPrefab;

    public Canvas Canvas;

    private void Start()
    {
        // Disable all first
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetInactive();

            buttons[i].ResetButton();
        }

        // Activate 2 random buttons
        ActivateTwoButtons();

        UpdateUI();
    }

    private void Update()
    {
        if (gameEnded)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = 0;

            EndGame();
        }

        // Handle active button lifetimes
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].isActive && buttons[i].timerStarted)
            {
                buttons[i].currentLifetime -= Time.deltaTime;

                if (buttons[i].currentLifetime <= 0)
                {
                    Debug.Log(buttons[i].name + " expired");

                    EndGame();
                }
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        timerText.text = Mathf.CeilToInt(timer).ToString();

        scoreText.text = "Score: " + score;
    }

    void ActivateTwoButtons()
    {
        List<int> indexes = new List<int>();

        while (indexes.Count < 2)
        {
            int rand = Random.Range(0, buttons.Length);

            if (!indexes.Contains(rand))
            {
                indexes.Add(rand);
            }
        }

        for (int i = 0; i < indexes.Count; i++)
        {
            buttons[indexes[i]].Activate();
        }
    }

    public void TapButton(TapButton button)
    {
        if (gameEnded)
            return;

        if (!button.isActive)
            return;

        // Player switched buttons
        if (currentButton != null && currentButton != button)
        {
            HandleSwitch(button);
        }

        // First time selecting a button
        bool firstTapOnThisButton =
            button.currentTaps == 0;

        currentButton = button;

        button.Tap();

        score++;

        // Activate extra option after first tap
        // Only once in entire game
        if (firstTapOnThisButton &&
            !extraButtonActivated)
        {
            ActivateExtraButton(button);

            extraButtonActivated = true;
        }

        // OVERLOAD
        if (button.currentTaps > button.maxTaps)
        {
            SpawnPopup("OVERLOAD!", Color.magenta);

            EndGame();

            return;
        }
    }

    void HandleSwitch(TapButton newButton)
    {
        float previousPercent =
            (float)currentButton.currentTaps /
            currentButton.maxTaps;

        // BAD
        if (previousPercent < 0.5f)
        {
            timer -= 3f;

            SpawnPopup("BAD", Color.red);
        }

        // GOOD
        else if (previousPercent >= 0.65f &&
                 previousPercent < 0.85f)
        {
            timer += 3f;

            SpawnPopup("GOOD", Color.cyan);
        }

        // PERFECT
        else if (previousPercent >= 0.85f &&
                 previousPercent <= 1f)
        {
            timer += 8f;

            SpawnPopup("PERFECT!", Color.yellow);
        }

        // Deactivate previous
        currentButton.SetInactive();

        currentButton.ResetButton();

        // Find inactive buttons
        // Find inactive buttons
        List<TapButton> inactiveButtons =
            new List<TapButton>();

        for (int i = 0; i < buttons.Length; i++)
        {
            if (!buttons[i].isActive &&
                buttons[i] != newButton &&
                buttons[i] != currentButton)
            {
                inactiveButtons.Add(buttons[i]);
            }
        }

        // Activate one random inactive
        if (inactiveButtons.Count > 0)
        {
            int rand =
                Random.Range(0, inactiveButtons.Count);

            inactiveButtons[rand].Activate();
        }
    }

    void ActivateExtraButton(TapButton current)
    {
        List<TapButton> inactiveButtons =
            new List<TapButton>();

        for (int i = 0; i < buttons.Length; i++)
        {
            if (!buttons[i].isActive)
            {
                inactiveButtons.Add(buttons[i]);
            }
        }

        if (inactiveButtons.Count > 0)
        {
            int rand =
                Random.Range(0, inactiveButtons.Count);

            inactiveButtons[rand].Activate();
        }
    }

    void SpawnPopup(string message, Color color)
    {
        GameObject popup =
            Instantiate(
                popupPrefab,
                Canvas.transform
            );

        RectTransform rect =
            popup.GetComponent<RectTransform>();

        rect.anchoredPosition =
            new Vector2(0f, 200f);

        TMP_Text text =
            popup.GetComponent<TMP_Text>();

        text.text = message;

        text.color = color;
    }

    void EndGame()
    {
        gameEnded = true;

        Debug.Log("GAME OVER");
    }
}