using Managers;
using UnityEngine;

public class PlaneMirrorController : MonoBehaviour
{
    private float _previousZ;
    [SerializeField] private SelectPlaneManager _mngr;

    private bool _isFirstTime = true;


    private void OnEnable()
    {
        if (_isFirstTime)
        {
            _isFirstTime = false;
            return;
        }

        var curScale = transform.localScale;
        curScale.x = Mathf.Abs(curScale.x) * Mathf.Sign(_mngr.selectedPlane.transform.localScale.x);
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
    private void Mirror(float targetZ)
    {
        transform.localEulerAngles = new Vector3(0, 0, targetZ); var curScale = transform.localScale;
        curScale.x = Mathf.Abs(curScale.x) * Mathf.Sign(_mngr.selectedPlane.transform.localScale.x);
        transform.localScale = curScale;
    }
}
