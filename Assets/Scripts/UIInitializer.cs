using UnityEngine;
using UnityEngine.Animations;
using Managers;
using System.Diagnostics.CodeAnalysis;

public class UIInitializer : MonoBehaviour
{
    private static readonly Vector2 INFO_PANEL_PIVOT = new(0.5f, 0f);

    [SerializeField, NotNull] private Canvas canvas;
    [SerializeField, NotNull] private GameObject infoPanel;
    [SerializeField, NotNull] private GameObject mapBackground;

    void Start()
    {
        UISizeManager.SetDownCenterAnchors(infoPanel);
        UISizeManager.SetPivot(INFO_PANEL_PIVOT, infoPanel);
        UISizeManager.SetHeightByCanvasPercent(45f, canvas, infoPanel);

        GOSizeManager.SetGOSizePercent(mapBackground, Axis.X, 100f);
    }

}
