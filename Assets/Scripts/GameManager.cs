using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using static TapButton;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public TapButton[] buttons;

    public TMP_Text timerText;
    public TMP_Text scoreText;
    public TMP_Text comboText;

    public GameObject popupPrefab;

    public Canvas Canvas;

    private int combo = 1;

    private Vector3 comboOriginalScale;

    private bool comboAnimating = false;

    private float comboAnimTimer = 0f;

    private float timer = 30f;

    private int score = 0;

    private bool gameEnded = false;

    private TapButton currentButton;

    private bool extraButtonActivated = false;

    private TapButton lastUsedButton;
    public static GameManager Instance;

    private void Start()
    {
        Instance = this;
        comboOriginalScale =
            comboText.transform.localScale;

        // Disable all first
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetInactive();

            buttons[i].ResetButton();
        }

        // Activate 2 random buttons
        ActivateInitialButtons();

        UpdateUI();
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }

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
            if (buttons[i].isActive &&
                buttons[i].timerStarted)
            {
                buttons[i].currentLifetime -=
                    Time.deltaTime;

                if (buttons[i].currentLifetime <= 0)
                {
                    Debug.Log(
                        buttons[i].name + " expired"
                    );
                    ExpireButton(buttons[i]);
                }
            }
        }

        UpdateComboAnimation();

        UpdateUI();
    }

    void UpdateUI()
    {
        timerText.text =
            Mathf.CeilToInt(timer).ToString();

        scoreText.text =
            "Score: " + score;

        comboText.text =
            "x" + combo;

        if (combo < 2)
        {
            comboText.color = Color.white;
        }
        else if (combo < 3)
        {
            comboText.color = Color.cyan;
        }
        else if (combo < 4)
        {
            comboText.color = Color.green;
        }
        else if (combo < 5)
        {
            comboText.color = Color.yellow;
        }
        else if (combo < 6)
        {
            comboText.color =
                new Color(1f, 0.5f, 0f);
        }
        else
        {
            comboText.color = Color.red;
            comboText.outlineColor = Color.yellow;
        }
    }

    public void AddTime(float amount)
    {
        timer += amount;

        if (timer < 0)
        {
            timer = 0;
        }
    }

    void UpdateComboAnimation()
    {
        // Base pulse always active
        float pulseSpeed = 8f;

        float pulseAmount = 0.15f;

        // Stronger pulse on high combo
        if (combo >= 5)
        {
            pulseSpeed = 14f;

            pulseAmount = 0.25f;
        }

        float pulse =
            1f +
            Mathf.Sin(Time.time * pulseSpeed)
            * pulseAmount;

        comboText.transform.localScale =
            comboOriginalScale * pulse;

        // Punch animation when combo increases
        if (comboAnimating)
        {
            comboAnimTimer += Time.deltaTime;

            float progress =
                comboAnimTimer / 0.12f;

            float extraScale =
                Mathf.Lerp(
                    1.2f,
                    0f,
                    progress
                );

            comboText.transform.localScale =
                comboOriginalScale *
                (pulse + extraScale);

            if (progress >= 1f)
            {
                comboAnimating = false;
            }
        }
    }

    void ActivateInitialButtons()
    {
        List<TapButton> pool = new List<TapButton>(buttons);

        for (int i = 0; i < 3; i++)
        {
            int rand = Random.Range(0, pool.Count);

            pool[rand].Activate();

            pool.RemoveAt(rand);
        }
    }

    public void TapButton(TapButton button)
    {

        if (gameEnded)
            return;

        if (!button.isActive)
            return;

        // Player switched buttons
        if (currentButton != null &&
            currentButton != button)
        {
            HandleSwitch(button);
        }

        // First time selecting a button
        bool firstTapOnThisButton =
            button.currentTaps == 0;

        currentButton = button;

        button.Tap();
        if (button.eggType == EggType.Precision && button.currentTaps == button.maxTaps + 1)
        {
            button.resultEvaluated = true;
            button.isActive = false;
            SpawnPopup("OVERLOAD!", Color.magenta);
            timer -= 1f;
            ResetCombo();
            button.ShowDestroyed();
            StartCoroutine(DelayedExpire(button)
        );

            return;
        }
        // crack

        float percent = (float)button.currentTaps / button.maxTaps;

        // only update if changed
        button.ApplyCrack(percent);

        int points = 1;

        if (button.eggType == EggType.Hidden)
        {
            points = 3;
        }

        score += points * combo;


        // Activate extra option
        if (firstTapOnThisButton &&
            !extraButtonActivated)
        {
            ActivateExtraButton(button);

            extraButtonActivated = true;
        }

    }

    public IEnumerator DelayedExpire(TapButton button)
    {
        yield return new WaitForSeconds(0.7f);

        ExpireButton(button);
    }


    void EvaluateButtonResult(TapButton button)
    {

        // RHYTHM DOES NOT USE EVALUATION
        if (button.eggType == EggType.Rhythm)
        {
            return;
        }

        float percent =
            (float)button.currentTaps /
            button.maxTaps;


        // =========================
        // PRECISION EGG
        // =========================
        if (button.eggType == EggType.Precision)
        {  

            // PERFECT
            if (button.currentTaps == button.maxTaps)
            {
                timer += 10f;
                AddCombo();
                SpawnPopup("PERFECT!", Color.yellow);
                button.ShowDestroyed();
            }
            // MISS
            else if (button.currentTaps < button.maxTaps)
            {
                SpawnPopup("MISS",Color.red);
                ResetCombo();
                timer -= 1f;
            }

            return;
        }          

        // =========================
        // HIDDEN EGG
        // =========================
        if (button.eggType ==
            EggType.Hidden)
        {
            // OVERLOAD
            if (button.currentTaps > button.maxTaps)
            {
                SpawnPopup(
                    "OVERLOAD!",
                    Color.magenta
                );
                timer -= 1f;
                ResetCombo();
                button.ShowDestroyed();
                return;
            }

            // PERFECT
            if (percent > 0.90f)
            {
                timer += 7f;
                AddCombo();
                SpawnPopup("PERFECT!", Color.yellow);
                button.ShowDestroyed();
            }
            // GOOD
            else if (percent >= 0.5f)
            {
                timer += 1f;
                AddCombo();
                SpawnPopup("GOOD", Color.cyan);
            }
            // BAD
            else
            {
                timer -= 1f;
                ResetCombo();
                SpawnPopup( "BAD", Color.red);
            }
            return;
        }

        // =========================
        // NORMAL EGG
        // =========================

        // OVERLOAD
        if (button.currentTaps > button.maxTaps)
        {
            SpawnPopup("OVERLOAD!", Color.magenta);
            timer -= 1f;
            ResetCombo();
            button.ShowDestroyed();
            return;
        }

        // PERFECT
        if (percent > 0.9f)
        {
            timer += 7f;
            AddCombo();
            SpawnPopup("PERFECT!", Color.yellow);
            button.ShowDestroyed();
            return;
        }

        if (percent > 0.5f)
        {
            timer += 1f;
            AddCombo();
            SpawnPopup("GOOD", Color.cyan);
            return;
        }



        // BAD
        if (percent < 0.5f)
        {
            timer -= 1f;
            ResetCombo();
            SpawnPopup("BAD",Color.red);
            return;
        }

    }

    void EnsureThreeActive(TapButton exclude = null)
    {
        List<TapButton> inactive = new List<TapButton>();
        int activeCount = 0;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].isActive)
                activeCount++;
            else
                inactive.Add(buttons[i]);
        }

        while (activeCount < 3 && inactive.Count > 0)
        {
            int rand = Random.Range(0, inactive.Count);

            TapButton candidate = inactive[rand];

            // ❗ prevent reactivating last used
            if (candidate == lastUsedButton || candidate == exclude)
            {
                inactive.RemoveAt(rand);
                continue;
            }

            candidate.Activate();

            inactive.RemoveAt(rand);

            activeCount++;
        }
    }

    void HandleSwitch(TapButton newButton)
    {
        // Evaluate old
        if (!currentButton.resultEvaluated)
        {
            currentButton.resultEvaluated = true;

            EvaluateButtonResult(currentButton);
        }

        // Remember it (important!)
        lastUsedButton = currentButton;

        // Deactivate old
        currentButton.SetInactive();
        currentButton.ResetButton();

        // Maintain 3 active buttons
        EnsureThreeActive(newButton);
    }

    void ActivateExtraButton(
        TapButton current)
    {
        List<TapButton> inactiveButtons =
            new List<TapButton>();

        for (int i = 0; i < buttons.Length; i++)
        {
            if (!buttons[i].isActive)
            {
                inactiveButtons.Add(
                    buttons[i]
                );
            }
        }

        if (inactiveButtons.Count > 0)
        {
            int rand =
                Random.Range(
                    0,
                    inactiveButtons.Count
                );

            inactiveButtons[rand]
                .Activate();
        }
    }

    void ExpireButton(TapButton expiredButton)
    {
        if (!expiredButton.isActive && expiredButton.isDestroying)
        {
            return;
        }

        if (!expiredButton.resultEvaluated)
        {
            expiredButton.resultEvaluated = true;

            EvaluateButtonResult(expiredButton);
        }

        lastUsedButton = expiredButton;

        expiredButton.SetInactive();
        expiredButton.ResetButton();

        if (currentButton == expiredButton)
            currentButton = null;

        EnsureThreeActive();
    }

    void AddCombo()
    {
        combo++;

        combo = Mathf.Clamp(
            combo,
            1,
            999
        );

        comboAnimating = true;

        comboAnimTimer = 0f;
    }

    void ResetCombo()
    {
        combo = 1;
    }

    void SpawnPopup(
        string message,
        Color color)
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