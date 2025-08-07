using Managers;
using System;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// исключение, обозначающая отсутсвтвие или пустоту файла
/// </summary>
[Serializable]
public class FileNotFoundOrEmpty : Exception
{
    public FileNotFoundOrEmpty() { }
    public FileNotFoundOrEmpty(string message) : base(message) { }

    public FileNotFoundOrEmpty(string message, Exception innerException) : base(message, innerException) { }

    protected FileNotFoundOrEmpty(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

/// <summary>
/// класс для генерации случайной информации о рейсе, например, случайного города или имени рейса.
/// </summary>
public class FlightInformationGenerator
{
    private readonly string[] _citiesList;
    private readonly string[] _planesList;

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
        var number = UnityEngine.Random.Range(100, 10000).ToString();  // генерация номера из имени рейса самолета

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
    private const int SPEED_DIVIDER = 35_000;

    public const int 
        MIN_SPEED = 780,
        MAX_SPEED = 900;

    private const float DEAD_POINT = 2.9f;  // пока что магическое число. TODO: сделать более универсальным, чтобы не зависело от размера экрана.

    //[HideInInspector]
    public int
        speed,     // в км/ч
        altitude;  // в км
    private float _screenSpeed;  // реальная скорость на экране

    [HideInInspector]
    public string
        flightName,
        planeModel,
        destination,    // на англ.
        startingPlace,  // на англ.
        condition;      // напр. climbing, horizontal flight, stall и т.д.

    [SerializeField]
    private string
        citiesTxtFilename,  // города на англ.
        planesTxtFilename;

    [SerializeField] private Rigidbody2D _rb;
    public SpriteRenderer spriteRenderer;

    [HideInInspector] public Vector2 direction;  // влево/вправо
    [HideInInspector] public bool isSelected = false;

    private FlightInformationGenerator _infoGenerator;
    private SelectPlaneManager _selectManager;

    public int[] flightLevels = { 40, 30, 20, 5 };

    /// <summary>
    /// передвигает самолет по экрану используя _rb.MovePosition. удаляет самолет при выходе за пределы экрана.
    /// </summary>
    public void MovePlane()
    {
        transform.position += (Vector3)(_screenSpeed * Time.fixedDeltaTime * direction);  // движение самолета через изменение _rb.position
        if ((direction == Vector2.right && transform.position.x > DEAD_POINT) ||
            (direction == Vector2.left && transform.position.x < -DEAD_POINT))  // проверка на выход за пределы экрана
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
        _selectManager = SelectPlaneManager.Instance;

        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;  // установка интерполяции Rigidbody2D. необходима для плавного движения по экрану.
        _rb.gravityScale = 0;  // отключение гравитации. необходимо для того, чтобы самолеты не падали. ВРЕМЕННО
        _rb.bodyType = RigidbodyType2D.Kinematic;  // необходимо чтобы при столкновении двух коллайдеров они могли проходить сквозь друг друга.

        _selectManager.DeSelectObject(this);  // сначала самолет должен быть невыбранным.

        SetFlightInfo();

        SelectPlaneManager.OnSelect += OnSelect;
    }

    private void OnSelect(Plane selectedScript)
    {
        if (isSelected) _selectManager.DeSelectObject(this);
    }

    /// <summary>
    /// назначает информацию о самолете с помощью FlightInformationGenerator
    /// </summary>
    private void SetFlightInfo()
    {
        _infoGenerator = new FlightInformationGenerator(citiesTxtFilename, planesTxtFilename);

        flightName = _infoGenerator.GenerateRandomFlightName();
        planeModel = _infoGenerator.GenerateRandomPlaneModel();

        destination = _infoGenerator.GenerateRandomCity();
        do { startingPlace = _infoGenerator.GenerateRandomCity(); } while (destination == startingPlace);  // генерация точки отправления, отличной от пункта назначения

        SetSpeed(UnityEngine.Random.Range(MIN_SPEED, MAX_SPEED));
    }

    public void SetSpeed(int targetSpeed)
    {
        speed = targetSpeed;
        _screenSpeed = speed / (float)SPEED_DIVIDER;
    }

    public void SetAltitude(int targetAltitude) => altitude = targetAltitude;

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (isSelected) { _selectManager.DeSelectObject(this, self: true); } else { _selectManager.SelectObject(this); }
    }

    private void OnDestroy()
    {
        SelectPlaneManager.OnSelect -= OnSelect;
    }
}