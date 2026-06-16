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

    private Vector3 originalScale;

    public ParticleSystem tapParticles;
    public int particleSpriteIndex;
    public ParticleSystem destroyParticles;

    public bool isDestroying;
    public bool resultEvaluated;

    public TMP_Text precisionTextCount;
    public TextMeshProUGUI rhythmTextResults;
    public SpriteRenderer crackRenderer;

    private Vector3 crackOriginalScale;

    [Range(0, 1)]
    public float crackProgress;

    public Material eggMaterial; // assign in inspector
    public float glowIntensity;
    public EggVisualState visualState;

    private static readonly int RevealID =
        Shader.PropertyToID("_revealamount");

    private static readonly int GlowID =
        Shader.PropertyToID("_glowcolor");


    public GameObject dragonPrefab;
    public GameObject brokenEggBottomPrefab;
    public GameObject gooLeakPrefab;

    [Header("Popup Text")]

    public GameObject popupTextPrefab;
    public Transform popupSpawnPoint;
    private int goodTextIndex = 0;


    private ObjectShake objectShake;

    public GameObject gooExplosionPrefab;


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
        objectShake = GetComponent<ObjectShake>();
        // emergency protection
        if (originalScale.x <= 0.001f)
        {
            originalScale =
                new Vector3(0.3f, 0.3f, 0.3f);

            transform.localScale =
                originalScale;
        }

        //crackRenderer = GetComponentInChildren<SpriteRenderer>();

        //crackOriginalScale =
        //    crackRenderer.transform.localScale;

        //Color c = crackRenderer.color;
        //c.a = 0f;
        //crackRenderer.color = c;
    }

    public enum EggVisualState
    {
        Inactive,
        Active,
        Cracking,
        Hatched,
        Destroyed
    }


    public void Activate()
    {
        isActive = true;
        missedRhythmHits = 0;
        pulseActive = false;
        goodTextIndex = 0;
        if (crackRenderer != null)
        {
            Color c = crackRenderer.color;
            c.a = 0f;
            crackRenderer.color = c;
        }

        if (eggMaterial != null)
        {
            eggMaterial.SetFloat(RevealID, 0f);
            eggMaterial.SetColor(GlowID, Color.black);
        }

        if (eggType == EggType.Rhythm)
        {
            StartCoroutine(RhythmPulse());
        }
        if (precisionTextCount != null)
        {
            precisionTextCount.gameObject.SetActive(
                eggType == EggType.Precision
            );
        }
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

        image.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        transform.localScale = originalScale;

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


    public void ShowDestroyed()
    {
        if (isDestroying)
            return;
        if (precisionTextCount != null)
        {
            precisionTextCount.gameObject.SetActive(false);
        }

        isDestroying = true;

        StartCoroutine(DestroySequence());
    }

    IEnumerator DestroySequence()
    {
        // hide crack overlay
        if (crackRenderer != null)
        {
            Color crackColor = crackRenderer.color;
            crackColor.a = 0f;
            crackRenderer.color = crackColor;
        }
        if (eggMaterial != null)
        {
            eggMaterial.SetFloat(RevealID, 0f);
            eggMaterial.SetColor(GlowID, Color.black);
        }

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
        objectShake.Shake(0.08f, 0.05f);

        // hit pause
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(0.06f);

        Time.timeScale = 1f;

        yield return new WaitForSeconds(0.6f);

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

        currentTaps = 0;
        image.enabled = true;
        image.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        UpdatePrecisionText();


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

                maxTaps = Random.Range(8, 12);

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

        if (eggMaterial != null)
        {
            eggMaterial.SetFloat(RevealID, 0f);

            eggMaterial.SetColor(
                GlowID,
                Color.black
            );
        }

        currentLifetime = lifetime;

        timerStarted = false;
        if (crackRenderer != null)
        {
            crackProgress = 0;
            Color c = crackRenderer.color;
            c.a = 0f;
            crackRenderer.color = c;
        }
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
            crackProgress = (float)currentTaps / maxTaps;
            crackProgress = Mathf.Clamp01(crackProgress);

            UpdateCrackVisual();

            // WRONG HIT
            if (!pulseActive)
            {
                ShowPopupText("OFFBEAT", Color.red);
                GameManager.Instance.AddTime(-1f);

                SpawnParticles();

                if (objectShake != null)
                {
                    objectShake.Shake(0.08f, 0.05f);
                }
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

                if (objectShake != null)
                {
                    objectShake.Shake(0.08f, 0.05f);
                }

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
        UpdateCrackVisual();
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

        if (objectShake != null)
        {
            objectShake.Shake(0.08f, 0.05f);

        }
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

    void UpdateCrackVisual()
    {
        if (eggMaterial == null)
            return;

        crackProgress =
            Mathf.Clamp01(
                (float)currentTaps / maxTaps
            );

        // Reveal reaches max at 50% progress
        float revealProgress =
            Mathf.Clamp01(crackProgress * 2f);

        float reveal =
            Mathf.Lerp(
                0f,
                0.55f,
                revealProgress
            );

        eggMaterial.SetFloat(
            RevealID,
            reveal
        );

        // Glow starts only AFTER reveal is complete
        float glowProgress =
            Mathf.InverseLerp(
                0.5f, // start glowing here
                1f,   // fully glowing here
                crackProgress
            );

        float glowIntensity =
            Mathf.Lerp(
                0f,
                4f,
                glowProgress
            );

        Color glowColor =
            Color.white * glowIntensity;

        eggMaterial.SetColor(
            GlowID,
            glowColor
        );
    }

    public void HatchEgg()
    {
        visualState = EggVisualState.Hatched;

        image.enabled = false;

        if (crackRenderer != null)
        {
            Color c = crackRenderer.color;
            c.a = 0f;
            crackRenderer.color = c;
        }

        Instantiate(dragonPrefab, transform.position, Quaternion.identity);
        if (precisionTextCount != null)
        {
            precisionTextCount.gameObject.SetActive(false);
        }

        if (objectShake != null)
        {
            objectShake.Shake(0.08f, 0.05f);
        }
    }

    public void BreakEgg()
    {
        visualState = EggVisualState.Destroyed;

        // hide main egg
        image.enabled = false;

        // hide cracks
        if (crackRenderer != null)
        {
            crackRenderer.enabled = false;
        }

        // GOO EXPLOSION
        if (gooExplosionPrefab != null)
        {
            GameObject goo =
                Instantiate(
                    gooExplosionPrefab,
                    transform.position,
                    Quaternion.identity,
                    transform.parent
                );

            goo.transform.localScale =
                transform.localScale;

            Destroy(goo, 1f); // animation length
        }
        

        // spawn broken egg
        GameObject crack =
            Instantiate(
                brokenEggBottomPrefab,
                transform.position,
                Quaternion.identity,
                transform.parent
            );

        crack.transform.localScale =
            transform.localScale;

        // remove broken egg later
        Destroy(crack, 0.7f);
        if (precisionTextCount != null)
        {
            precisionTextCount.gameObject.SetActive(false);
        }

        // restore original egg later
        StartCoroutine(
            RestoreEggAfterBreak()
        );

        if (objectShake != null)
        {
            objectShake.Shake(0.08f, 0.05f);
        }
    }

    IEnumerator RestoreEggAfterBreak()
    {
        yield return new WaitForSeconds(0.7f);

        // restore original egg
        image.enabled = true;

        image.color =
            new Color(
                0.5f,
                0.5f,
                0.5f,
                1f
            );

        if (crackRenderer != null)
        {
            crackRenderer.enabled = true;

            Color c = crackRenderer.color;
            c.a = 0f;
            crackRenderer.color = c;
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