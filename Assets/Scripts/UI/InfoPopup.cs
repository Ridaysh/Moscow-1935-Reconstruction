using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class InfoPopup : PopupWindow
{
    private static InfoPopup _instance;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image image;
    [SerializeField] private Button closeButton;
    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }

    private void OnEnable()
    {
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    private void OnDisable()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public static void Show(ProjectData data)
    {
        if (_instance == null || data == null)
        {
            return;
        }

        if (_instance.titleText != null)
        {
            _instance.titleText.text = data.ProjectName;
        }

        if (_instance.descriptionText != null)
        {
            _instance.descriptionText.text = data.HistoricalInfo;
        }

        if (_instance.image != null)
        {
            bool hasImage = data.HistoricalImage != null;
            _instance.image.gameObject.SetActive(hasImage);
            if (hasImage)
            {
                _instance.image.sprite = data.HistoricalImage;
            }
        }

        _instance.Show();
    }
}
