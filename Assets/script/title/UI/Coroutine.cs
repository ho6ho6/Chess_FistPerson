using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Coroutine : MonoBehaviour
{
    [SerializeField] Image logoImg;
    [SerializeField] float startScale = 10.0f;  // 仮に
    [SerializeField] float endScale = 1.0f;
    [SerializeField] float duration = 1.5f;

    void Awake()
    {
        logoImg.gameObject.SetActive(false);
    }

    void Start()
    {
        logoImg.gameObject.SetActive(true);
        StartCoroutine(Play());        
    }


    // 画像をフェードインさせ、拡大スケールから指定のスケールに
    IEnumerator Play()      // IEnumerator = C#でコレクションの要素を順番に取り出すためのインターフェース
    {
        RectTransform rt = (RectTransform)logoImg.transform;

        // 初期状態　かなり大きく
        rt.localScale = Vector3.one * startScale;

        Color c = logoImg.color;
        c.a = 0f;
        logoImg.color = c;

        float t = 0f;


        while (t < duration)
        {
            t += Time.deltaTime;
            float rate = Mathf.Clamp01(t / duration);

            // Ease
            float eased = Mathf.SmoothStep(0f, 1f, rate);

            // Scale
            float scale = Mathf.Lerp(startScale, endScale, eased);
            rt.localScale = Vector3.one * scale;

            // Alpha
            c.a = eased / 1.5f;
            logoImg.color = c;

            yield return null;
        }

        // 最終地は固定
        rt.localScale = Vector3.one * endScale;
        c.a = 1.0f;
    }

}
