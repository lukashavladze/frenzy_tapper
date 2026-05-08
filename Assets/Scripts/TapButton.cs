using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TapButton : MonoBehaviour
{
    public int currentTaps;
    public int maxTaps;

    public bool isActive;

    public bool timerStarted = false;

    public float lifetime = 5f;

    public float currentLifetime;

    public Image image;

    public TMP_Text label;

    private Vector3 originalScale;

    private bool animating = false;

    private float animationTimer = 0f;

    public ParticleSystem tapParticles;



    private void Start()
    {
        originalScale = transform.localScale;
    }


    private void Update()
    {
        if (!animating)
            return;

        animationTimer += Time.deltaTime;

        float progress = animationTimer / 0.12f;

        // Bounce curve
        float scale =
            Mathf.Lerp(
                0.8f,
                1.15f,
                Mathf.Sin(progress * Mathf.PI)
            );

        transform.localScale =
            originalScale * scale;

        if (progress >= 1f)
        {
            animating = false;

            transform.localScale = originalScale;
        }
    }


    public void Activate()
    {
        isActive = true;

        image.color = Color.green;

        transform.localScale = Vector3.one * 1.1f;

        currentLifetime = lifetime;
    }

    public void SetInactive()
    {
        isActive = false;

        timerStarted = false;

        currentLifetime = lifetime;

        image.color = Color.red;

        transform.localScale = Vector3.one;
    }

    public void ResetButton()
    {
        currentTaps = 0;

        maxTaps = Random.Range(20, 31);

        currentLifetime = lifetime;

        timerStarted = false;
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
        

        Debug.Log(gameObject.name + " taps: " + currentTaps + "/" + maxTaps);

        PlayTapAnimation();
        SpawnParticles();
    }

    void PlayTapAnimation()
    {
        animating = true;

        animationTimer = 0f;
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

        particles.Play();

        Destroy(
            particles.gameObject,
            2f
        );
    }
}