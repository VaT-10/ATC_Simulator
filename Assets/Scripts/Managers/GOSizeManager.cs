using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;


[Serializable] 
public class TooManyComponentsException : Exception
{
    public Type Type {  get; private set; }
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
    /// <exception cref="TooManyComponentsException">выбрасывается при наличии больше одного коллайдера на объекте</exception>
    public static class GOSizeManager
    {
        /// <summary>
        /// функция для получения размера игрового объекта по определенной оси по его рендереру
        /// </summary>
        /// <param name="gameObject">игровой объект, размер которого необходимо получить</param>
        /// <param name="axis">измеряеммая ось</param>
        /// <returns>размер игрового объекта по определенной оси</returns>
        public static float GetGOSizeAlongAxis(GameObject gameObject, Axis axis)
        {
            Renderer renderer = GetValidRenderer(gameObject);

            var axisToSize = new Dictionary<Axis, float>()
            {
                {Axis.X, renderer.bounds.size.x},
                {Axis.Y, renderer.bounds.size.y},
                {Axis.Z, renderer.bounds.size.z}
            };  // создаем словарь соответствий между каждой осью и соответствующей

            return axisToSize[axis];
        }

        /// <summary>
        /// позволяет получить размеры экрана по осям X/Y в мировых координатах
        /// </summary>
        /// <param name="axis">ось</param>
        /// <returns>размер экрана по оси axis</returns>
        /// <exception cref="ArgumentException">возбуждается при получении Z-оси в axis</exception>
        public static float GetScreenSizeAlongAxis(Axis axis)
        {
            var camera = Camera.main;
            if (axis == Axis.Z)
            {
                throw new ArgumentException($"Cannot get screen size along Z axis (it's 2D, you idiot)!");
            }

            var screenHeight = camera.orthographicSize * 2;
            return axis == Axis.Y ? screenHeight : screenHeight * (Screen.width / Screen.height);

            // f лягушке.
        }

        /// <summary>
        /// выставляет размер объекта по определенной оси в мировых координатах.
        /// </summary>
        /// <param name="gameObject">игровой объект, размер которого необходимо поменять.</param>
        /// <param name="axis">ось, по которой необходимо менять размер</param>
        /// <param name="targetSize">целевой размер объекта</param>
        /// <param name="preserveAspect">важный параметр, определяющий, нужно ли сохранять соотношение сторон объекта</param>
        /// <exception cref="ArgumentOutOfRangeException">возбуждается при нулевом scale по оси у объекта</exception>
        public static void SetGOSizeAlongAxis(GameObject gameObject, Axis axis, float targetSize, bool preserveAspect = true)
        {
            if (targetSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetSize), "Target size must not be negative (do you seriously think I can make object smaller than it can be?)");
            }

            Vector3 localScale = gameObject.transform.localScale;

            Axis[] axes = { Axis.X, Axis.Y, Axis.Z };
            float[] scales = { localScale.x, localScale.y, localScale.z };

            var axisToScale = axes.Zip(scales, (axis, scale) => (axis, scale)).ToDictionary(t => t.axis, t => t.scale);  // нужен для получения значения scale по оси

            float curTargetAxisScale = axisToScale[axis];

            if (Mathf.Approximately(curTargetAxisScale, 0))
            {
                throw new ArgumentOutOfRangeException($"Setting the size failed: the {axis} scale must not be zero!");
            }

            float targetAxisScale = targetSize / (GetGOSizeAlongAxis(gameObject, axis) / curTargetAxisScale);  // получаем scale, при котором ширина станет такой же, как и целевая. один из финальных этапов

            for (int i = 0; i < scales.Length; i++)
            {
                if (!preserveAspect && axes[i] != axis)
                {
                    continue;  // если нет необходимости соблюдать соотношение сторон то необходимо пропускать всё, ось чего не равна той, scale которой необходимо изменить
                }
                if (Mathf.Approximately(scales[i], 0))
                {
                    Debug.LogWarning($"Cannot preserve aspect: the {axes[i]} scale must not be zero! The final result may differ from the expected one.");
                    continue;
                }
                var aspect = curTargetAxisScale / scales[i];
                scales[i] = targetAxisScale / aspect;
            }

            gameObject.transform.localScale = new Vector3(scales[0], scales[1], scales[2]);
        }

        /// <summary>
        /// выставляет размер объекта по определенной оси в процентах от экрана.
        /// </summary>
        /// <param name="gameObject">игровой объект, размер которого необходимо поменять.</param>
        /// <param name="axis">ось, по которой необходимо менять размер</param>
        /// <param name="targetPercentage">параметр, определяющий, сколько процентов от экрана по оси Axis будет занимать gameObject</param>
        /// <param name="preserveAspect">важный параметр, определяющий, нужно ли сохранять соотношение сторон объекта</param>
        public static void SetGOSizePercent(GameObject gameObject, Axis axis, float targetPercentage, bool preserveAspect)
        {
            var targetSize = (GetScreenSizeAlongAxis(axis) / 100f) * targetPercentage;

            SetGOSizeAlongAxis(gameObject, axis, targetSize, preserveAspect: preserveAspect);  // Нокс, знай, ты лучшая морская свинка в мире
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
            var renderers = gameObject.GetComponents<Renderer>();
            var rendererCount = renderers.Length;

            if (rendererCount == 0)
            {
                throw new MissingComponentException($"The Renderer component is missing on the {gameObject.name} gameObject.");
            }

            if (rendererCount > 1)
            {
                throw new TooManyComponentsException(typeof(Renderer), gameObject, $"More than one renderer has been found on the {gameObject.name} gameObject.");
            }

            return renderers[0];
        }
    }

}


// 🍔 mcdonald's