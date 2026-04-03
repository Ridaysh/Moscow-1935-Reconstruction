public static class PopupManager
{
    private static PopupWindow _currentPopup;

    public static void Show(PopupWindow popup)
    {
        if (popup == null)
        {
            return;
        }

        if (_currentPopup != null && _currentPopup != popup)
        {
            _currentPopup.SetOpenFromManager(false);
        }

        _currentPopup = popup;

        if (ContextMenuController.TryGetGlobal(out ContextMenuController contextMenu))
        {
            contextMenu.Hide();
        }

        popup.SetOpenFromManager(true);
    }

    public static void Hide(PopupWindow popup)
    {
        if (popup == null)
        {
            return;
        }

        popup.SetOpenFromManager(false);
        if (_currentPopup == popup)
        {
            _currentPopup = null;
        }
    }
}
