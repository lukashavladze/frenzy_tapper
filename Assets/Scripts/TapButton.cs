using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TapButton : MonoBehaviour
{
    public int currentTaps;
    public int maxTaps;

    public bool isActive;

    public bool timerStarted = false;

    public float lifetime = 5f;

    public float currentLifetime;

    public SpriteRenderer image;

    //public TMP_Text label;

    private Vector3 originalScale;

    public ParticleSystem tapParticles;
    public int particleSpriteIndex;
    public ParticleSystem destroyParticles;

    // maybe be deleted need to ensure
    public Sprite intactEgg;

    public SpriteRenderer crackSoft;
    public SpriteRenderer crackMedium;
    public SpriteRenderer crackHard;
    public SpriteRenderer crackDestroyed;

    public bool isDestroying;
    public bool resultEvaluated;

    public TextMeshProUGUI precisionTextCount;
    public TextMeshProUGUI rhythmTextResults;

    [Header("Popup Text")]

    public GameObject popupTextPrefab;
    public Transform popupSpawnPoint;
    private int goodTextIndex = 0;


    public enum EggType
    {
        Precision,
        Normal,
        Rhythm,
        Hidden
    }

    public EggType eggType;

    [Header("Rhythm")]

    public bool pulseActive;

    public int missedRhythmHits;


    private void Start()
    {
        originalScale = transform.localScale;
    }


    public void Activate()
    {
        isActive = true;
        missedRhythmHits = 0;
        pulseActive = false;
        goodTextIndex = 0;
        if (eggType == EggType.Rhythm)
        {
            StartCoroutine(RhythmPulse());
        }

        //image.color = Color.green;
        image.color = Color.white;

        transform.localScale = originalScale * 1.1f;

        currentLifetime = lifetime;
        UpdatePrecisionText();
    }

    public void SetInactive()
    {
        isActive = false;

        timerStarted = false;

        currentLifetime = lifetime;

        image.color = Color.gray;

        transform.localScale = originalScale;

        if (crackSoft != null)
            crackSoft.color = new Color(1, 1, 1, 0);

        if (crackMedium != null)
            crackMedium.color = new Color(1, 1, 1, 0);

        if (crackHard != null)
            crackHard.color = new Color(1, 1, 1, 0);

        if (crackDestroyed != null)
            crackDestroyed.color = new Color(1, 1, 1, 0);

        if (precisionTextCount != null)
            precisionTextCount.gameObject.SetActive(false);
    }

    IEnumerator RhythmPulse()
    {
        while (isActive &&
               eggType == EggType.Rhythm)
        {
            pulseActive = true;

            transform.localScale =
                originalScale * 1.25f;

            yield return new WaitForSeconds(0.25f);

            pulseActive = false;

            transform.localScale =
                originalScale * 1.1f;

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void ApplyCrack(float percent)
    {
        if (eggType == EggType.Hidden)
            return;

        percent = Mathf.Clamp01(percent);

        // SOFT
        float softT =
            Mathf.InverseLerp(
                0.15f,
                0.40f,
                percent
            );

        crackSoft.color =
            new Color(
                1,
                1,
                1,
                Mathf.Clamp01(softT)
            );

        // MEDIUM
        float mediumT =
            Mathf.InverseLerp(
                0.40f,
                0.65f,
                percent
            );

        crackMedium.color =
            new Color(
                1,
                1,
                1,
                Mathf.Clamp01(mediumT)
            );

        // HARD
        float hardT =
            Mathf.InverseLerp(
                0.65f,
                0.90f,
                percent
            );

        crackHard.color =
            new Color(
                1,
                1,
                1,
                Mathf.Clamp01(hardT)
            );
    }

    public void ShowDestroyed()
    {
        if (isDestroying)
            return;

        isDestroying = true;

        StartCoroutine(DestroySequence());
    }

    IEnumerator DestroySequence()
    {
        // hide crack layers
        crackSoft.color = new Color(1, 1, 1, 0);
        crackMedium.color = new Color(1, 1, 1, 0);
        crackHard.color = new Color(1, 1, 1, 0);

        // hide egg
        image.color = new Color(1, 1, 1, 0);

        // particles
        if (destroyParticles != null)
        {
            Vector3 spawnPos =
                transform.position +
                new Vector3(0.1f, 0.1f, 0f);

            ParticleSystem p =
                Instantiate(
                    destroyParticles,
                    spawnPos,
                    Quaternion.identity,
                    transform.parent
                );

            p.Play();

            Destroy(p.gameObject, 2f);
        }

        // shake
        CameraShake.Instance.Shake(0.15f, 0.12f);

        // SMALL HIT PAUSE
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(0.06f);

        Time.timeScale = 1f;

        // small extra delay
        yield return new WaitForSeconds(0.20f);

        // show destroyed egg
        crackDestroyed.color = Color.white;

        yield return new WaitForSeconds(0.4f);

        SetInactive();

        ResetButton();

        isDestroying = false;
    }


    void ShowPopupText(string message, Color color)
    {
        if (popupTextPrefab == null)
            return;

        Transform spawn =
            popupSpawnPoint != null
            ? popupSpawnPoint
            : transform;

        GameObject obj =
            Instantiate(
                popupTextPrefab,
                spawn
            );

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.localPosition = Vector3.zero;

        TMP_Text txt =
            obj.GetComponent<TMP_Text>();

        txt.text = message;
        txt.color = color;
    }

    void UpdatePrecisionText()
    {
        if (precisionTextCount == null)
            return;

        if (eggType != EggType.Precision)
        {
            precisionTextCount.gameObject.SetActive(false);
            return;
        }

        precisionTextCount.gameObject.SetActive(true);

        precisionTextCount.text =
            currentTaps +
            " / " +
            maxTaps;
    }

    public void ResetButton()
    {
        resultEvaluated = false;
        missedRhythmHits = 0;
        pulseActive = false;

        image.sprite = intactEgg;

        currentTaps = 0;
        UpdatePrecisionText();

        if (crackSoft != null)
            crackSoft.color = new Color(1, 1, 1, 0);

        if (crackMedium != null)
            crackMedium.color = new Color(1, 1, 1, 0);

        if (crackHard != null)
            crackHard.color = new Color(1, 1, 1, 0);

        if (crackDestroyed != null)
            crackDestroyed.color = new Color(1, 1, 1, 0);

        switch (eggType)
        {
            case EggType.Precision:

                int[] precisionValues = { 15, 20 };

                maxTaps =
                    precisionValues[
                        Random.Range(0, precisionValues.Length)
                    ];

                if (maxTaps == 15)
                    lifetime = 3f;
                else if (maxTaps == 20)
                    lifetime = 4f;

                break;

            case EggType.Normal:

                maxTaps = Random.Range(25, 40);

                lifetime = 5f;

                break;

            case EggType.Rhythm:

                maxTaps = 5;

                lifetime = 7f;

                break;

            case EggType.Hidden:

                maxTaps = Random.Range(10, 40);

                lifetime = 4f;

                break;
        }

        currentLifetime = lifetime;

        timerStarted = false;
    }
    

    public void Tap()
    {
        if (!isActive)
            return;

        // prevent extra taps while destroying
        if (isDestroying)
            return;

        if (!timerStarted)
        {
            timerStarted = true;
        }

        // =========================
        // RHYTHM EGG
        // =========================
        if (eggType == EggType.Rhythm)
        {
            // EVERY TAP COUNTS
            currentTaps++;

            // WRONG HIT
            if (!pulseActive)
            {
                ShowPopupText("OFFBEAT", Color.red);
                GameManager.Instance.AddTime(-1f);

                SpawnParticles();

                CameraShake.Instance.Shake(
                    0.03f,
                    0.02f
                );
            }
            // CORRECT HIT
            else
            {
                
                string[] goodTexts = {"GOOD", "NICE!", "GREAT!", "PERFECT!", "INSANE!"};
                string SelectedText = goodTexts[goodTextIndex];

                Color[] rhythmColors ={Color.cyan, Color.yellow, new Color(1f, 0.4f, 1f), Color.white};

                Color randomColor = rhythmColors[Random.Range(0, rhythmColors.Length)];

                ShowPopupText(SelectedText, randomColor);

                goodTextIndex++;
                if (goodTextIndex >= goodTexts.Length)
                {
                    goodTextIndex = 0;
                }

                GameManager.Instance.AddTime(2f);

                SpawnParticles();

                CameraShake.Instance.Shake(
                    0.05f,
                    0.03f
                );

                StartCoroutine(PunchAnimation());
            }

            Debug.Log(
                gameObject.name +
                " rhythm taps: " +
                currentTaps +
                "/" +
                maxTaps
            );

            // DESTROY AFTER TOTAL 5 TAPS
            if (currentTaps >= maxTaps)
            {
                resultEvaluated = true;

                isActive = false;

                ShowDestroyed();

                StartCoroutine(
                    GameManager.Instance
                        .DelayedExpire(this)
                );
            }

            return;
        }

        // =========================
        // NORMAL / HIDDEN / PRECISION
        // =========================

        currentTaps++;
        if (eggType == EggType.Precision)
        {
            UpdatePrecisionText();
        }
        

        StartCoroutine(PunchAnimation());

        Debug.Log(
            gameObject.name +
            " taps: " +
            currentTaps +
            "/" +
            maxTaps
        );

        SpawnParticles();

        CameraShake.Instance.Shake(
            0.05f,
            0.03f
        );
    }

    IEnumerator PunchAnimation()
    {
        if (eggType == EggType.Rhythm)
            yield break;

        transform.localScale =
            originalScale * 0.85f;

        yield return new WaitForSeconds(0.05f);

        if (isActive)
        {
            transform.localScale =
               originalScale * 1.1f;
        }
        else
        {
            transform.localScale =
                originalScale;
        }
    }


    void OnMouseDown()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TapButton(this);
            Debug.Log("CLICK WORKED");
        }
    }
    

    void SpawnParticles()
    {
        if (tapParticles == null)
            return;

        Vector3 worldPos =
            Camera.main.ScreenToWorldPoint(
                RectTransformUtility.WorldToScreenPoint(
                    Camera.main,
                    transform.position
                )
            );

        worldPos.z = 0f;

        ParticleSystem particles =
            Instantiate(
                tapParticles,
                worldPos,
                Quaternion.identity
            );

        // Change sprite
        var textureSheet =
            particles.textureSheetAnimation;

        textureSheet.SetSprite(
            0,
            textureSheet.GetSprite(
                particleSpriteIndex
            )
        );

        particles.Play();

        Destroy(
            particles.gameObject,
            2f
        );
    }
}