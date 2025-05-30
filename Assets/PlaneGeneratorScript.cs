using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Managers;
using System;

public class PlaneGeneratorScript : MonoBehaviour
{
    private float[] _planesYs = { 0.9f, 1.6f, 2.28f, 2.97f };  // координаты y на которых может появиться самолет
    private float[] _planesXs = { 3.7f, -3.7f };  // координаты x на которых может появиться самолет, т.е. справа и слева от экрана.

    [SerializeField] private float _spawnRate;  // время, проходящее между генерацией самолетов
    [SerializeField] private GameObject _plane;  // префаб самолета

    [SerializeField] private GameObject _mapBackground;

    private float _timer;  // таймер, используемый для подсчета времени, прошедшего со спавна предыдущего самолета. увеличивается на Time.deltaTime каждый кадр.


    void Start()
    {
        CheckArgs();
        TMPFlightInfoUIGroup.ClearAllText();
        CreatePlane();  // создание первого самолета
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
        Vector3 planeCoordinates = GetRandomSpawnPos();

        GameObject generatedPlane = InstantiatePlane(planeCoordinates);
        SetPlaneDirection(generatedPlane);
    }

    /// <summary>
    /// возвращает две случайные координаты x и y из списков _planesYs и _planesXs соответственно
    /// </summary>
    /// <returns>две случайные координаты x и y</returns>
    private Vector3 GetRandomSpawnPos()
    {
        float planeX = _planesXs[UnityEngine.Random.Range(0, _planesXs.Length)];
        float planeY = _planesYs[UnityEngine.Random.Range(0, _planesYs.Length)]; 

        return new Vector3(planeX, planeY);
    }

    /// <summary>
    /// устанавливает направление самолета
    /// </summary>
    /// <param name="plane">объект самолета</param>
    private void SetPlaneDirection(GameObject plane)
    {
        Vector3 planeScreenDirection = (plane.transform.position.x < 0)? Vector3.right : Vector3.left;  // выбор направления движения самолета на основе координаты x

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
    private GameObject InstantiatePlane(Vector3 planeCoordinates) { return Instantiate(_plane, planeCoordinates, Quaternion.identity, _mapBackground.transform); }

}
