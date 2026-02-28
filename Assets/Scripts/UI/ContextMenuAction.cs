using System;

public readonly struct ContextMenuAction
{
    public string Label { get; }
    public bool HideMenuAfterInvoke { get; }

    private readonly Action _onSelected;

    public ContextMenuAction(string label, Action onSelected, bool hideMenuAfterInvoke = true)
    {
        Label = string.IsNullOrWhiteSpace(label) ? "Action" : label;
        HideMenuAfterInvoke = hideMenuAfterInvoke;
        _onSelected = onSelected;
    }

    public void Invoke()
    {
        _onSelected?.Invoke();
    }
}
