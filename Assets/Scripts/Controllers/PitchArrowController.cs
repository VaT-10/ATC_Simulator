using Managers;
using UnityEngine;
using UnityEngine.EventSystems;

public class PitchArrowController : MonoBehaviour, IDragHandler
{
    [SerializeField]
    private GameObject canvasObj;
    private RectTransform canvasRt;
    [SerializeField]
    private Camera cam;

    private const float MAX_ANGLE = 90f;
    private const float ASPECT = PlaneConditionManager.ANGLE_CHANGE / MAX_ANGLE;

    float zAngle;

    void Start() => canvasRt = canvasObj.GetComponent<RectTransform>();

    public void OnDrag(PointerEventData data)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRt,
            data.position,
            cam,
            out Vector3 worldPoint
        );

        var direction = worldPoint - transform.position;
        transform.up = direction;
        zAngle = Mathf.Clamp(Mathf.DeltaAngle(0, transform.localEulerAngles.z), -MAX_ANGLE, MAX_ANGLE);
        transform.localRotation = Quaternion.Euler(0, 0, zAngle);

        var aircraft = SelectPlaneManager.Instance.selectedPlane;
        var targetAngle = ASPECT * (aircraft.direction == Vector2.right ? 1 : -1) * zAngle;

        PlaneConditionManager.ChangePitch(targetAngle, SelectPlaneManager.Instance.selectedPlane);
    }
}
