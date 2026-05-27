using UnityEngine;
using System.Collections;

public class ObjectShake : MonoBehaviour
{
    private Vector3 originalPos;

    private Coroutine shakeRoutine;

    public void Shake(float duration, float magnitude)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);

            transform.localPosition = originalPos;
        }

        shakeRoutine =
            StartCoroutine(
                ShakeRoutine(duration, magnitude)
            );
    }

    IEnumerator ShakeRoutine(
        float duration,
        float magnitude)
    {
        originalPos = transform.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x =
                Random.Range(-1f, 1f)
                * magnitude;

            float y =
                Random.Range(-1f, 1f)
                * magnitude;

            transform.localPosition =
                originalPos +
                new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition =
            originalPos;

        shakeRoutine = null;
    }
}