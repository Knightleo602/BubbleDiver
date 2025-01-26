using System.Collections;
using UnityEngine;

public class MenuFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeSpeed = 1f;

    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
