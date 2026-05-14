using UnityEngine;
using TMPro;

public class PopupText : MonoBehaviour
{
    public float moveSpeed = 150f;

    public float lifetime = 1f;

    private TMP_Text text;

    private RectTransform rect;

    private float timer;

    public AnimationCurve scaleCurve;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();

        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Move upward
        rect.anchoredPosition += Vector2.up *
                                 moveSpeed *
                                 Time.deltaTime;

        // Fade out
        float alpha =
            Mathf.Lerp(1f, 0f, timer / lifetime);

        Color currentColor = text.color;

        text.color = new Color(
            currentColor.r,
            currentColor.g,
            currentColor.b,
            alpha
        );

        // Slight scale animation
        float scale = scaleCurve.Evaluate(timer / lifetime);

        transform.localScale = Vector3.one * scale;

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}