using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => AudioManager.Instance.PlayOverlap("Button"));
    }
}
