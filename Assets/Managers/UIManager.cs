using System;
using System.Data;
using System.Diagnostics.Tracing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// ����� ��� �������� TMP ��� ����������� ���������� � ��������
    /// </summary>
    [Serializable]
    public class TMPFlightInfoUIGroup
    {
        private static readonly Lazy<TMPFlightInfoUIGroup> _instance = new Lazy<TMPFlightInfoUIGroup>(() => new TMPFlightInfoUIGroup());
        public static TMPFlightInfoUIGroup Instance => _instance.Value;

        public TextMeshProUGUI flightNameText;
        public TextMeshProUGUI planeModelText;
        public TextMeshProUGUI routeText;
        public TextMeshProUGUI speedText;
        public TextMeshProUGUI altitudeText;

        private TMPFlightInfoUIGroup() 
        {
            flightNameText = GameObject.Find("FlightName").GetComponent<TextMeshProUGUI>();
            planeModelText = GameObject.Find("PlaneModel").GetComponent<TextMeshProUGUI>();
            routeText = GameObject.Find("Route").GetComponent<TextMeshProUGUI>();
            speedText = GameObject.Find("Speed").GetComponent<TextMeshProUGUI>();
            altitudeText = GameObject.Find("Altitude").GetComponent<TextMeshProUGUI>();
        }

        /// <returns>��� ������� TMP, ���������� � ������</returns>
        public TextMeshProUGUI[] GetAllTMPs()
        {
            return new TextMeshProUGUI[] { flightNameText, planeModelText, routeText, speedText, altitudeText };
        }

        /// <summary>
        /// ������� ��� TMP �� ����������� �������, ������� �� ����� �� ������ ������.
        /// </summary>
        public static void ClearAllText()
        {
            foreach (TextMeshProUGUI TMPToClear in Instance.GetAllTMPs())
            {
                TMPToClear.text = "";
            }
        }
    }

    /// <summary>
    /// ����� ��� ���������� �������� ��������� ����������
    /// </summary>
    public static class UISizeManager
    {
        /// <summary>
        /// ������� ��� ��������� ������ �������� � ��������� �� �������
        /// </summary>
        /// <param name="percentage">��������</param>
        /// <param name="canvas">������, ������������ �������� �������� ������ ��������</param>
        /// <param name="element">��� ������� ��� ��������� �������</param>
        /// <param name="preserveAspect">������ ��������, ������������, ���������� �� �������� ���������</param>
        public static void SetHeightPercentage(float percentage, Canvas canvas, GameObject element, bool preserveAspect = true)
        {
            CheckArgs(percentage, canvas, element);

            var canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
            var rectTransform = element.GetComponent<RectTransform>();

            var newHeight = canvasHeight / 100f * percentage;
            float newWidth = preserveAspect ? GetRectTransformAspect(rectTransform) * newHeight : rectTransform.sizeDelta.x;

            rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
        }

        /// <summary>
        /// ������� ��� ��������� ������ �������� � ��������� �� �������
        /// </summary>
        /// <param name="percentage">��������</param>
        /// <param name="canvas">������, ������������ �������� �������� ������ ��������</param>
        /// <param name="element">��� ������� ��� ��������� �������</param>
        /// <param name="preserveAspect">������ ��������, ������������, ���������� �� �������� ���������</param>
        public static void SetWidthPercentage(float percentage, Canvas canvas, GameObject element, bool preserveAspect = true)
        {
            CheckArgs(percentage, canvas, element);

            var canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
            var rectTransform = element.GetComponent<RectTransform>();

            var newWidth = canvasWidth / 100f * percentage;

            float newHeight = preserveAspect ? newWidth / GetRectTransformAspect(rectTransform) : rectTransform.sizeDelta.y;

            rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
        }

        /// <summary>
        /// ������� ��� �������� ���������� ��� ��������� ������� � ��������� �� ����������
        /// </summary>
        /// <param name="percentage">��������</param>
        /// <param name="canvas">������, ������������ �������� �������� ������ ��������</param>
        /// <param name="element">��� ������� ��� ��������� �������</param>
        private static void CheckArgs(float percentage, Canvas canvas, GameObject element)
        {
            if (percentage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(percentage), $"Percentage must be more than 0, got {percentage}");
            }
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas), $"Got null instead of canvas");
            }
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element), $"Got null instead of element");
            }

            var rt = element.GetComponent<RectTransform>();
            if (rt == null)
            {
                throw new MissingComponentException("RectTransform component required!");
            }
            if (rt.anchorMax != rt.anchorMin)
            {
                throw new Exception($"The anchors of element \"{element.name}\" are not the same. The result may differ from the expected one.");
            }
        }

        /// <summary>
        /// ������� ��� ������� ��������� rectTransform
        /// </summary>
        /// <param name="rectTransform">���������, ��������� �������� ����� �����</param>
        /// <returns>��������� rectTransform</returns>
        private static float GetRectTransformAspect(RectTransform rectTransform)
        {
            var width = rectTransform.sizeDelta.x;
            var height = rectTransform.sizeDelta.y;

            return (height != 0)? width / height : width;
        }
    }
}