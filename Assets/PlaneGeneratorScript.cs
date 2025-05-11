using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Managers;

public class PlaneGeneratorScript : MonoBehaviour
{
    private float[] _planesYs = { 0.9f, 1.6f, 2.28f, 2.97f };  // ���������� y �� ������� ����� ��������� �������
    private float[] _planesXs = { 3.7f, -3.7f };  // ���������� x �� ������� ����� ��������� �������, �.�. ������ � ����� �� ������.
    private Dictionary<float, Vector2> _planesScreenDirections;  // ������� ������������ planesXs ������������ ��������

    public float spawnRate;  // �����, ���������� ����� ���������� ���������
    public GameObject plane;  // ������ ��������

    private float _timer;  // ������, ������������ ��� �������� �������, ���������� �� ������ ����������� ��������. ������������� �� Time.deltaTime ������ ����.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TMPFlightInfoUIGroup.ClearAllText();
        _planesScreenDirections = new Dictionary<float, Vector2>  // ������������� ������� planesScreenDirections
        {
            [_planesXs.Max()] = Vector2.left,
            [_planesXs.Min()] = Vector2.right
        };
        createPlane();  // �������� ������� ��������
    }

    // Update is called once per frame
    void Update()
    {
        if (_timer >= spawnRate)
        {
            _timer = 0;
            createPlane();  // ��������� ������� � �������� �������� ��� ����������� ������������� �������, ���������� � ���������� spawnRate
        }
        else
        {
            _timer += Time.deltaTime;  // ���������� ������� �� �����, ��������� � ����������� �����
        }
    }

    public void createPlane()
    {
        /// <summary>
        /// ������� ������� � ����������� ����� ����� ������� ��������
        /// </summary>
        float planeY = _planesYs[Random.Range(0, _planesYs.Length)];  // ����� ��������� ���������� y ��� ��������� ��������
        float planeX = _planesXs[Random.Range(0, _planesXs.Length)];  // ����� ��������� ���������� x ��� ��������� ��������

        Vector3 planeScreenDirection = _planesScreenDirections[planeX];  // ����� ����������� �������� �������� �� ������ ���������� x

        #if UNITY_EDITOR
        Debug.Log($"planeY: {planeY}, planeX: {planeX}, planeScreenDirection: {planeScreenDirection}");  // ����������� � ��������� �������� � ��� ������ ��� �������
        #endif

        GameObject generatedPlane = Instantiate(plane, new Vector3(planeX, planeY), Quaternion.identity);  // �������� ����� ������� ��� ���������� ���������

        generatedPlane.GetComponent<Plane>().screenDirection = planeScreenDirection;  // ����������� ����������� �������� �� ������ ������� ��������

        if (planeScreenDirection == Vector3.left)  // ��� �������� ����� ���������� ���������� ������� � ������ �������. ��� �������� � ������ ���
        {
            Vector3 scale = generatedPlane.transform.localScale;  // ��������� �������� scale ���������������� ��������
            scale.x *= -1;  // �������� �������� ����������� ��������� x �� scale �� -1, ����� ��� �������������
            generatedPlane.transform.localScale = scale;  // ����������� ������������������ scale �� �������
        }

    }

}
