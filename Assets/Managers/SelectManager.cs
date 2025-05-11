
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace Managers
{
    /// <summary>
    /// �������� ��� �������� ���������� ������� ���������.
    /// </summary>
    public class SelectPlaneManager
    {
        private static readonly Lazy<SelectPlaneManager> _instance = new Lazy<SelectPlaneManager>(() => new SelectPlaneManager());  // ������ �������� ����������
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
        /// ������� ��� ������ ��������. ������ ���������� isSelected � �������� ������ �� selectedPlaneSprite.
        /// </summary>
        /// <param name="plane">������ Plane.cs, ����������� � ����������� ��������</param>
        public void selectObject(Plane plane)
        {
            onSelect?.Invoke(plane);

            plane.isSelected = true;

            _runner.StartCoroutine(_spriteAnimationManager.SpriteSmoothTransition(plane.GetComponent<SpriteRenderer>(), getFirstChildSpriteRenderer(plane.gameObject), 0.2f));

            var _flightInfoUIGroup = TMPFlightInfoUIGroup.Instance;
            var TMPtoValues = new Dictionary<TextMeshProUGUI, string>  // ������� ������� ������������ ����� UI � ������� � ��������
            {
                [_flightInfoUIGroup.flightNameText] = plane.flightName,
                [_flightInfoUIGroup.planeModelText] = plane.planeModel,
                [_flightInfoUIGroup.routeText] = $"{plane.startingPlace} - {plane.destination}",
                [_flightInfoUIGroup.speedText] = plane.speed.ToString(),
                [_flightInfoUIGroup.altitudeText] = plane.altitude.ToString()
            };

            // ����� ���������� � �������� � ��������������� UI -> TMP
            SetTextFromDict(TMPtoValues);
        }

        /// <summary>
        /// ������� ����� � ��������.
        /// </summary>
        /// <param name="plane">������ Plane.cs, ����������� � ��������, � �������� ����� ����� �����</param>
        public void deSelectObject(Plane plane)
        {
            plane.isSelected = false;

            _runner.StartCoroutine(_spriteAnimationManager.SpriteSmoothTransition(getFirstChildSpriteRenderer(plane.gameObject), plane.GetComponent<SpriteRenderer>(), 0.2f));

        }

        /// <summary>
        /// �������� ������� tmp -> string � ������ ����� ������� ����������� TMP �� ������ 
        /// ���������� �������� ��� ������ ������
        /// </summary>
        /// <param name="textMeshProDct">������� � ������ tmp -> string</param>
        private static void SetTextFromDict(Dictionary<TextMeshProUGUI, string> textMeshProDct)
        {
            foreach (KeyValuePair<TextMeshProUGUI, string> kvp in textMeshProDct)  // ������� ���� ��� ����-��������
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