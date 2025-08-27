using DG.Tweening;
using Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    public class SpeedArrowsController : MonoBehaviour
    {
        public const int
            MIN_SPEED_CHANGE_TIME = 5,   // � ��������
            MAX_SPEED_CHANGE_TIME = 8,   // � ��������
            SPEED_CHANGE = 10;

        [SerializeField] private SelectPlaneManager _manager;
        private readonly Dictionary<Plane, (Tween Tween, int EndValue, int StartValue)> _planeTweenPairs = new();

        private enum ChangeDirection
        {
            Up,
            Down
        }

        private void Start()
        {
            _planeTweenPairs.Clear();
        }
        private void ChangeSpeed(ChangeDirection direction)
        {
            var isUp = direction == ChangeDirection.Up;
            

            var selectedPlane = _manager.selectedPlane;
            if (selectedPlane == null) throw new InvalidOperationException("The selected plane is null � cannot continue.");

            var curSpeed = int.Parse(DataLinks.Instance.SpeedText.text);
            var targetSpeed = Math.Clamp(curSpeed + (isUp ? SPEED_CHANGE : -SPEED_CHANGE), Plane.MIN_SPEED, Plane.MAX_SPEED);
            DataLinks.Instance.SpeedText.text = targetSpeed.ToString();

            if (!_planeTweenPairs.ContainsKey(selectedPlane))
            {
                var duration = GetChangeTime();

                _planeTweenPairs[selectedPlane] = (Tween:
                    DOTween.To(() => selectedPlane.speed, selectedPlane.SetSpeed, targetSpeed, duration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => _planeTweenPairs.Remove(selectedPlane)), EndValue: targetSpeed, StartValue: selectedPlane.speed);
            }
            else
            {
                var value = _planeTweenPairs[selectedPlane];
                var animatedPlaneTweener = value.Tween as Tweener;

                // ������: ���� �� 10 �� 20 �� 5 ������. ������ �� �� 15. ����� �������� ����� �� 35
                var stepsPerSecond = Math.Abs(value.EndValue - value.StartValue) / animatedPlaneTweener.Duration();  // ������: (20 - 10) / 5 = 10 / 5 = 2 ���� � �������
                var newDuration = Math.Abs(targetSpeed - selectedPlane.speed) / stepsPerSecond; // ������: (35 - 15) / 2 = 20 (�����) / 2 = 10 ������ ��� ���� ��������

                animatedPlaneTweener.ChangeEndValue(targetSpeed, newDuration: newDuration, snapStartValue: true);
                _planeTweenPairs[selectedPlane] = (value.Tween, EndValue: targetSpeed, StartValue: selectedPlane.speed);
            }
        }

        public void SpeedUp() => ChangeSpeed(ChangeDirection.Up);
        public void SpeedDown() => ChangeSpeed(ChangeDirection.Down);

        public static float GetChangeTime() => UnityEngine.Random.Range(MIN_SPEED_CHANGE_TIME, MAX_SPEED_CHANGE_TIME);
    }
}