using System;
using System.ComponentModel;
using TMPro;
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
        public static void SetHeightByCanvasPercent(float percentage, Canvas canvas, GameObject element, bool preserveAspect = true)
        {
            CheckArgs(percentage, canvas, element);

            var canvasRect = canvas.GetComponent<RectTransform>().rect;

            var rectTransform = element.GetComponent<RectTransform>();

            var newHeight = canvasRect.height / 100f * percentage;
            var aspect = GetRectTransformAspect(rectTransform);

            float newWidth = preserveAspect ? aspect * newHeight : rectTransform.sizeDelta.y;

            if (newWidth > canvasRect.width)
            {
                newWidth = canvasRect.width;
                newHeight = newWidth / aspect;
            }
            rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
        }

        /// <summary>
        /// ������� ��� ��������� ������ �������� � ��������� �� �������
        /// </summary>
        /// <param name="percentage">��������</param>
        /// <param name="canvas">������, ������������ �������� �������� ������ ��������</param>
        /// <param name="element">��� ������� ��� ��������� �������</param>
        /// <param name="preserveAspect">������ ��������, ������������, ���������� �� �������� ���������</param>
        public static void SetWidthByCanvasPercent(float percentage, Canvas canvas, GameObject element, bool preserveAspect = true)
        {
            CheckArgs(percentage, canvas, element);

            var canvasRect = canvas.GetComponent<RectTransform>().rect;

            var rectTransform = element.GetComponent<RectTransform>();

            var newWidth = canvasRect.width / 100f * percentage;
            var aspect = GetRectTransformAspect(rectTransform);

            float newHeight = preserveAspect ? newWidth / aspect : rectTransform.sizeDelta.x;

            if (newHeight > canvasRect.height)
            {
                newHeight = canvasRect.height;
                newWidth = newHeight * aspect;
            }
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
            if (percentage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(percentage), $"Percentage must be greater than or equal to 0, got {percentage}");
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
                throw new MissingComponentException($"RectTransform component required! Please add it to the \"{element.name}\" element.");
            }
            if (rt.anchorMax != rt.anchorMin)
            {
                throw new InvalidOperationException($"The anchors of element \"{element.name}\" are not the same. Set anchorMin and anchorMax to the same value to get the expected result.");
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

            if (Mathf.Approximately(height, 0))
            {
                throw new ArgumentOutOfRangeException("Height must be greater than 0");
            }

            return width / height;
        }
    }
}