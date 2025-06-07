using UnityEngine;
using Managers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// калькулятор для координат спавна самолетов
/// </summary>
public static class PlaneCoordinatesCalculator
{
    private static List<float> _planesYs = new List<float>();
    private static List<float> _planesXs = new List<float>();

    /// <summary>
    /// вычисляет все возможные координаты спавна самолетов
    /// </summary>
    /// <param name="spawnOffset">расстояние между линиями спавна (в контексте игры, эшелонами)</param>
    /// <param name="maxSpawnY">самая высокая линяя спавна (в контексте игры, наивысший эшелон)</param>
    public static void CalculateCoordinates(float spawnOffset, float maxSpawnY, float ysCount, float maskEndX)
    {
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
        float planeX = _planesXs[UnityEngine.Random.Range(0, _planesXs.Count)];
        float planeY = _planesYs[UnityEngine.Random.Range(0, _planesYs.Count)];

        return new Vector3(planeX, planeY);
    }
}

public class PlaneGenerator : MonoBehaviour
{
    [SerializeField, NotNull] private float _spawnRate;
    [SerializeField, NotNull] private GameObject _plane;  // префаб самолета

    [SerializeField, NotNull] private GameObject _mapBackground;

    private float _timer;  // таймер, используемый для подсчета времени, прошедшего со спавна предыдущего самолета. увеличивается на Time.deltaTime каждый кадр.

    static readonly Vector3 PLANE_LOCAL_SCALE = new Vector3(0.8f, 0.8f, 0.8f);
    private const float SPAWN_OFFSET = 0.9f;
    private const float LOCAL_MAX_SPAWN_Y = 1.8f;
    private const int YS_COUNT = 4;
    private const float LOCAL_MASK_END_X = 3.8f;

    void Start()
    {
        CheckArgs();
        PlaneCoordinatesCalculator.CalculateCoordinates(SPAWN_OFFSET, LOCAL_MAX_SPAWN_Y, YS_COUNT, LOCAL_MASK_END_X);
        TMPFlightInfoUIGroup.ClearAllText();

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
        if (_plane == null)
        {
            throw new NullReferenceException("The plane prefab is missing.");
        }
        if (_mapBackground == null)
        {
            throw new NullReferenceException("The gameObject of mapBackground is missing.");
        }
        if (_spawnRate <= 0)
        {
            throw new ArgumentOutOfRangeException($"The spawnRate should be greater than 0, got {_spawnRate}");
        }

    }

    /// <summary>
    /// функция создает и настраивает новую копию префаба самолета
    /// </summary>
    private void CreatePlane()
    {
        Vector3 planeCoordinates = PlaneCoordinatesCalculator.GetRandomSpawnPos();

        GameObject generatedPlane = InstantiatePlane(planeCoordinates);
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

        plane.GetComponent<Plane>().screenDirection = planeScreenDirection;

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
    private GameObject InstantiatePlane(Vector3 planeCoordinates) 
    { 
        var plane = Instantiate(_plane, _mapBackground.transform);
        plane.transform.localScale = PLANE_LOCAL_SCALE;
        plane.transform.localPosition = planeCoordinates;
        plane.transform.localRotation = Quaternion.identity;

        return plane;
    }
}

// where is my PIZZA 🍕???