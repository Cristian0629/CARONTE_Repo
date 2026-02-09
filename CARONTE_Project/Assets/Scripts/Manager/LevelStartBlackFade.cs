using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelStartBlackFade : MonoBehaviour
{
    [SerializeField] private float fadeInDuration = 0.25f; // negro -> transparente

    private CanvasGroup cg;

    private void Awake()
    {
        // Crear Canvas encima de TODO
        GameObject canvasGO = new GameObject("___LevelStartFadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // MUY por encima

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Crear panel negro a pantalla completa
        GameObject panelGO = new GameObject("FadePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        Image img = panelGO.AddComponent<Image>();
        img.color = Color.black;

        RectTransform rt = panelGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        cg = panelGO.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;

        // Opcional: que no se destruya si cambias rápido de escena
        // DontDestroyOnLoad(canvasGO);

        StartCoroutine(FadeInAndDestroy(canvasGO));
    }

    private IEnumerator FadeInAndDestroy(GameObject root)
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeInDuration);
            yield return null;
        }

        cg.alpha = 0f;
        Destroy(root);
        Destroy(this);
    }
}
