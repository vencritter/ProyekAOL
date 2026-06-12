using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public Image image;
    public float fadeDuration = 1f;

    void Start()
    {
        // Fade in when the scene loads
        StartCoroutine(FadeIn(fadeDuration));
    }

    public void FadeAndLoad(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName, fadeDuration));
    }

    public void FadeAndLoad(string sceneName, float duration)
    {
        StartCoroutine(FadeOut(sceneName, duration));
    }

    IEnumerator FadeIn(float duration)
    {
        float elapsedTime = 0f;
        Color color = image.color;
        color.a = 1f;
        image.color = color;

        while (elapsedTime < duration)
        {
            color.a = 1f - Mathf.Clamp01(elapsedTime / duration);
            image.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 0f;
        image.color = color;
        image.enabled = false;
    }

    IEnumerator FadeOut(string sceneName, float duration)
    {
        float elapsedTime = 0f;
        Color color = image.color;
        color.a = 0f;
        image.color = color;

        while (elapsedTime < duration)
        {
            color.a = Mathf.Clamp01(elapsedTime / duration);
            image.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 1f;
        image.color = color;

        SceneManager.LoadScene(sceneName);
    }
}