using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    public AudioClip clickSound;

    void Start()
    {
        Button btn = GetComponent<Button>();

        if (clickSound == null && GameManager.Instance != null)
            clickSound = GameManager.Instance.soundButtonClick;

        btn.onClick.AddListener(() =>
        {
            if (GameManager.Instance != null && clickSound != null)
                GameManager.Instance.PlaySound(clickSound);
        });
    }
}
