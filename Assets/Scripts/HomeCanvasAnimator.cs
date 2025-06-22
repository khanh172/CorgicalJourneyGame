using UnityEngine;
using DG.Tweening;

public class HomeCanvasAnimator : MonoBehaviour
{
    [Header("Logo Intro & Idle")]
    public RectTransform logo;
    public float logoIntroScaleDuration = 0.6f;   
    public float logoIdleMoveAmount = 10f;        
    public float logoIdleDuration = 2f;           

    [Header("Buttons Intro")]
    public CanvasGroup[] buttons;                 
    public float buttonIntroDelay = 0.1f;         
    public float buttonIntroDuration = 0.5f;      
    public float buttonSlideOffset = 200f;        


    private Vector2 logoOriginalPos;
    private Vector3 logoOriginalScale;
    private Vector2[] buttonOriginalPos;
    private Vector3[] buttonOriginalScale;

    private Tween logoIdleTween;

    void Awake()
    {
    }

    void OnEnable()
    {
        AnimateIntro();
    }

    void OnDisable()
    {
        CleanUpTweens();
    }

    void OnDestroy()
    {
        CleanUpTweens();
    }

    private void CleanUpTweens()
    {
        
        if (logoIdleTween != null && logoIdleTween.IsActive())
            logoIdleTween.Kill();
        if (logo != null)
            DOTween.Kill(logo);

        
        if (buttons != null)
        {
            foreach (var cg in buttons)
            {
                if (cg != null)
                {
                    DOTween.Kill(cg);
                    RectTransform rt = cg.GetComponent<RectTransform>();
                    if (rt != null)
                        DOTween.Kill(rt);
                }
            }
        }
        
        CancelInvoke();
    }

    public void AnimateIntro()
    {
        
        if (logo != null)
        {
            
            DOTween.Kill(logo);

            
            logoOriginalPos = logo.anchoredPosition;
            logoOriginalScale = logo.localScale;

            
            logo.localScale = Vector3.zero;

            
            logo.DOScale(logoOriginalScale, logoIntroScaleDuration)
                .SetEase(Ease.OutBack);

            
            logoIdleTween = logo
                .DOAnchorPosY(logoOriginalPos.y + logoIdleMoveAmount, logoIdleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(logoIntroScaleDuration);

            
            logo.anchoredPosition = new Vector2(logoOriginalPos.x, logoOriginalPos.y);
        }

        
        if (buttons != null && buttons.Length > 0)
        {
            int len = buttons.Length;
            
            buttonOriginalPos = new Vector2[len];
            buttonOriginalScale = new Vector3[len];

            for (int i = 0; i < len; i++)
            {
                CanvasGroup cg = buttons[i];
                if (cg == null) continue;

                RectTransform rt = cg.GetComponent<RectTransform>();
                if (rt == null) continue;

                
                DOTween.Kill(cg);
                DOTween.Kill(rt);

               
                buttonOriginalPos[i] = rt.anchoredPosition;
                buttonOriginalScale[i] = rt.localScale;

                rt.anchoredPosition = new Vector2(buttonOriginalPos[i].x, buttonOriginalPos[i].y - buttonSlideOffset);

                cg.alpha = 0f;
                rt.localScale = buttonOriginalScale[i] * 0.8f; 

                
                float delay = logoIntroScaleDuration + i * buttonIntroDelay;

                
                rt.DOAnchorPos(buttonOriginalPos[i], buttonIntroDuration)
                    .SetEase(Ease.OutBack)
                    .SetDelay(delay);

                
                cg.DOFade(1f, buttonIntroDuration)
                  .SetEase(Ease.Linear)
                  .SetDelay(delay);

               
                rt.DOScale(buttonOriginalScale[i], buttonIntroDuration)
                  .SetEase(Ease.OutBack)
                  .SetDelay(delay);
            }
        }
    }
}
