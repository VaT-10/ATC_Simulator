using DG.Tweening;
using Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    public class SpeedArrowsController : MonoBehaviour
    {
        private const int
            SPEED_CHANGE = 10,
            MIN_SPEED_CHANGE_TIME = 5,   // в секундах
            MAX_SPEED_CHANGE_TIME = 8;  // в секундах
        private SelectPlaneManager _manager;
        private readonly Dictionary<Plane, (Tween Tween, int EndValue, int StartValue)> _planeTweenPairs = new();

        private enum ChangeDirection
        {
            Up,
            Down
        }

        private void Start()
        {
            _manager = SelectPlaneManager.Instance;
            _planeTweenPairs.Clear();
        }
        private void ChangeSpeed(ChangeDirection direction)
        {
            var isUp = direction == ChangeDirection.Up;

            var selectedPlane = _manager.selectedPlane;
            if (selectedPlane == null) throw new InvalidOperationException("The selected plane is null — cannot continue.");

            var curSpeed = int.Parse(TMPFlightInfoUIGroup.Instance.speedText.text);
            var targetSpeed = Math.Clamp(curSpeed + (isUp ? SPEED_CHANGE : -SPEED_CHANGE), Plane.MIN_SPEED, Plane.MAX_SPEED);
            TMPFlightInfoUIGroup.Instance.speedText.text = targetSpeed.ToString();

            if (!_planeTweenPairs.ContainsKey(selectedPlane))
            {
                var duration = UnityEngine.Random.Range(MIN_SPEED_CHANGE_TIME, MAX_SPEED_CHANGE_TIME);

                _planeTweenPairs[selectedPlane] = (Tween:
                    DOTween.To(() => selectedPlane.speed, selectedPlane.SetSpeed, targetSpeed, duration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => _planeTweenPairs.Remove(selectedPlane)), EndValue: targetSpeed, StartValue: selectedPlane.speed);
            }
            else
            {
                var value = _planeTweenPairs[selectedPlane];
                var animatedPlaneTweener = value.Tween as Tweener;

                // ПРИМЕР: было от 10 до 20 за 5 секунд. сейчас мы на 15. нужно поменять конец на 35
                var stepsPerSecond = Math.Abs(value.EndValue - value.StartValue) / animatedPlaneTweener.Duration();  // ПРИМЕР: (20 - 10) / 5 = 10 / 5 = 2 шага в секунду
                var newDuration = Math.Abs(targetSpeed - selectedPlane.speed) / stepsPerSecond; // ПРИМЕР: (35 - 15) / 2 = 20 (шагов) / 2 = 10 секунд еще идти анимации

                animatedPlaneTweener.ChangeEndValue(targetSpeed, newDuration: newDuration, snapStartValue: true);
                _planeTweenPairs[selectedPlane] = (value.Tween, EndValue: targetSpeed, StartValue: selectedPlane.speed);
            }
        }

        public void SpeedUp() => ChangeSpeed(ChangeDirection.Up);
        public void SpeedDown() => ChangeSpeed(ChangeDirection.Down);
    }
}