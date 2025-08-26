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

    private Plane _cachedSelectedPlane;
    private int _sign;

    private const float MAX_ANGLE = 90f;
    private const float ASPECT = PlaneConditionManager.ANGLE_CHANGE / MAX_ANGLE;

    float zAngle;

    void Start()
    {
        canvasRt = canvasObj.GetComponent<RectTransform>();
        SelectPlaneManager.OnSelect += CachePlane;
    }

    public void OnDrag(PointerEventData data)
    {
        if (_cachedSelectedPlane == null) return;

        LookAt2D(data.position);  // поворачиваем стрелку на нажатие
        TurnPlane();              // поворачиваем самолет, основываясь на повороте стрелки
    }

    private void LookAt2D(Vector2 pos)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRt, pos, cam, out Vector3 worldPoint);
        transform.up = worldPoint - transform.position;
    }

    private void TurnPlane()
    {
        zAngle = GetClampedAngle();
        transform.localRotation = Quaternion.Euler(0, 0, zAngle);
        var targetAngle = ASPECT * _sign * zAngle;

        PlaneConditionManager.ChangePitch(targetAngle, _cachedSelectedPlane);
    }

    private float GetClampedAngle() => Mathf.Clamp(Mathf.DeltaAngle(0, transform.localEulerAngles.z), -MAX_ANGLE, MAX_ANGLE);

    private void CachePlane(Plane plane) { _cachedSelectedPlane = plane; _sign = _cachedSelectedPlane.direction == Vector2.right ? 1 : -1; }

}
