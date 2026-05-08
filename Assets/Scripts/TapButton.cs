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

    public Image image;

    public TMP_Text label;

    private Vector3 originalScale;

    public ParticleSystem tapParticles;



    private void Start()
    {
        originalScale = transform.localScale;
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
        StartCoroutine(PunchAnimation());


        Debug.Log(gameObject.name + " taps: " + currentTaps + "/" + maxTaps);

        SpawnParticles();
        CameraShake.Instance.Shake(0.05f, 0.03f);
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