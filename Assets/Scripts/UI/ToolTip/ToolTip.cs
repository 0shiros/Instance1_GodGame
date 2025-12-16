using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform rectTransform, parentTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector2 cameraOffset;

    public static ToolTip Instance;
    
    private Image backgroundImage;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        backgroundImage = GetComponent<Image>();
        backgroundImage.enabled = false;
        tooltipText.enabled = false;
    }

    private void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentTransform,
            new Vector2(Input.mousePosition.x + cameraOffset.x, Input.mousePosition.y + cameraOffset.y), mainCamera,
            out Vector2 localPoint);
        transform.localPosition = localPoint;
    }

    public void ShowTooltip(string pTooltip)
    {
        backgroundImage.enabled = true;
        tooltipText.enabled = true;
        tooltipText.text = pTooltip;
    }

    public void HideTooltip()
    {
        backgroundImage.enabled = false;
        tooltipText.enabled = false;
    }
}