using Managers;
using UnityEngine;
using DG.Tweening;

public class PlaneMirrorController : MonoBehaviour
{
    private float _previousZ;
    private SelectPlaneManager _mngr;
    private Tweener _shake;

    private void Start() => _mngr = SelectPlaneManager.Instance;

    private void OnEnable()
    {
        var curScale = transform.localScale;
        curScale.x *= Mathf.Sign(_mngr.selectedPlane.transform.localScale.x);
        transform.localScale = curScale;

        _shake ??= transform.DOShakePosition(0.5f, 4f, 3).SetLoops(-1);
        _shake.Play();
    }

    private void OnDisable() => _shake.Pause();

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
