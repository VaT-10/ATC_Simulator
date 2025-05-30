using UnityEngine;
using Managers;

public class UIInitializer : MonoBehaviour
{
    
    public Canvas canvas;
    public GameObject infoPanel;

    void Start()
    {
        UISizeManager.SetHeightByCanvasPercent(45f, canvas, infoPanel);
    }
}
