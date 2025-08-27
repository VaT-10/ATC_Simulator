using System;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

namespace Managers
{
    /// <summary>
    /// класс для хранения TMP для отображения информации о самолете
    /// </summary>
    [Serializable]
    public class DataLinks
    {
        public static DataLinks Instance { get; private set; } = null;

        public TextMeshProUGUI FlightNameText { get; private set; }
        public TextMeshProUGUI PlaneModelText { get; private set; }
        public TextMeshProUGUI RouteText { get; private set; }
        public TextMeshProUGUI SpeedText { get; private set; }
        public TextMeshProUGUI AltitudeText { get; private set; }
        public SelectPlaneManager SPM { get; private set; }
        public PlaneConditionManager PCM { get; private set; }


        /// <summary>
        /// инициализирует все переменные синглтона
        /// </summary>
        /// <exception cref="MissingComponentException">выбрасывается при отсутствии TMP-компонента у какого-либо объекта</exception>
        public static void Initialize() 
        {
            if (Instance == null)
            {
                Instance = new DataLinks();
                var TMPExceptionText = "The game object with name {0} does not have the \"TextMeshPro\" component. Please check it and try again.";

                Instance.FlightNameText = GameObject.Find("FlightName").GetComponent<TextMeshProUGUI>()
                    ?? throw new MissingComponentException(string.Format(TMPExceptionText, "FlightName"));

                Instance.PlaneModelText = GameObject.Find("PlaneModel").GetComponent<TextMeshProUGUI>()
                    ?? throw new MissingComponentException(string.Format(TMPExceptionText, "PlaneModel"));

                Instance.RouteText = GameObject.Find("Route").GetComponent<TextMeshProUGUI>()
                    ?? throw new MissingComponentException(string.Format(TMPExceptionText, "Route"));

                Instance.SpeedText = GameObject.Find("Speed").GetComponent<TextMeshProUGUI>()
                    ?? throw new MissingComponentException(string.Format(TMPExceptionText, "Speed"));

                Instance.AltitudeText = GameObject.Find("Altitude").GetComponent<TextMeshProUGUI>()
                    ?? throw new MissingComponentException(string.Format(TMPExceptionText, "Altitude"));

                Instance.SPM = GameObject.FindWithTag("SPM")?.GetComponent<SelectPlaneManager>();
                Instance.PCM = GameObject.FindWithTag("PCM")?.GetComponent<PlaneConditionManager>();
            }
        }

        /// <returns>все объекты TMP, хранящиеся в классе</returns>
        public TextMeshProUGUI[] GetAllTMPs()
        {
            return new TextMeshProUGUI[] { FlightNameText, PlaneModelText, RouteText, SpeedText, AltitudeText };
        }

        /// <summary>
        /// очищает все TMP из полученного массива, заменяя их текст на пустую строку.
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
    /// класс для управления размером элементов интерфейса
    /// </summary>
    public static class UISizeManager
    {
        /// <summary>
        /// получает размер канваса по указанной оси (X или Y).
        /// </summary>
        /// <param name="axis">ось по которой необходимо получить размер канваса</param>
        /// <returns>размер канваса по указанной оси</returns>
        public static float GetCanvasSizeAlongAxis(Axis axis)
        {
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            
            return GetElementSizeAlongAxis(canvas.gameObject, axis);
        }

        /// <summary>
        /// получает размер элемента интерфейса по указанной оси (X или Y).
        /// </summary>
        /// <param name="axis">ось, по которой необходимо получить размер элемента</param>
        /// <param name="element">элемент, размер которого требуется получить</param>
        /// <returns>размер элемента интерфейса по указанной оси</returns>
        /// <exception cref="ArgumentNullException">выбрасывается при передаче null как элемент</exception>
        /// <exception cref="MissingComponentException">выбрасывается при отсутствии компонента RectTransform </exception>
        /// <exception cref="ArgumentException"></exception>
        public static float GetElementSizeAlongAxis(GameObject element, Axis axis)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element), "Element cannot be null.");
            }
            if (!element.TryGetComponent<RectTransform>(out var rectTransform))
            {
                throw new MissingComponentException($"RectTransform component is missing on the element: {element.name}. Please add it.");
            }
            return axis switch
            {
                Axis.X => rectTransform.sizeDelta.x,
                Axis.Y => rectTransform.sizeDelta.y,
                Axis.Z => throw new ArgumentException("Z axis is not supported for element size."),
                _ => throw new ArgumentException($"Invalid axis: {axis}. Use X or Y."),
            };
        }

        public static void SetDownCenterAnchors(GameObject element)
        {
            var rectTransform = element.GetComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
        }

        public static void SetPivot(Vector2 pivot, GameObject element)
        {
            RectTransform rectTransform;
            if (element.TryGetComponent(out rectTransform))
            {
                rectTransform.pivot = pivot;
            } else
            {
                throw new MissingComponentException("The RectTransform component is missing! Please add it for using this function.");
            }
        }

        /// <summary>
        /// функция для проверки аргументов для изменения размера в процентах на валидность
        /// </summary>
        /// <param name="percentage">проценты</param>
        /// <param name="canvas">канвас, относительно которого меняется размер элемента</param>
        /// <param name="element">сам элемент для изменения размера</param>
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

            if (!element.TryGetComponent<RectTransform>(out var rt))
            {
                throw new MissingComponentException($"RectTransform component required! Please add it to the \"{element.name}\" element.");
            }
            if (rt.anchorMax != rt.anchorMin)
            {
                throw new InvalidOperationException($"The anchors of element \"{element.name}\" are not the same. Set anchorMin and anchorMax to the same value to get the expected result.");
            }
        }

        /// <summary>
        /// функция для расчета пропорций rectTransform
        /// </summary>
        /// <param name="rectTransform">компонент, пропорции которого нужно найти</param>
        /// <returns>пропорции rectTransform</returns>
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