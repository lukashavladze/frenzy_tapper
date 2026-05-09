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
    public int particleSpriteIndex;

    // maybe be deleted need to ensure
    public Sprite intactEgg;
    public Sprite crack1Egg;
    public Sprite crack2Egg;

    private int crackState = 0;

    private int lastAppliedState = -1;

    public Image crackOverlaySoft;
    public Image crackOverlayHard;


    private void Start()
    {
        originalScale = transform.localScale;
        image.sprite = intactEgg;
    }


    public void Activate()
    {
        isActive = true;

        //image.color = Color.green;

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

    //public void ApplyCrack(int state)
    //{
    //    if (state == lastAppliedState)
    //        return;

    //    lastAppliedState = state;
    //    crackState = state;

    //    Sprite targetSprite = intactEgg;

    //    if (crackState == 1)
    //        targetSprite = crack1Egg;
    //    else if (crackState >= 2)
    //        targetSprite = crack2Egg;

    //    // THIS WAS MISSING 👇
    //    image.sprite = targetSprite;
    //}

    //public void ApplyCrack(float percent)
    //{
    //    // 0 → 1 progress

    //    if (percent < 0.2f)
    //    {
    //        crackOverlay.color = new Color(1, 1, 1, 0);
    //        return;
    //    }

    //    if (percent < 0.6f)
    //    {
    //        crackOverlay.sprite = crack1Egg;

    //        float t = Mathf.InverseLerp(0.2f, 0.6f, percent);

    //        crackOverlay.color = new Color(1, 1, 1, t);
    //    }
    //    else
    //    {
    //        crackOverlay.sprite = crack2Egg;

    //        float t = Mathf.InverseLerp(0.6f, 1f, percent);

    //        crackOverlay.color = new Color(1, 1, 1, t);
    //    }
    //}

    public void ApplyCrack(float percent)
    {
        // FULL RESET safety
        if (percent <= 0f)
        {
            crackOverlaySoft.color = new Color(1, 1, 1, 0);
            crackOverlayHard.color = new Color(1, 1, 1, 0);
            return;
        }

        // PHASE 1 → soft cracks (0 → 0.5)
        float softT = Mathf.InverseLerp(0.1f, 0.5f, percent);
        crackOverlaySoft.color = new Color(1, 1, 1, Mathf.Clamp01(softT));

        // PHASE 2 → hard cracks (0.5 → 1)
        float hardT = Mathf.InverseLerp(0.5f, 1f, percent);
        crackOverlayHard.color = new Color(1, 1, 1, Mathf.Clamp01(hardT));
    }

    public void ResetButton()
    {
        crackState = 0;

        image.sprite = intactEgg;

        currentTaps = 0;

        if (crackOverlaySoft != null)
            crackOverlaySoft.color = new Color(1, 1, 1, 0);

        if (crackOverlayHard != null)
            crackOverlayHard.color = new Color(1, 1, 1, 0);

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