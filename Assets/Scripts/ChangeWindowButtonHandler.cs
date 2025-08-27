using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeWindowButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject _basicContent, _pitchContent;
    [SerializeField] private Image _infoPanelImg;
    [SerializeField] private Sprite _basicBg, _pitchBg;

    [SerializeField] private string _basicText, _pitchText;
    [SerializeField] private TextMeshProUGUI _btnText;
    public enum WindowType
    {
        Basic,
        Pitch
    }
    private static WindowType _curType = WindowType.Basic;


    /// <summary>
    /// изменяет окно на указанное или следующее
    /// </summary>
    /// <param name="type">по умолчанию null - новый тип определяется автоматически. иначе изменяется на указанный</param>
    public void ChangeWindow(WindowType? type = null)
    {
        _curType = type ?? (_curType == WindowType.Basic ? WindowType.Pitch : WindowType.Basic);

        _basicContent.SetActive(_curType == WindowType.Basic);
        _pitchContent.SetActive(_curType == WindowType.Pitch);

        _btnText.text = _curType == WindowType.Basic ? _basicText : _pitchText;

        ChangeBackground(_curType);
    }

    public void AutoChangeWindow() => ChangeWindow();
    private void ChangeBackground(WindowType wt) => _infoPanelImg.sprite = wt == WindowType.Basic ? _basicBg : _pitchBg;
}
