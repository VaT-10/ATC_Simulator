
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace Managers
{
    /// <summary>
    /// менеджер для удобного управления выбором самолетов.
    /// </summary>
    public class SelectPlaneManager
    {
        private static readonly Lazy<SelectPlaneManager> _instance = new Lazy<SelectPlaneManager>(() => new SelectPlaneManager());  // делаем менеджер синглтоном
        public static SelectPlaneManager Instance => _instance.Value;

        private SpriteAnimationManager _spriteAnimationManager;

        public static event Action<Plane> onSelect;

        private class CoroutineRunner : MonoBehaviour { };
        private static CoroutineRunner _runner;


        private SelectPlaneManager() 
        {
            _spriteAnimationManager = new SpriteAnimationManager();

            var runnerObj = new GameObject("CoroutineRunner");
            _runner = runnerObj.AddComponent<CoroutineRunner>();
        }

        /// <summary>
        /// функция для выбора самолета. меняет переменную isSelected и заменяет спрайт на selectedPlaneSprite.
        /// </summary>
        /// <param name="plane">скрипт Plane.cs, привзяанный к выбираемому самолету</param>
        public void selectObject(Plane plane)
        {
            onSelect?.Invoke(plane);

            plane.isSelected = true;

            _runner.StartCoroutine(_spriteAnimationManager.SpriteSmoothTransition(plane.GetComponent<SpriteRenderer>(), getFirstChildSpriteRenderer(plane.gameObject), 0.2f));

            var _flightInfoUIGroup = TMPFlightInfoUIGroup.Instance;
            var TMPtoValues = new Dictionary<TextMeshProUGUI, string>  // создаем словарь соответствий между UI и данными о самолете
            {
                [_flightInfoUIGroup.flightNameText] = plane.flightName,
                [_flightInfoUIGroup.planeModelText] = plane.planeModel,
                [_flightInfoUIGroup.routeText] = $"{plane.startingPlace} - {plane.destination}",
                [_flightInfoUIGroup.speedText] = plane.speed.ToString(),
                [_flightInfoUIGroup.altitudeText] = plane.altitude.ToString()
            };

            // вывод информации о самолете в соответствующие UI -> TMP
            SetTextFromDict(TMPtoValues);
        }

        /// <summary>
        /// снимает выбор с самолета.
        /// </summary>
        /// <param name="plane">скрипт Plane.cs, привзяанный к самолету, с которого нужно снять выбор</param>
        public void deSelectObject(Plane plane)
        {
            plane.isSelected = false;

            _runner.StartCoroutine(_spriteAnimationManager.SpriteSmoothTransition(getFirstChildSpriteRenderer(plane.gameObject), plane.GetComponent<SpriteRenderer>(), 0.2f));

        }

        /// <summary>
        /// получает словарь tmp -> string и меняет текст каждого полученного TMP на каждое 
        /// полученное значение или пустую строку
        /// </summary>
        /// <param name="textMeshProDct">словарь с парами tmp -> string</param>
        private static void SetTextFromDict(Dictionary<TextMeshProUGUI, string> textMeshProDct)
        {
            foreach (KeyValuePair<TextMeshProUGUI, string> kvp in textMeshProDct)  // перебор всех пар ключ-значение
            {
                kvp.Key.text = kvp.Value;
            }
        }

        private static SpriteRenderer getFirstChildSpriteRenderer(GameObject obj)
        {
            return obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
        }
    }
}