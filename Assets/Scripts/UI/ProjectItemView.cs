using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image icon;
    [SerializeField] private Button buildButton;
    [SerializeField] private TMP_Text buildButtonText;

    private System.Action _onBuildClicked;

    public void Setup(ProjectData data, bool canStart, System.Action onBuild, string actionLabel = null)
    {
        if (buildButton != null)
        {
            buildButton.onClick.RemoveListener(HandleBuildClicked);
        }

        if (nameText != null) nameText.text = data.ProjectName;
        if (costText != null) costText.text = BudgetSystem.FormatBudget(data.Cost);
        if (durationText != null) durationText.text = $"{data.DurationMonths} мес.";
        if (descriptionText != null) descriptionText.text = data.Description;

        if (icon != null)
        {
            bool hasIcon = data.Icon != null;
            icon.gameObject.SetActive(hasIcon);
            if (hasIcon) icon.sprite = data.Icon;
        }

        if (buildButton != null)
        {
            buildButton.interactable = canStart;
            _onBuildClicked = onBuild;
            buildButton.onClick.AddListener(HandleBuildClicked);

            TMP_Text label = buildButtonText != null ? buildButtonText : buildButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = string.IsNullOrWhiteSpace(actionLabel) ? "Построить" : actionLabel;
            }
        }
    }

    private void HandleBuildClicked()
    {
        _onBuildClicked?.Invoke();
    }

    private void OnDestroy()
    {
        if (buildButton != null) buildButton.onClick.RemoveListener(HandleBuildClicked);
    }
}
