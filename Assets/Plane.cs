using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Managers;


/// <summary>
/// исключение, обозначающая отсутсвтвие или пустоту файла
/// </summary>
[Serializable]
public class FileNotFoundOrEmpty : Exception
{
    public FileNotFoundOrEmpty() { }
    public FileNotFoundOrEmpty(string message) : base(message) { }

    public FileNotFoundOrEmpty(string message,  Exception innerException) : base(message, innerException) { }

    protected FileNotFoundOrEmpty(
        System.Runtime.Serialization.SerializationInfo info, 
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

/// <summary>
/// класс для генерации случайной информации о рейсе, например, случайного города или имени рейса.
/// </summary>
public class FlightInformationGenerator
{
    private string[] _citiesList;
    private string[] _planesList;

    /// <summary>
    /// загружает файл с городами на английском языке и файл названиями моделей самолетов.
    /// </summary>
    /// <param name="fileName">имя файла</param>
    public FlightInformationGenerator(string citiesFileName, string planesFileName) 
    {
        _citiesList = GetTextAsset(citiesFileName);
        _planesList = GetTextAsset(planesFileName);  // загрузка всех файлов
    }

    /// <summary>
    /// загружает полученный файл из Assets/Resources и разбивает его на массив строк, разделяя через перенос строки.
    /// при отсутствии файла возбуждает исключение FileNotFoundOrEmpty
    /// </summary>
    /// <param name="fileName">имя файла, который необходимо загрузить</param>
    /// <returns>массив строк из полученного файла</returns>
    private static string[] GetTextAsset(string fileName)
    {
        var loadedAsset = Resources.Load<TextAsset>(fileName);  // загрузка текстового ассета из папки Assets/Resources
        if (loadedAsset == null || loadedAsset.text.Length < 2)
        {
            Debug.LogError($"File not found or empty: {fileName}");  // логирование ошибки при отсутствии файла или текста в нем.
            throw new FileNotFoundOrEmpty($"File not found or empty: {fileName}");  // создание системной ошибки при отсутствии файла или текста в нем.
        }
        return loadedAsset.text.Split('\n');
    }

    /// <summary>
    /// выбирает случайный элемент из массива.
    /// </summary>
    /// <param name="array">массив, из которого необходимо выбрать случайный элемент</param>
    /// <returns>случайный элемент из массива array</returns>
    private static T ChoiceRandomElement<T>(T[] array)
    {
        int randomIndex = UnityEngine.Random.Range(0, array.Length);  // получение случайного индекса
        return array[randomIndex];
    }

    /// <summary>
    /// выбирает случайный город из списка, полученного из файла.
    /// </summary>
    /// <returns>случайный город из списка, полученного из файла</returns>
    public string GenerateRandomCity()
    {
        return ChoiceRandomElement(_citiesList);
    }

    public string GenerateRandomPlaneModel()
    {
        return ChoiceRandomElement(_planesList);
    }

    /// <summary>
    /// генерирует случайное имя рейса в формате "AB1234", где "AB" - два случайных символа, а "1234" - случайное четырехзначное число.
    /// </summary>
    /// <returns>случайное имя рейса в формате "AB1234", где "AB" - два случайных символа, а "1234" - случайное четырехзначное число.</returns>
    public string GenerateRandomFlightName()
    {
        var firstChar = ((char)UnityEngine.Random.Range('A', 'Z' + 1)).ToString();  // генерация первого символа имени рейса самолета
        var secondChar = ((char)UnityEngine.Random.Range('A', 'Z' + 1)).ToString();  // генерация второго символа имени рейса самолета
        var number = UnityEngine.Random.Range(1000, 10000).ToString();  // генерация номера из имени рейса самолета

        #if UNITY_EDITOR
        Debug.Log($"Generated new flight name, first char: {firstChar}, second char: {secondChar}, number: {number}.");
        #endif

        return firstChar + secondChar + number;
    }

}


/// <summary>
/// основной класс самолета. содержит всю информацию о нем (генерируемую FlightInformationGenerator-ом).
/// управляет его положением, скоростью, и т.д.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class Plane : MonoBehaviour, IPointerClickHandler
{
    public int speed;  // скорость самолета в км/ч. отображается в окне инофрмации о самолете.
    public int altitude;  // высота полета самолета в км. отображается в окне инофрмации о самолете.
    public string flightName;  // уникальное имя рейса самолета, например "AB1234". отображается в окне инофрмации о самолете.
    public string planeModel;  // название модели самолета, например, "Boeing 737". отображается в окне инофрмации о самолете.
    public string destination;  // пункт назначения на английском языке, например, "Saint Petersburg". отображается в окне инофрмации о самолете.
    public string startingPlace;  // точка отправления на английском языке, например, "Moscow". отображается в окне инофрмации о самолете.
    public string direction;  // состояние самолета на английском языке. например, "horizontal flight", "climbing", "falling", "landing" и т.д. отображается в окне инофрмации о самолете стрелками. 
    public float moveSpeed;  // скорость движения объекта самолета по экрану.
    private FlightInformationGenerator _infoGenerator;  // генератор информации о рейсе. используется для генерации всех вышеперечисленных переменных.

    [HideInInspector] public Vector2 screenDirection;  // направление движения по экрану (Vector2.left / Vector2.right)
    private Rigidbody2D _rb;
    public float deadPoint = 2.9f;  // координата x, на которой самолет вылетает за пределы экрана.

    public string citiesTxtFileName;  // имя файла с названиями городов на английском языке.
    public string planesTxtFileName;  // имя файла с названиями моделей самолетов

    public bool isSelected = false;  // флаг. показывает выбран ли текущий самолет

    public SpriteRenderer spriteRenderer;
    private SelectPlaneManager _selectManager;

    /// <summary>
    /// передвигает самолет по экрану используя _rb.MovePosition. удаляет самолет при выходе за пределы экрана.
    /// </summary>
    public void MovePlane()
    {
        _rb.MovePosition(_rb.position + screenDirection * moveSpeed * Time.fixedDeltaTime);  // движение самолета через изменение _rb.position
        if ((screenDirection == Vector2.right && transform.position.x > deadPoint) ||
            (screenDirection == Vector2.left && transform.position.x < -deadPoint))  // проверка на выход за пределы экрана
        {
            Destroy(gameObject);  // удаление объекта при выходе за пределы экрана
        }
    }

    void FixedUpdate()
    {
        MovePlane();  // передвижение самолета без зависимости от FPS
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _selectManager = SelectPlaneManager.Instance;
        _rb = GetComponent<Rigidbody2D>();
        
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;  // установка интерполяции Rigidbody2D. необходима для плавного движения по экрану.
        _rb.gravityScale = 0;  // отключение гравитации. необходимо для того, чтобы самолеты не падали.
        _rb.bodyType = RigidbodyType2D.Kinematic;  // необходимо чтобы при столкновении двух коллайдеров они могли проходить сквозь друг друга.

        _selectManager.deSelectObject(this);  // сначала самолет должен быть невыбранным.

        GenerateFlightInfo();
        moveSpeed = 0.1f;  // установка скорости движения по экрану. 1.5f - временное значение, в будущем будет вычисляться на основе speed.

        SelectPlaneManager.onSelect += OnSelect;
    }

    private void OnSelect(Plane selectedScript)
    {
        if (isSelected) {
            _selectManager.deSelectObject(this);
            if (selectedScript == this)
            {
                TMPFlightInfoUIGroup.ClearAllText();  // очистка всех TMP из UI при снятии выбора с самого себя
            }
        }       
    }

    /// <summary>
    /// генерирует информацию о самолете с помощью FlightInformationGenerator
    /// </summary>
    private void GenerateFlightInfo()
    {
        _infoGenerator = new FlightInformationGenerator(citiesTxtFileName, planesTxtFileName);
        destination = _infoGenerator.GenerateRandomCity();
        flightName = _infoGenerator.GenerateRandomFlightName();
        planeModel = _infoGenerator.GenerateRandomPlaneModel();
        do { startingPlace = _infoGenerator.GenerateRandomCity(); } while (destination == startingPlace);  // генерация точки отправления, отличной от пункта назначения
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click detected!");
        if (!isSelected)
        {
            _selectManager.selectObject(this);
        }
        else
        {
            _selectManager.deSelectObject(this);
        }
    }

    private void OnDestroy()
    {
        SelectPlaneManager.onSelect -= OnSelect;
    }

}