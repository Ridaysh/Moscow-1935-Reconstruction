using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button clickButton;

    private System.Action _onClick;

    private void Awake()
    {
        if (clickButton == null)
        {
            clickButton = GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        if (clickButton != null)
        {
            clickButton.onClick.AddListener(HandleClick);
        }
    }

    private void OnDisable()
    {
        if (clickButton != null)
        {
            clickButton.onClick.RemoveListener(HandleClick);
        }
    }

    public void Setup(ObjectiveProgress objective, System.Action onClick)
    {
        _onClick = onClick;

        if (objective == null || objective.Data == null)
        {
            if (clickButton != null)
            {
                clickButton.interactable = false;
            }

            return;
        }

        if (titleText != null)
        {
            titleText.text = objective.Data.Title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = objective.Data.Description;
        }

        if (progressText != null)
        {
            progressText.text = objective.GetProgressText();
        }

        if (statusText != null)
        {
            statusText.text = objective.GetStatusText();
        }

        if (clickButton != null)
        {
            clickButton.interactable = _onClick != null;
        }
    }

    private void HandleClick()
    {
        _onClick?.Invoke();
    }
}
