using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Managers;


/// <summary>
/// ����������, ������������ ����������� ��� ������� �����
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
/// ����� ��� ��������� ��������� ���������� � �����, ��������, ���������� ������ ��� ����� �����.
/// </summary>
public class FlightInformationGenerator
{
    private string[] _citiesList;
    private string[] _planesList;

    /// <summary>
    /// ��������� ���� � �������� �� ���������� ����� � ���� ���������� ������� ���������.
    /// </summary>
    /// <param name="fileName">��� �����</param>
    public FlightInformationGenerator(string citiesFileName, string planesFileName) 
    {
        _citiesList = GetTextAsset(citiesFileName);
        _planesList = GetTextAsset(planesFileName);  // �������� ���� ������
    }

    /// <summary>
    /// ��������� ���������� ���� �� Assets/Resources � ��������� ��� �� ������ �����, �������� ����� ������� ������.
    /// ��� ���������� ����� ���������� ���������� FileNotFoundOrEmpty
    /// </summary>
    /// <param name="fileName">��� �����, ������� ���������� ���������</param>
    /// <returns>������ ����� �� ����������� �����</returns>
    private static string[] GetTextAsset(string fileName)
    {
        var loadedAsset = Resources.Load<TextAsset>(fileName);  // �������� ���������� ������ �� ����� Assets/Resources
        if (loadedAsset == null || loadedAsset.text.Length < 2)
        {
            Debug.LogError($"File not found or empty: {fileName}");  // ����������� ������ ��� ���������� ����� ��� ������ � ���.
            throw new FileNotFoundOrEmpty($"File not found or empty: {fileName}");  // �������� ��������� ������ ��� ���������� ����� ��� ������ � ���.
        }
        return loadedAsset.text.Split('\n');
    }

    /// <summary>
    /// �������� ��������� ������� �� �������.
    /// </summary>
    /// <param name="array">������, �� �������� ���������� ������� ��������� �������</param>
    /// <returns>��������� ������� �� ������� array</returns>
    private static T ChoiceRandomElement<T>(T[] array)
    {
        int randomIndex = UnityEngine.Random.Range(0, array.Length);  // ��������� ���������� �������
        return array[randomIndex];
    }

    /// <summary>
    /// �������� ��������� ����� �� ������, ����������� �� �����.
    /// </summary>
    /// <returns>��������� ����� �� ������, ����������� �� �����</returns>
    public string GenerateRandomCity()
    {
        return ChoiceRandomElement(_citiesList);
    }

    public string GenerateRandomPlaneModel()
    {
        return ChoiceRandomElement(_planesList);
    }

    /// <summary>
    /// ���������� ��������� ��� ����� � ������� "AB1234", ��� "AB" - ��� ��������� �������, � "1234" - ��������� �������������� �����.
    /// </summary>
    /// <returns>��������� ��� ����� � ������� "AB1234", ��� "AB" - ��� ��������� �������, � "1234" - ��������� �������������� �����.</returns>
    public string GenerateRandomFlightName()
    {
        var firstChar = ((char)UnityEngine.Random.Range('A', 'Z' + 1)).ToString();  // ��������� ������� ������� ����� ����� ��������
        var secondChar = ((char)UnityEngine.Random.Range('A', 'Z' + 1)).ToString();  // ��������� ������� ������� ����� ����� ��������
        var number = UnityEngine.Random.Range(1000, 10000).ToString();  // ��������� ������ �� ����� ����� ��������

        #if UNITY_EDITOR
        Debug.Log($"Generated new flight name, first char: {firstChar}, second char: {secondChar}, number: {number}.");
        #endif

        return firstChar + secondChar + number;
    }

}


/// <summary>
/// �������� ����� ��������. �������� ��� ���������� � ��� (������������ FlightInformationGenerator-��).
/// ��������� ��� ����������, ���������, � �.�.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class Plane : MonoBehaviour, IPointerClickHandler
{
    public int speed;  // �������� �������� � ��/�. ������������ � ���� ���������� � ��������.
    public int altitude;  // ������ ������ �������� � ��. ������������ � ���� ���������� � ��������.
    public string flightName;  // ���������� ��� ����� ��������, �������� "AB1234". ������������ � ���� ���������� � ��������.
    public string planeModel;  // �������� ������ ��������, ��������, "Boeing 737". ������������ � ���� ���������� � ��������.
    public string destination;  // ����� ���������� �� ���������� �����, ��������, "Saint Petersburg". ������������ � ���� ���������� � ��������.
    public string startingPlace;  // ����� ����������� �� ���������� �����, ��������, "Moscow". ������������ � ���� ���������� � ��������.
    public string direction;  // ��������� �������� �� ���������� �����. ��������, "horizontal flight", "climbing", "falling", "landing" � �.�. ������������ � ���� ���������� � �������� ���������. 
    public float moveSpeed;  // �������� �������� ������� �������� �� ������.
    private FlightInformationGenerator _infoGenerator;  // ��������� ���������� � �����. ������������ ��� ��������� ���� ����������������� ����������.

    [HideInInspector] public Vector2 screenDirection;  // ����������� �������� �� ������ (Vector2.left / Vector2.right)
    private Rigidbody2D _rb;
    public float deadPoint = 2.9f;  // ���������� x, �� ������� ������� �������� �� ������� ������.

    public string citiesTxtFileName;  // ��� ����� � ���������� ������� �� ���������� �����.
    public string planesTxtFileName;  // ��� ����� � ���������� ������� ���������

    public bool isSelected = false;  // ����. ���������� ������ �� ������� �������

    public SpriteRenderer spriteRenderer;
    private SelectPlaneManager _selectManager;

    /// <summary>
    /// ����������� ������� �� ������ ��������� _rb.MovePosition. ������� ������� ��� ������ �� ������� ������.
    /// </summary>
    public void MovePlane()
    {
        _rb.MovePosition(_rb.position + screenDirection * moveSpeed * Time.fixedDeltaTime);  // �������� �������� ����� ��������� _rb.position
        if ((screenDirection == Vector2.right && transform.position.x > deadPoint) ||
            (screenDirection == Vector2.left && transform.position.x < -deadPoint))  // �������� �� ����� �� ������� ������
        {
            Destroy(gameObject);  // �������� ������� ��� ������ �� ������� ������
        }
    }

    void FixedUpdate()
    {
        MovePlane();  // ������������ �������� ��� ����������� �� FPS
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _selectManager = SelectPlaneManager.Instance;
        _rb = GetComponent<Rigidbody2D>();
        
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;  // ��������� ������������ Rigidbody2D. ���������� ��� �������� �������� �� ������.
        _rb.gravityScale = 0;  // ���������� ����������. ���������� ��� ����, ����� �������� �� ������.
        _rb.bodyType = RigidbodyType2D.Kinematic;  // ���������� ����� ��� ������������ ���� ����������� ��� ����� ��������� ������ ���� �����.

        _selectManager.deSelectObject(this);  // ������� ������� ������ ���� �����������.

        GenerateFlightInfo();
        moveSpeed = 0.1f;  // ��������� �������� �������� �� ������. 1.5f - ��������� ��������, � ������� ����� ����������� �� ������ speed.

        SelectPlaneManager.onSelect += OnSelect;
    }

    private void OnSelect(Plane selectedScript)
    {
        if (isSelected) {
            _selectManager.deSelectObject(this);
            if (selectedScript == this)
            {
                TMPFlightInfoUIGroup.ClearAllText();  // ������� ���� TMP �� UI ��� ������ ������ � ������ ����
            }
        }       
    }

    /// <summary>
    /// ���������� ���������� � �������� � ������� FlightInformationGenerator
    /// </summary>
    private void GenerateFlightInfo()
    {
        _infoGenerator = new FlightInformationGenerator(citiesTxtFileName, planesTxtFileName);
        destination = _infoGenerator.GenerateRandomCity();
        flightName = _infoGenerator.GenerateRandomFlightName();
        planeModel = _infoGenerator.GenerateRandomPlaneModel();
        do { startingPlace = _infoGenerator.GenerateRandomCity(); } while (destination == startingPlace);  // ��������� ����� �����������, �������� �� ������ ����������
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