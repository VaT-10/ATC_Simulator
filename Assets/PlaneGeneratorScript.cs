using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Managers;

public class PlaneGeneratorScript : MonoBehaviour
{
    private float[] _planesYs = { 0.9f, 1.6f, 2.28f, 2.97f };  // координаты y на которых может появиться самолет
    private float[] _planesXs = { 3.7f, -3.7f };  // координаты x на которых может появиться самолет, т.е. справа и слева от экрана.
    private Dictionary<float, Vector2> _planesScreenDirections;  // словарь соответствий planesXs направлениям движения

    public float spawnRate;  // время, проходящее между генерацией самолетов
    public GameObject plane;  // префаб самолета

    private float _timer;  // таймер, используемый для подсчета времени, прошедшего со спавна предыдущего самолета. увеличивается на Time.deltaTime каждый кадр.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TMPFlightInfoUIGroup.ClearAllText();
        _planesScreenDirections = new Dictionary<float, Vector2>  // инициализация словаря planesScreenDirections
        {
            [_planesXs.Max()] = Vector2.left,
            [_planesXs.Min()] = Vector2.right
        };
        createPlane();  // создание первого самолета
    }

    // Update is called once per frame
    void Update()
    {
        if (_timer >= spawnRate)
        {
            _timer = 0;
            createPlane();  // обнуление таймера и создание самолета при прохождении определенного времени, указанного в переменной spawnRate
        }
        else
        {
            _timer += Time.deltaTime;  // увеличение таймера на время, прошедшее с предыдущего кадра
        }
    }

    public void createPlane()
    {
        /// <summary>
        /// функция создает и настраивает новую копию префаба самолета
        /// </summary>
        float planeY = _planesYs[Random.Range(0, _planesYs.Length)];  // выбор случайной координаты y для появления самолета
        float planeX = _planesXs[Random.Range(0, _planesXs.Length)];  // выбор случайной координаты x для появления самолета

        Vector3 planeScreenDirection = _planesScreenDirections[planeX];  // выбор направления движения самолета на основе координаты x

        #if UNITY_EDITOR
        Debug.Log($"planeY: {planeY}, planeX: {planeX}, planeScreenDirection: {planeScreenDirection}");  // логирование о появлении самолета и его данных для отладки
        #endif

        GameObject generatedPlane = Instantiate(plane, new Vector3(planeX, planeY), Quaternion.identity);  // создание копии префаба для дальнейшей настройки

        generatedPlane.GetComponent<Plane>().screenDirection = planeScreenDirection;  // передавание направления движения по экрану скрипту самолета

        if (planeScreenDirection == Vector3.left)  // при движении влево необходимо развернуть самолет в другую сторону. эта проверка и делает это
        {
            Vector3 scale = generatedPlane.transform.localScale;  // получение текущего scale сгенерированного самолета
            scale.x *= -1;  // разворот самолета посредством умножения x из scale на -1, делая его отрицательным
            generatedPlane.transform.localScale = scale;  // возвращение отредактированного scale на самолет
        }

    }

}
