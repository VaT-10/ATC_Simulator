using Managers;
using UnityEngine;

public class PlaneMirrorController : MonoBehaviour
{
    private float _previousZ;
    private SelectPlaneManager _mngr;

    private void Start() => _mngr = SelectPlaneManager.Instance;

    private void OnEnable()
    {
        var curScale = transform.localScale;
        curScale.x = _mngr.selectedPlane.transform.localScale.x;
        transform.localScale = curScale;
    }
    private void Update()
    {
        var curZ = GetCurZRotation();
        if (!Mathf.Approximately(curZ, _previousZ))
        {
            Mirror(curZ);
            _previousZ = curZ;
        }
    }

    private float GetCurZRotation() => _mngr.selectedPlane.transform.localEulerAngles.z;
    private void Mirror(float targetZ) => transform.localEulerAngles = new Vector3(0, 0, targetZ);
}
