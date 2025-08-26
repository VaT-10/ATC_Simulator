using Controllers;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��������� ������������ ����������� ��������: ����������, ������, �����������.
/// </summary>
public class PlaneConditionManager : MonoBehaviour
{
    private float _timer = 0f;             // ������ ��� �������� ������� �� ��������� ��������
    private const float CHECK_RATE = 20f;  // ������ ������ ������ � ��������� ����� ��� �� ���������

    private System.Random random = new();
    private const float
        // ��������� ����������
        CRITICAL_SPEED_OFFSET = 200f,

        // ����� ���������
        GROUND_Y = -1.75f,
        TT_FALLING_TIME = 15f,  // � ��������. TT � Ten Thousand

        // �����
        STALL_CHANCE = 0.1f,      // 10%
        TAILSPIN_CHANCE = 0.1f,   // 10% ������ ����������. 1%
        DIVING_CHANCE = 0.07f;    // 7%

    public const float ANGLE_CHANGE = 37f;  // относится к группе общих настроек

    [SerializeField]
    private Sprite
        climbingArrow,
        descentArrow,
        HFArrow,  // horizontal flight
        divingArrow,
        stallArrow,
        tailspinArrow;
    [SerializeField] private Image iconImage;

    public enum Condition
    {
        Climbing,
        Descent,
        HF,
        Diving,
        Stall,
        Tailspin
    }

    private enum CriticalSpeed 
    { 
        Max = Plane.MAX_SPEED + (int)CRITICAL_SPEED_OFFSET, 
        Min = Plane.MIN_SPEED - (int)CRITICAL_SPEED_OFFSET
    };

    // Update is called once per frame
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= CHECK_RATE)
        {
            _timer = 0f;
            CheckPlanes();
        }
    }

    private void CheckPlanes()
    {
        if (Random.value < DIVING_CHANCE)
        {
            var randPlane = PickRandomPlane();
            StartDiving(randPlane);
        }
        else if (Random.value < STALL_CHANCE)
        {
            var randPlane = PickRandomPlane();
            StartStall(randPlane);
        }
    }

    private GameObject PickRandomPlane()
    {
        var allPlanes = GameObject.FindGameObjectsWithTag("Plane");
        return random.Choice(allPlanes);
    }

    /// <summary>
    /// �������� ���������� ��������: �������� ������, ��������� �������
    /// </summary>
    /// <param name="stallingPlane"></param>
    private void StartStall(GameObject stallingPlane)
    {
        var planeComponent = stallingPlane.GetComponent<Plane>();  // ��������

        ChangePitch(AltitudeArrowsController.ROTATE_ANGLE.z + ANGLE_CHANGE, planeComponent);  // ��������� ������� (���� �����)
        SetCriticalSpeed(planeComponent, CriticalSpeed.Min).OnComplete(() =>                            // ������� �������� ��-�� ������� ���� ������������
        StartFalling(planeComponent));                                                                  // ������� ������� �������� � �������� ������.

        Debug.Log("��� ������� ������ �����");
    }

    private void StartTailspin(GameObject tailspinningPlane) { /* TODO */ }

    /// <summary>
    /// �������� ����������� ��������: ��������� ������, ��������, ��������� �������
    /// </summary>
    /// <param name="divingPlane">���������� �������</param>
    private void StartDiving(GameObject divingPlane)
    {
        // ��� �������� ��������� �����. �����������.
        var planeComponent = divingPlane.GetComponent<Plane>();  // ��������

        ChangePitch(-ANGLE_CHANGE, planeComponent).OnComplete(() =>                              // ������ ������, ��������� ������� ����� � �����
        {
            SetCriticalSpeed(planeComponent, CriticalSpeed.Max, GetFallingTime(planeComponent));  // ������������� �������� ���� ������������. ����� �������� ������������� ��� ����� �������
            StartFalling(planeComponent);                                                         // ������� ������
        });

        Debug.Log("��� ������� ������ ����� ���� �����");
    }

    /// <summary>
    /// �������� "������" �������
    /// </summary>
    /// <param name="angle">���� ������� (��� z �� localRotation)</param>
    /// <param name="plane">�������, ������ �������� ����� ��������</param>
    /// <returns>Tweener ��������� �������</returns>
    public static Tweener ChangePitch(float angle, Plane planeComponent)
    {
        planeComponent.transform.DOKill();
        var curAngle = planeComponent.transform.localRotation;
        var targetAngle = new Vector3(x: curAngle.x, y: curAngle.y, z: angle);

        if (planeComponent.direction == Vector2.left) targetAngle = targetAngle.Negative();
        var targetDuration = (AltitudeArrowsController.ATTACK_ANGLE_CHANGE_TIME / AltitudeArrowsController.ROTATE_ANGLE.z) * Mathf.Abs(curAngle.z - targetAngle.z);  // ������ ��� ����������
        Debug.Log($"({AltitudeArrowsController.ATTACK_ANGLE_CHANGE_TIME} / {AltitudeArrowsController.ROTATE_ANGLE.z}) * {Mathf.Abs(curAngle.z - targetAngle.z)} = {(AltitudeArrowsController.ATTACK_ANGLE_CHANGE_TIME / AltitudeArrowsController.ROTATE_ANGLE.z)} * {Mathf.Abs(curAngle.z - targetAngle.z)} = {targetDuration}");
        return planeComponent.transform.DORotate(targetAngle, targetDuration);
    }

    /// <summary>
    /// ��������� �������� ������� ��������. ������������ �� ���� ����������� ���������
    /// </summary>
    /// <param name="planeComponent">��������� ������</param>
    /// <returns>Sequence � ���������� ������� � y � altitude</returns>
    private Sequence StartFalling(Plane planeComponent)
    {
        var fallingTime = GetFallingTime(planeComponent);

        return DOTween.Sequence()
            .Append(
                DOTween.To(() => planeComponent.transform.localPosition.y, planeComponent.transform.SetLocalY, GROUND_Y, fallingTime)
                    .SetEase(Ease.InSine)
            )
            .Join(
                DOTween.To(() => planeComponent.altitude, planeComponent.SetAltitude, 0, fallingTime)
                    .SetEase(Ease.InSine)  // ����������� �� ���� �������
            );
    }

    /// <summary>
    /// ��������� ��������� �������� �� �����������.
    /// </summary>
    /// <param name="planeComponent">������� ��� ��������� ��������</param>
    /// <returns>Tweener ��������� ��������</returns>
    private Tweener SetCriticalSpeed(Plane planeComponent, CriticalSpeed criticalSpeedType, float? customDuration = null)
    {
        var criticalSpeed = (int)criticalSpeedType;

        // MAX_SPEED_CHANGE_TIME � �� SPEED_CHANGE ��/�. ����� ������� ������� ����� ������ �� criticalSpeed � ����� �� ��� � �� �� ����������/���������� ���� ��� ���������� �������
        var speedUnits = Mathf.Abs(criticalSpeed - planeComponent.speed) / SpeedArrowsController.SPEED_CHANGE;
        var targetDuration = customDuration ?? SpeedArrowsController.GetChangeTime() * speedUnits / 2;

        return DOTween.To(
            getter: () => planeComponent.speed,
            setter: planeComponent.SetSpeed,
            endValue: criticalSpeed,
            duration: targetDuration
        );
    }

    private float GetFallingTime(Plane planeComponent) => TT_FALLING_TIME * (planeComponent.altitude / 10f);

    public void SetIcon(Condition iconType)
    {
        switch (iconType)
        {
            case Condition.Climbing: iconImage.sprite = climbingArrow; break;
            case Condition.Descent: iconImage.sprite = descentArrow; break;
            case Condition.Diving: iconImage.sprite = divingArrow; break;
            case Condition.HF: iconImage.sprite = HFArrow; break;
            case Condition.Stall: iconImage.sprite = stallArrow; break;
            case Condition.Tailspin: iconImage.sprite = tailspinArrow; break;
            default: break;
        }
    }
}
