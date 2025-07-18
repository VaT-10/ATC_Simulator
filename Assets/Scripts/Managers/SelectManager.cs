
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace Managers
{
    /// <summary>
    /// менеджер дл€ удобного управлени€ выбором самолетов.
    /// </summary>
    public class SelectPlaneManager
    {
        private static readonly Lazy<SelectPlaneManager> _instance = new Lazy<SelectPlaneManager>(() => new SelectPlaneManager());  // делаем менеджер синглтоном
        public static SelectPlaneManager Instance => _instance.Value;

        public static event Action<Plane> OnSelect;


        /// <summary>
        /// функци€ дл€ выбора самолета. мен€ет переменную isSelected и замен€ет спрайт на selectedPlaneSprite.
        /// </summary>
        /// <param name="plane">скрипт Plane.cs, привз€анный к выбираемому самолету</param>
        public void SelectObject(Plane plane)
        {
            OnSelect?.Invoke(plane);

            plane.isSelected = true;

            SpriteAnimationManager.DoCrossfade(
                plane.spriteRenderer,
                GetFirstChildSpriteRenderer(plane.gameObject),
                0.2f
            );

            var _flightInfoUIGroup = TMPFlightInfoUIGroup.Instance;

            _flightInfoUIGroup.flightNameText.text = plane.flightName;
            _flightInfoUIGroup.planeModelText.text = plane.planeModel;
            _flightInfoUIGroup.routeText.text = $"{plane.startingPlace} -\n{plane.destination}";
            _flightInfoUIGroup.speedText.text = plane.speed.ToString();
            _flightInfoUIGroup.altitudeText.text = plane.altitude.ToString();
        }

        /// <summary>
        /// снимает выбор с самолета.
        /// </summary>
        /// <param name="plane">скрипт Plane.cs, привз€анный к самолету, с которого нужно сн€ть выбор</param>
        public void DeSelectObject(Plane plane)
        {
            plane.isSelected = false;

            _runner.StartCoroutine(_spriteAnimationManager.SpriteSmoothTransition(GetFirstChildSpriteRenderer(plane.gameObject), plane.GetComponent<SpriteRenderer>(), 0.2f));

        }

        private static SpriteRenderer GetFirstChildSpriteRenderer(GameObject obj)
        {
            return obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
        }
    }
}