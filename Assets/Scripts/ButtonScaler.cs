using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Hover Scale")]
    public float scaleUp = 1.2f;
    public float hoverDuration = 0.2f;
    [Header("Pulse Effect")]
    public float pulseInterval = 2f;         // khoảng thời gian giữa các pulse
    public float pulseScale = 1.1f;          // tỷ lệ scale đỉnh pulse so với original
    public float pulseDuration = 0.3f;       // thời gian scale lên và xuống cho pulse
    [Header("Punch Effect")]
    public float punchStrength = 0.1f;       // độ lớn punch khi click
    public float punchDuration = 0.2f;       // thời gian punch tween

    private Vector3 originalScale;
    private Tween hoverTween;
    private Tween pulseTween;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;
        
        InvokeRepeating(nameof(DoPulse), pulseInterval, pulseInterval);
    }

   
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        KillTweens();
        
        hoverTween = transform
            .DOScale(originalScale * scaleUp, hoverDuration)
            .SetEase(Ease.OutBack);
    }

    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        KillTweens();
       
        hoverTween = transform
            .DOScale(originalScale, hoverDuration)
            .SetEase(Ease.OutBack);
    }

   
    public void OnPointerDown(PointerEventData eventData)
    {
        
        transform.DOPunchScale(Vector3.one * punchStrength, punchDuration).SetEase(Ease.OutQuad);
    }

    void DoPulse()
    {
        
        if (isHovered) return;
       
        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Kill();
        }
        
        pulseTween = transform
            .DOScale(originalScale * pulseScale, pulseDuration)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                
                pulseTween = transform
                    .DOScale(originalScale, pulseDuration)
                    .SetEase(Ease.InSine);
            });
    }

    void KillTweens()
    {
        if (hoverTween != null && hoverTween.IsActive())
        {
            hoverTween.Kill();
        }
        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Kill();
        }
    }

    void OnDisable()
    {
        
        CancelInvoke(nameof(DoPulse));
        KillTweens();
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(DoPulse));
        KillTweens();
    }
}
