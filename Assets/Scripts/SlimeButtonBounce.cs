using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SlimeButtonBounce : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float bounceScale = 1.1f;       // Scale khi nảy nhẹ
    public float hoverScale = 1.3f;        // Scale khi hover
    public float bounceDuration = 0.4f;    // Tốc độ nảy

    private Vector3 originalScale;
    private Tween bounceTween;

    void Start()
    {
        originalScale = transform.localScale;
        StartBounce(); // bắt đầu nảy khi khởi động
    }

    void StartBounce()
    {
        if (bounceTween != null && bounceTween.IsActive()) bounceTween.Kill();

        bounceTween = transform
            .DOScale(originalScale * bounceScale, bounceDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (bounceTween != null && bounceTween.IsActive()) bounceTween.Kill(); // Ngưng nảy

        // Scale to hover, không lặp lại
        transform.DOScale(originalScale * hoverScale, 0.15f)
            .SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Thu về scale gốc rồi bắt đầu nảy lại
        transform.DOScale(originalScale, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                StartBounce();
            });
    }
}
