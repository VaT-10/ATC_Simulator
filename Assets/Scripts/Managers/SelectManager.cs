
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;


namespace Managers
{
    /// <summary>
    /// менеджер дл€ удобного управлени€ выбором самолетов.
    /// </summary>
    public class SelectPlaneManager : MonoBehaviour
    {
        public static event Action<Plane> OnSelect;

        [Header("UI References")]
        [SerializeField] private CanvasGroup _infoPanelCanvasGroup;
        [SerializeField] private GameObject _infoPanel;
        [SerializeField] private GameObject _planeMirror;
        [SerializeField] private GameObject _pitchContent;
        [SerializeField] private TextMeshProUGUI _pitchContentInfoText;

        [HideInInspector] public Plane selectedPlane;
        private bool _isOnScreen = false;

        private void Awake()
        {
            // sanity checks
            if (!_infoPanelCanvasGroup) Debug.LogError("ContentGroup not assigned in inspector!");
            if (!_infoPanel) Debug.LogError("InfoPanel not assigned in inspector!");
            if (!_planeMirror) Debug.LogError("PlaneMirror not assigned in inspector!");
            if (!_pitchContent) Debug.LogError("PitchContent not assigned in inspector!");

            // initial UI state
            _infoPanelCanvasGroup.alpha = 0;
            UIAnimationManager.YSlideScreen(_infoPanel, UIAnimationManager.YSlides.SlideOut, 0f);

            _planeMirror.SetActive(false);
            _pitchContent.SetActive(false);
            selectedPlane = null;
        }

        /// <summary>
        /// функци€ дл€ выбора самолета. мен€ет переменную isSelected и замен€ет спрайт на selectedPlaneSprite.
        /// </summary>
        /// <param name="plane">скрипт Plane.cs, привз€анный к выбираемому самолету</param>
        public void SelectObject(Plane plane)
        {
            OnSelect?.Invoke(plane);
            if (!_isOnScreen)
            {
                UIAnimationManager.YSlideScreen(
                    _infoPanel,
                    UIAnimationManager.YSlides.SlideIn,
                    0.4f
                );

                _isOnScreen = true;
            }

            plane.isSelected = true;
            selectedPlane = plane;
            _planeMirror.SetActive(true);

            _pitchContentInfoText.text = $"{plane.planeModel} Х {plane.flightName}";

            SpriteAnimationManager.DoCrossFade(
                plane.spriteRenderer,
                GetFirstChildSpriteRenderer(plane.gameObject),
                0.2f
            );

            var _flightInfoUIGroup = DataLinks.Instance;

            _infoPanelCanvasGroup.DOFade(0, 0.2f)
                .OnComplete(() =>
                {
                    _flightInfoUIGroup.FlightNameText.text = plane.flightName;
                    _flightInfoUIGroup.PlaneModelText.text = plane.planeModel;
                    _flightInfoUIGroup.RouteText.text = $"{plane.startingPlace} Ч\n{plane.destination}";
                    _flightInfoUIGroup.SpeedText.text = plane.speed.ToString();
                    _flightInfoUIGroup.AltitudeText.text = plane.altitude.ToString() + 'K';

                    _infoPanelCanvasGroup.DOFade(1, 0.2f);
                });

        }

        /// <summary>
        /// снимает выбор с самолета.
        /// </summary>
        /// <param name="plane">скрипт Plane.cs, привз€анный к самолету, с которого нужно сн€ть выбор</param>
        public void DeSelectObject(Plane plane, bool self = false)
        {
            plane.isSelected = false;

            if (self)
            {
                UIAnimationManager.YSlideScreen(
                    _infoPanel, 
                    UIAnimationManager.YSlides.SlideOut, 
                    0.4f
                );

                _planeMirror.SetActive(false);
                _isOnScreen = false;
                selectedPlane = null;
            }

            SpriteAnimationManager.DoCrossFade(
                GetFirstChildSpriteRenderer(plane.gameObject),
                plane.spriteRenderer,
                0.2f
            );

        }

        private static SpriteRenderer GetFirstChildSpriteRenderer(GameObject obj) => obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }
}