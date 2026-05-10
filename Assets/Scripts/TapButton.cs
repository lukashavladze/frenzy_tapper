using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TapButton : MonoBehaviour
{
    public int currentTaps;
    public int maxTaps;

    public bool isActive;

    public bool timerStarted = false;

    public float lifetime = 5f;

    public float currentLifetime;

    public Image image;

    //public TMP_Text label;

    private Vector3 originalScale;

    public ParticleSystem tapParticles;
    public int particleSpriteIndex;

    public GameObject crackPrefab;
    public Transform crackContainer;
    private Button button;

    private Vector2 lastCrackPosition;
    private bool hasPreviousCrack = false;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        FindObjectOfType<GameManager>()
            .TapButton(this);
    }


    private void Start()
    {
        originalScale = transform.localScale;
    }


    public void Activate()
    {
        isActive = true;

        //image.color = Color.green;
        image.color = Color.white;

        transform.localScale = Vector3.one * 1.1f;

        currentLifetime = lifetime;
    }

    public void SetInactive()
    {
        isActive = false;

        timerStarted = false;

        currentLifetime = lifetime;

        image.color = Color.gray;

        transform.localScale = Vector3.one;

    }

    void SpawnCrack()
    {
        float progress =
            (float)currentTaps / maxTaps;

        // EXISTING crack gets upgraded
        if (spawnedCracks.Count >= maxCracks)
        {
            int randomIndex =
                Random.Range(0, spawnedCracks.Count);

            Image existing =
                spawnedCracks[randomIndex];

            RectTransform existingRT =
                existing.GetComponent<RectTransform>();

            float biggerScale =
                existingRT.localScale.x + 0.08f;

            existingRT.localScale =
                Vector3.one * biggerScale;

            Color c = existing.color;

            c.a = Mathf.Clamp01(c.a + 0.03f);

            float brightness =
                Mathf.Lerp(
                    4f,
                    8f,
                    progress
                );

            c.r = brightness;
            c.g = brightness;
            c.b = brightness;



            existing.color = c;

            return;
        }

        // CREATE NEW CRACK
        GameObject crack =
            Instantiate(
                crackPrefab,
                crackContainer
            );

        Image img =
            crack.GetComponent<Image>();

        RectTransform rt =
            crack.GetComponent<RectTransform>();

        Vector2 spawnPos;

        // FIRST crack near center
        if (!hasPreviousCrack)
        {
            spawnPos =
                new Vector2(
                    Random.Range(-15f, 15f),
                    Random.Range(-15f, 15f)
                );

            hasPreviousCrack = true;
        }
        else
        {
            // grow from previous crack
            spawnPos =
                lastCrackPosition +
                Random.insideUnitCircle * 12f;
        }

        lastCrackPosition = spawnPos;

        rt.anchoredPosition = spawnPos;

        // SMALL START
        float startScale =
            Random.Range(0.25f, 0.40f);

        rt.localScale =
            Vector3.one * startScale;

        rt.localRotation =
            Quaternion.Euler(
                0,
                0,
                Random.Range(0f, 360f)
            );

        // START TRANSPARENT
        Color crackColor =
new Color(
    4f,
    4f,
    4f,
    0f
);

        img.color = crackColor;

        spawnedCracks.Add(img);

        StartCoroutine(
            AnimateCrack(
                img,
                rt,
                progress
            )
        );
    }

    IEnumerator AnimateCrack(
    Image img,
    RectTransform rt,
    float progress)
    {
        float duration = 0.28f;

        float timer = 0f;

        Vector3 startScale =
            rt.localScale;

        Vector3 targetScale =
            startScale *
            Mathf.Lerp(
    1.8f,
    3f,
    progress
);

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                timer / duration;

            // smooth fade in
            Color c = img.color;

            c.a = Mathf.Lerp(0f, 1f, t);

            img.color = c;

            // smooth grow
            rt.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }
    }

    private List<Image> spawnedCracks = new List<Image>();

    public int maxCracks = 8;

    public void ResetButton()
    {
        image.gameObject.SetActive(true);

        currentTaps = 0;

        foreach (Transform child in crackContainer)
        {
            Destroy(child.gameObject);
        }

        maxTaps = Random.Range(20, 31);

        currentLifetime = lifetime;

        timerStarted = false;
        spawnedCracks.Clear();

        hasPreviousCrack = false;

        lastCrackPosition = Vector2.zero;
    }

    public void Tap()
    {
        if (!isActive)
            return;

        if (!timerStarted)
        {
            timerStarted = true;
        }

        currentTaps++;
        SpawnCrack();
        StartCoroutine(PunchAnimation());


        Debug.Log(gameObject.name + " taps: " + currentTaps + "/" + maxTaps);

        SpawnParticles();
        CameraShake.Instance.Shake(0.05f, 0.03f);

        if (currentTaps >= maxTaps)
        {
            return;
        }
    }

    IEnumerator PunchAnimation()
    {
        transform.localScale = Vector3.one * 0.85f;

        yield return new WaitForSeconds(0.05f);

        if (isActive)
        {
            transform.localScale = Vector3.one * 1.1f;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }


    IEnumerator BreakAnimation()
    {
        transform.localScale = Vector3.one * 1.3f;

        yield return new WaitForSeconds(0.08f);

        transform.localScale = Vector3.one;
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