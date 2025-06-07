using UnityEngine;
using Managers;
using System.Diagnostics.CodeAnalysis;

public class UIInitializer : MonoBehaviour
{
    
    [SerializeField, NotNull] private Canvas canvas;
    [SerializeField, NotNull] private GameObject infoPanel;
    [SerializeField, NotNull] private GameObject mapBackground;

    void Start()
    {
        UISizeManager.SetDownCenterAnchors(infoPanel);
        UISizeManager.SetHeightByCanvasPercent(45f, canvas, infoPanel);


    }

}
