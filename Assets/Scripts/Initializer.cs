using UnityEngine;
using UnityEngine.Animations;
using Managers;
using System.Diagnostics.CodeAnalysis;

public class Initializer : MonoBehaviour
{
    private static readonly Vector2 INFO_PANEL_PIVOT = new(0.5f, 0f);

    [SerializeField, NotNull] private Canvas canvas;
    [SerializeField, NotNull] private GameObject infoPanel;
    [SerializeField, NotNull] private GameObject mapBackground;

    void Awake()
    {
        TMPFlightInfoUIGroup.Initialize();
        UISizeManager.SetDownCenterAnchors(infoPanel);
        UISizeManager.SetPivot(INFO_PANEL_PIVOT, infoPanel);
        GOSizeManager.SetGOSizePercent(infoPanel, Axis.Y, 45f);

        GOSizeManager.SetGOSizePercent(mapBackground, Axis.Y, 45f);
    }

}
