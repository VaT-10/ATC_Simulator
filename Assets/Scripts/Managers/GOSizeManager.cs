using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Animations;


[Serializable]
public class TooManyComponentsException : Exception
{
    public Type Type { get; private set; }
    public GameObject GameObject { get; private set; }
    public TooManyComponentsException() { }
    public TooManyComponentsException(string message) : base(message) { }
    public TooManyComponentsException(string message, Exception inner) : base(message, inner) { }
    public TooManyComponentsException(Type type, string message)
        : base($"Type: {type.Name}. {message}")
    {
        Type = type;
    }
    public TooManyComponentsException(Type type, GameObject gameObject, string message)
        : base($"Type: {type.Name}, GameObject: {gameObject.name}. {message}")
    {
        Type = type;
        GameObject = gameObject;
    }
    protected TooManyComponentsException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

namespace Managers
{
    /// <summary>
    /// менеджер размеров игровых объектов
    /// </summary>
    /// <exception cref="TooManyComponentsException">выбрасывается при наличии больше одного рендерера на объекте</exception>
    public static class GOSizeManager
    {
        private static Camera _cachedCamera;
        private static Dictionary<GameObject, SpriteRenderer> _rendererCache = new();
        public enum CacheType
        {
            CameraCache,
            RendererCache,
            All
        };  // для определения типа кэша, который необходимо очищать

        /// <summary>
        /// функция для получения размера игрового объекта по определенной оси по его рендереру
        /// </summary>
        /// <param name="gameObject">игровой объект, размер которого необходимо получить</param>
        /// <param name="axis">измеряемая ось</param>
        /// <returns>размер игрового объекта по определенной оси</returns>
        public static float GetGOSizeAlongAxis(GameObject gameObject, Axis axis)
        {
            Renderer renderer = GetValidRenderer(gameObject);

            return axis switch
            {
                Axis.X => renderer.bounds.size.x,
                Axis.Y => renderer.bounds.size.y,
                Axis.Z => renderer.bounds.size.z,
                Axis.None => throw new ArgumentException(nameof(axis)),
                _ => throw new ArgumentException(nameof(axis)),
            };
        }

        /// <summary>
        /// позволяет получить размеры экрана по осям X/Y в мировых координатах
        /// </summary>
        /// <param name="axis">ось</param>
        /// <returns>размер экрана по оси axis</returns>
        /// <exception cref="ArgumentException">возбуждается при получении Z-оси в axis</exception>
        public static float GetScreenSizeAlongAxis(Axis axis)
        {
            if (_cachedCamera == null)
            {
                _cachedCamera = Camera.main != null 
                    ? Camera.main 
                    : throw new InvalidOperationException("The camera (tag MainCamera) has not been found on the scene. Please check the main camera's tag and try again.");
            }

            if (axis == Axis.Z)
            {
                throw new ArgumentException($"Cannot get screen size along Z axis!");
            }

            var screenHeight = _cachedCamera.orthographicSize * 2;
            return axis == Axis.Y ? screenHeight : screenHeight * (Screen.width / (float)Screen.height);
        }

        /// <summary>
        /// выставляет размер 2D объекта по определенной оси в мировых координатах.
        /// P.S. также подходит для изменения размера UI-элементов, если они находятся в Canvas 
        /// </summary>
        /// <param name="gameObject">игровой объект, размер которого необходимо поменять.</param>
        /// <param name="axis">ось, по которой необходимо менять размер</param>
        /// <param name="targetSize">целевой размер объекта. указывается в мировых координатах при изменении размера GO, либо в ширине RectTransform при изменении размера UI-элемента</param>
        /// <param name="preserveAspect">важный параметр, определяющий, нужно ли сохранять соотношение сторон объекта</param>
        /// <exception cref="ArgumentOutOfRangeException">возбуждается при нулевом scale по оси у объекта</exception>
        public static void SetGOSizeAlongAxis(GameObject gameObject, Axis axis, float targetSize, bool preserveAspect = true, bool safe = true)
        {
            if (targetSize < 0) throw new ArgumentOutOfRangeException(nameof(targetSize), "Target size must not be negative!");
            if (axis == Axis.Z) throw new ArgumentException("Cannot set size along Z axis!");

            Canvas canvas = gameObject.GetComponentInParent<Canvas>();

            Vector3 localScale = (canvas == null) ? gameObject.transform.localScale : gameObject.GetComponent<RectTransform>().localScale;  // здесь и далее скобки в тернарном операторе добавлены для читаемости
            var axisScale = (axis == Axis.X) ? localScale.x : localScale.y;
            var axisSize = (canvas == null) ? GetGOSizeAlongAxis(gameObject, axis) : UISizeManager.GetElementSizeAlongAxis(gameObject, axis);

            var anotherAxisScale = (axis == Axis.X) ? localScale.y : localScale.x;

            if (Mathf.Approximately(axisScale, 0f) || axisScale < 0) throw new ArgumentOutOfRangeException(nameof(axis), "Cannot set size along axis with the scale below or equal to zero!");

            var axisTargetScale = targetSize / (axisSize / axisScale);
            var anotherAxisTargetScale = preserveAspect ? (anotherAxisScale / axisScale) * axisTargetScale : anotherAxisScale;

            if (safe)
            {
                var anotherAxis = (axis == Axis.X) ? Axis.Y : Axis.X;

                var anotherAxisSize = (canvas == null) ? GetGOSizeAlongAxis(gameObject, anotherAxis) : UISizeManager.GetElementSizeAlongAxis(gameObject, anotherAxis);
                var targetAxisSize = axisSize * axisTargetScale;
                var anotherAxisTargetSize = anotherAxisSize * anotherAxisTargetScale;

                var containerAxisSize = (canvas == null) ? GetScreenSizeAlongAxis(axis) : UISizeManager.GetCanvasSizeAlongAxis(axis);
                var containerAnotherAxisSize = (canvas == null) ? GetScreenSizeAlongAxis(anotherAxis) : UISizeManager.GetCanvasSizeAlongAxis(anotherAxis);

                if (targetAxisSize > containerAxisSize)
                {
                    axisTargetScale = containerAxisSize / (axisSize / axisScale);
                    anotherAxisTargetScale = preserveAspect ? (anotherAxisScale / axisScale) * axisTargetScale : anotherAxisScale;
                }
                if (anotherAxisTargetSize > containerAnotherAxisSize)
                {
                    anotherAxisTargetScale = containerAnotherAxisSize / (anotherAxisSize / anotherAxisScale);
                    axisTargetScale = preserveAspect ? (axisScale / anotherAxisScale) * anotherAxisTargetScale : axisTargetScale;
                }
            }

            gameObject.transform.localScale = new Vector3(
                axis == Axis.X ? axisTargetScale : anotherAxisTargetScale,
                axis == Axis.Y ? axisTargetScale : anotherAxisTargetScale,
                gameObject.transform.localScale.z);
        }

        /// <summary>
        /// выставляет размер объекта по определенной оси в процентах от экрана.
        /// </summary>
        /// <param name="gameObject">игровой объект, размер которого необходимо поменять.</param>
        /// <param name="axis">ось, по которой необходимо менять размер</param>
        /// <param name="targetPercentage">параметр, определяющий, сколько процентов от экрана по оси Axis будет занимать gameObject</param>
        /// <param name="preserveAspect">важный параметр, определяющий, нужно ли сохранять соотношение сторон объекта</param>
        public static void SetGOSizePercent(GameObject gameObject, Axis axis, float targetPercentage, bool preserveAspect = true, bool safe = true)
        {
            var targetSize = (gameObject.GetComponentInParent<Canvas>() != null 
                ? UISizeManager.GetCanvasSizeAlongAxis(axis)
                : GetScreenSizeAlongAxis(axis)) 
                / 100f * targetPercentage;

            SetGOSizeAlongAxis(gameObject, axis, targetSize, preserveAspect: preserveAspect, safe: safe);  // Нокс, знай, ты лучшая морская свинка в мире
        }

        /// <summary>
        /// находит валидный рендерер на объекте, используется для получения размеров игровых объектов
        /// </summary>
        /// <param name="gameObject">игровой объект</param>
        /// <returns>валидный рендерер</returns>
        /// <exception cref="MissingComponentException">выбрасывается при отсутствии рендерера на объекте</exception>
        /// <exception cref="TooManyComponentsException">выбрасывается при 2+ рендерерах</exception>
        private static Renderer GetValidRenderer(GameObject gameObject)
        {
            if (_rendererCache.TryGetValue(gameObject, out var cachedRenderer))
            {
                return cachedRenderer;
            }
            var renderers = gameObject.GetComponents<SpriteRenderer>();
            var rendererCount = renderers.Length;

            if (rendererCount == 0)
            {
                throw new MissingComponentException($"The Renderer component is missing on the {gameObject.name} gameObject! Add it, please.");
            }

            if (rendererCount > 1)
            {
                throw new TooManyComponentsException(typeof(SpriteRenderer), gameObject, $"More than one renderer has been found on the {gameObject.name} gameObject: {rendererCount}");
            }

            _rendererCache[gameObject] = renderers[0];  // кэшируем рендерер для последующего использования

            return renderers[0];
        }

        public static void ClearCache(CacheType cacheType = CacheType.All)
        {
            if (cacheType == CacheType.CameraCache || cacheType == CacheType.All)
            {
                _cachedCamera = null;  // очищаем кэш камеры
            }
            if (cacheType == CacheType.RendererCache || cacheType == CacheType.All)
            {
                _rendererCache.Clear();  // очищаем кэш рендереров
            }
        }
    }

}