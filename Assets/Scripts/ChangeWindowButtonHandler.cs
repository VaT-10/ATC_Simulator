using UnityEngine;

public class ChangeWindowButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject _basicContent, _pitchContent;
    private enum WindowType
    {
        Basic,
        Pitch
    }

    private WindowType _curType = WindowType.Basic;

    public void ChangeWindow()
    {
        _curType = _curType == WindowType.Basic ? WindowType.Pitch : WindowType.Basic;

        _basicContent.SetActive(_curType == WindowType.Basic);
        _pitchContent.SetActive(_curType == WindowType.Pitch);
    }
}
