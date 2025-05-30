using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Managers;
using System;

public class PlaneGeneratorScript : MonoBehaviour
{
    private float[] _planesYs = { 0.9f, 1.6f, 2.28f, 2.97f };  // ���������� y �� ������� ����� ��������� �������
    private float[] _planesXs = { 3.7f, -3.7f };  // ���������� x �� ������� ����� ��������� �������, �.�. ������ � ����� �� ������.

    [SerializeField] private float _spawnRate;  // �����, ���������� ����� ���������� ���������
    [SerializeField] private GameObject _plane;  // ������ ��������

    [SerializeField] private GameObject _mapBackground;

    private float _timer;  // ������, ������������ ��� �������� �������, ���������� �� ������ ����������� ��������. ������������� �� Time.deltaTime ������ ����.


    void Start()
    {
        CheckArgs();
        TMPFlightInfoUIGroup.ClearAllText();
        CreatePlane();  // �������� ������� ��������
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
    /// ������� ������� � ����������� ����� ����� ������� ��������
    /// </summary>
    private void CreatePlane()
    {
        Vector3 planeCoordinates = GetRandomSpawnPos();

        GameObject generatedPlane = InstantiatePlane(planeCoordinates);
        SetPlaneDirection(generatedPlane);
    }

    /// <summary>
    /// ���������� ��� ��������� ���������� x � y �� ������� _planesYs � _planesXs ��������������
    /// </summary>
    /// <returns>��� ��������� ���������� x � y</returns>
    private Vector3 GetRandomSpawnPos()
    {
        float planeX = _planesXs[UnityEngine.Random.Range(0, _planesXs.Length)];
        float planeY = _planesYs[UnityEngine.Random.Range(0, _planesYs.Length)]; 

        return new Vector3(planeX, planeY);
    }

    /// <summary>
    /// ������������� ����������� ��������
    /// </summary>
    /// <param name="plane">������ ��������</param>
    private void SetPlaneDirection(GameObject plane)
    {
        Vector3 planeScreenDirection = (plane.transform.position.x < 0)? Vector3.right : Vector3.left;  // ����� ����������� �������� �������� �� ������ ���������� x

        plane.GetComponent<Plane>().screenDirection = planeScreenDirection;

        if (planeScreenDirection == Vector3.left)  // ��� �������� ����� ���������� ���������� ������� � ������ �������. ��� �������� � ������ ���
        {
            Vector3 scale = plane.transform.localScale;
            scale.x *= -1;
            plane.transform.localScale = scale; 
        }
    }

    /// <summary>
    /// ������� ����� ������� � ������������ �����������
    /// </summary>
    /// <param name="planeCoordinates">���������� �������� � ���� �������, ��� ������ ������� - x, � ������ - y</param>
    /// <returns>������ ���������� ��������</returns>
    private GameObject InstantiatePlane(Vector3 planeCoordinates) { return Instantiate(_plane, planeCoordinates, Quaternion.identity, _mapBackground.transform); }

}
