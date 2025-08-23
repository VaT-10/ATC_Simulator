
using DG.Tweening;
using System;
using UnityEngine;


namespace Managers
{
    /// <summary>
    /// менеджер дл€ удобного управлени€ выбором самолетов.
    /// </summary>
    public class SelectPlaneManager
    {
        private static readonly Lazy<SelectPlaneManager> _instance = new(() => new SelectPlaneManager());  // делаем менеджер синглтоном
        public static SelectPlaneManager Instance => _instance.Value;
        public static event Action<Plane> OnSelect;

        private readonly CanvasGroup _contentGroup;  // CanvasGroup дл€ управлени€ прозрачностью UI
        private readonly GameObject _infoPanel;

        private readonly GameObject _planeMirror;

        public Plane selectedPlane;
        private bool _isOnScreen = false;  // флаг, показывающий, отображаетс€ ли окно с информацией о самолете на экране

        private SelectPlaneManager()
        {
            _contentGroup = GameObject.FindWithTag("ContentGroup")?.GetComponent<CanvasGroup>() ??
                throw new MissingComponentException("The game object with tag 'ContentGroup' does not exist " +
                                                    "or does not have the 'CanvasGroup' component. " +
                                                    "Please check it and try again.");

            _infoPanel = GameObject.FindWithTag("InfoPanel") ?? 
                throw new MissingComponentException("The game object with tag 'InfoPanel' does not exist. Please check it and try again.");

            _planeMirror = GameObject.FindWithTag("PlaneMirror") ??
                throw new MissingComponentException("The game object with tag 'PlaneMirror' does not exist. Please check it and try again");

            _contentGroup.alpha = 0;
            UIAnimationManager.YSlideScreen(
                _infoPanel,
                UIAnimationManager.YSlides.SlideOut,
                0f
            );


            _planeMirror.SetActive(false);
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

            SpriteAnimationManager.DoCrossFade(
                plane.spriteRenderer,
                GetFirstChildSpriteRenderer(plane.gameObject),
                0.2f
            );

            var _flightInfoUIGroup = TMPFlightInfoUIGroup.Instance;

            _contentGroup.DOFade(0, 0.2f)
                .OnComplete(() =>
                {
                    _flightInfoUIGroup.flightNameText.text = plane.flightName;
                    _flightInfoUIGroup.PlaneModelText.text = plane.planeModel;
                    _flightInfoUIGroup.routeText.text = $"{plane.startingPlace} Ч\n{plane.destination}";
                    _flightInfoUIGroup.speedText.text = plane.speed.ToString();
                    _flightInfoUIGroup.altitudeText.text = plane.altitude.ToString() + 'K';

                    _contentGroup.DOFade(1, 0.2f);
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

        private static SpriteRenderer GetFirstChildSpriteRenderer(GameObject obj)
        {
            return obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
        }
    }
}