using UnityEngine;
using DG.Tweening;

public class LevelSelectAnimator : MonoBehaviour
{
    public CanvasGroup logoLevel; 
    public CanvasGroup[] levelButtons; 

    public float logoFadeDuration = 0.5f;
    public float buttonFadeDelay = 0.1f;
    public float buttonFadeDuration = 0.4f;

    void OnEnable()
    {
        AnimateLevelSelect();
    }

    public void AnimateLevelSelect()
    {
        if (logoLevel != null)
        {
            logoLevel.alpha = 0;
            logoLevel.gameObject.SetActive(true);

            logoLevel.DOFade(1, logoFadeDuration)
                     .SetEase(Ease.OutQuad);
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            CanvasGroup btn = levelButtons[i];
            btn.alpha = 0;
            btn.gameObject.SetActive(true);

            btn.DOFade(1, buttonFadeDuration)
                .SetEase(Ease.OutQuad)
                .SetDelay(logoFadeDuration + i * buttonFadeDelay);
        }
    }
}
