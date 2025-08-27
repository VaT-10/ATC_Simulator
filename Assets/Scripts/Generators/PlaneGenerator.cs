using UnityEngine;
using Managers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

/// <summary>
/// калькулятор для координат спавна самолетов
/// </summary>
public static class PlaneCoordinatesCalculator
{
    public static readonly List<float> _planesYs = new();
    private static readonly List<float> _planesXs = new();

    /// <summary>
    /// вычисляет все возможные координаты спавна самолетов
    /// </summary>
    /// <param name="spawnOffset">расстояние между линиями спавна (в контексте игры, эшелонами)</param>
    /// <param name="maxSpawnY">самая высокая линяя спавна (в контексте игры, наивысший эшелон)</param>
    public static void CalculateCoordinates(float spawnOffset, float maxSpawnY, float ysCount, float maskEndX)
    {
        ysCount--;
        var minSpawnY = maxSpawnY - (spawnOffset * ysCount);
        for (float i = maxSpawnY; i >= minSpawnY; i -= spawnOffset)
        {
            _planesYs.Add(i);
        }

        _planesXs.Add(maskEndX);
        _planesXs.Add(-maskEndX);
    }

    /// <summary>
    /// возвращает две случайные координаты x и y из списков _planesYs и _planesXs соответственно
    /// </summary>
    /// <returns>две случайные координаты x и y</returns>
    public static Vector3 GetRandomSpawnPos()
    {
        float planeX = _planesXs[UnityEngine.Random.Range(0, 2)];
        float planeY = _planesYs[UnityEngine.Random.Range(0, _planesYs.Count)];

        return new Vector3(planeX, planeY);
    }
}

public class PlaneGenerator : MonoBehaviour
{
    [SerializeField, NotNull] private float _spawnRate;
    [SerializeField, NotNull] private GameObject _plane;  // префаб самолета

    [SerializeField, NotNull] private GameObject _mapBackground;
    [SerializeField, NotNull] private SelectPlaneManager _slm;

    private float _timer;  // таймер, используемый для подсчета времени, прошедшего со спавна предыдущего самолета. увеличивается на Time.deltaTime каждый кадр.

    private static readonly Vector3 PLANE_LOCAL_SCALE = new(0.11f, 0.11f);
    private const float SPAWN_OFFSET = 0.839f;
    private const float LOCAL_MAX_SPAWN_Y = 1.5265f;
    private const int YS_COUNT = 4;
    private const float LOCAL_MASK_END_X = 3.82f;

    void Start()
    {
        CheckArgs();
        PlaneCoordinatesCalculator.CalculateCoordinates(SPAWN_OFFSET, LOCAL_MAX_SPAWN_Y, YS_COUNT, LOCAL_MASK_END_X);
        DataLinks.ClearAllText();

        CreatePlane();
    }

    void Update()
    {
        if (_timer >= _spawnRate)
        {
            _timer = 0;
            CreatePlane();
        }
        else
        {
            _timer += Time.deltaTime;
        }
    }

    private void CheckArgs()
    {
        if (_plane == null) throw new NullReferenceException("The plane prefab is missing.");
        if (_mapBackground == null) throw new NullReferenceException("The gameObject of mapBackground is missing.");
        if (_spawnRate <= 0) throw new ArgumentOutOfRangeException($"The spawnRate should be greater than 0, got {_spawnRate}");
    }

    /// <summary>
    /// функция создает и настраивает новую копию префаба самолета
    /// </summary>
    private void CreatePlane()
    {
        Vector3 planeCoordinates = PlaneCoordinatesCalculator.GetRandomSpawnPos();

        GameObject generatedPlane = InstantiatePlane(planeCoordinates.x);
        SetPlaneDirection(generatedPlane);

        // taco 🌮
    }

    /// <summary>
    /// устанавливает направление самолета
    /// </summary>
    /// <param name="plane">объект самолета</param>
    private void SetPlaneDirection(GameObject plane)
    {
        Vector3 planeScreenDirection = (plane.transform.position.x < 0)? Vector3.right : Vector3.left;

        plane.GetComponent<Plane>().direction = planeScreenDirection;

        if (planeScreenDirection == Vector3.left)  // при движении влево необходимо развернуть самолет в другую сторону. эта проверка и делает это
        {
            Vector3 scale = plane.transform.localScale;
            scale.x *= -1;
            plane.transform.localScale = scale; 
        }
    }

    /// <summary>
    /// создает новый самолет в определенных координатах
    /// </summary>
    /// <param name="planeCoordinates">координаты самолета в виде кортежа, где первый элемент - x, а второй - y</param>
    /// <returns>объект созданного самолета</returns>
    private GameObject InstantiatePlane(float x)
    { 
        var plane = Instantiate(_plane, _mapBackground.transform);

        var rand = new System.Random();
        var planeComponent = plane.GetComponent<Plane>();

        var targetAltitude = rand.Choice(planeComponent.flightLevels);
        var targetY = PlaneCoordinatesCalculator._planesYs[Array.IndexOf(planeComponent.flightLevels, targetAltitude)];

        Debug.Log($"Target altitude:{targetAltitude}. Target Y: {targetY}.\nFlight levels: {{ {string.Join(", ", planeComponent.flightLevels)} }}. Planes Ys: {{ {string.Join(", ", PlaneCoordinatesCalculator._planesYs)} }}");

        plane.transform.localScale = PLANE_LOCAL_SCALE;
        plane.transform.SetLocalPositionAndRotation(new Vector2(x: x, y: targetY), Quaternion.identity);

        planeComponent.SetAltitude(targetAltitude);

        return plane;
    }
}

// where is my PIZZA 🍕???