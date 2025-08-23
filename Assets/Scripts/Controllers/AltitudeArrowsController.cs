using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Controllers
{
    public class AltitudeArrowsController : MonoBehaviour
    {
        private const float
            MIN_ALTITUDE_CHANGE_TIME = 30f,
            MAX_ALTITUDE_CHANGE_TIME = 45f;

        public const float ATTACK_ANGLE_CHANGE_TIME = 5f;
        public static readonly Vector3 ROTATE_ANGLE = new(x: 0f, y: 0f, z: 9.3f);
        [SerializeField] private PlaneConditionManager mngr;

        public enum ChangeDirection
        {
            Up,
            Down
        }

        private SelectPlaneManager _manager;
        private readonly Dictionary<Plane, (Tweener AltitudeTween,
                                            // Tweener YTween,
                                            TweenerCore<Quaternion, Vector3, QuaternionOptions> RotateTween,
                                            Tweener InnerTween,
                                            float StartAltitude,
                                            float EndAltitude)> _planeTweenPairs = new();

        private void Start()
        {
            _manager = SelectPlaneManager.Instance;
            mngr.SetIcon(PlaneConditionManager.IconType.HF);
        }

        public void ChangeAltitude(ChangeDirection direction)
        {
            var selectedPlane = _manager.selectedPlane;
            if (selectedPlane == null) throw new InvalidOperationException("The selected plane is null � cannot continue.");
            var curAltitude = int.Parse(TMPFlightInfoUIGroup.Instance.altitudeText.text.TrimEnd('K'));

            var isUp = direction == ChangeDirection.Up;
            mngr.SetIcon(isUp ? PlaneConditionManager.IconType.Climbing : PlaneConditionManager.IconType.Descent);
            

            var index = Array.IndexOf(selectedPlane.flightLevels, curAltitude);

            Debug.Log(index.ToString() + isUp);
            if ((index == 0 && isUp) || (index == selectedPlane.flightLevels.Length - 1 && !isUp))
            {
                return;
            }

            var nextAltitudeIndex = (index + (isUp ? -1 : 1)) % selectedPlane.flightLevels.Length;

            var targetAltitude = selectedPlane.flightLevels[nextAltitudeIndex];
            var targetY = PlaneCoordinatesCalculator._planesYs[nextAltitudeIndex];

            TMPFlightInfoUIGroup.Instance.altitudeText.text = targetAltitude.ToString() + 'K';
            RunAltitudeAnimation(targetAltitude, targetY);
        }

        private void RunAltitudeAnimation(float targetAltitude, float targetY)
        {
            var selectedPlane = _manager.selectedPlane;
            var duration = UnityEngine.Random.Range(MIN_ALTITUDE_CHANGE_TIME, MAX_ALTITUDE_CHANGE_TIME);

            var isUp = targetAltitude > selectedPlane.altitude;
            var targetAngle = selectedPlane.direction == (isUp ? Vector2.right : Vector2.left)
                    ? ROTATE_ANGLE
                    : ROTATE_ANGLE.Negative();

            if (!_planeTweenPairs.ContainsKey(selectedPlane))
            {
                _planeTweenPairs[selectedPlane] = (
                    AltitudeTween: DOTween.To(() => selectedPlane.altitude,
                                       altitude => selectedPlane.SetAltitude((int)altitude),
                                       targetAltitude, duration)
                                   .SetEase(Ease.Linear)  // �������� ��������������� ��� ��������� �����
                                   .OnComplete(() =>
                                   {
                                       _planeTweenPairs[selectedPlane].InnerTween.Play();
                                       _planeTweenPairs[selectedPlane].RotateTween.Kill();
                                       _planeTweenPairs.Remove(selectedPlane);
                                       mngr.SetIcon(PlaneConditionManager.IconType.HF);
                                   })
                                   .Pause(),

                    /* YTween: DOTween.To(() => GetPlaneY(selectedPlane),
                                y => SetPlaneY(selectedPlane, y),
                                targetY, duration)
                            .SetEase(Ease.Linear)
                            .Pause(), */

                    RotateTween: selectedPlane.transform.DORotate(targetAngle, ATTACK_ANGLE_CHANGE_TIME)
                                                        .OnComplete(() => _planeTweenPairs[selectedPlane].AltitudeTween.Play())
                                                        .SetEase(Ease.InOutSine)
                                                        .SetAutoKill(false),

                    InnerTween: selectedPlane.transform.DORotate(Vector3.zero, ATTACK_ANGLE_CHANGE_TIME)
                                                       .SetEase(Ease.InOutSine)
                                                       .Pause(),

                    StartAltitude: selectedPlane.altitude,
                    EndAltitude: targetAltitude
                    );
            }
            else if (_planeTweenPairs[selectedPlane].RotateTween.IsPlaying() && _planeTweenPairs[selectedPlane].StartAltitude == targetAltitude)
            {
                var (AltitudeTween, RotateTween, InnerTween, StartAltitude, EndAltitude) = _planeTweenPairs[selectedPlane];

                AltitudeTween.Kill();
                RotateTween.Kill();

                InnerTween.Play();
                _planeTweenPairs.Remove(selectedPlane);
            }
            else
            {
                var (AltitudeTween, RotateTween, InnerTween, StartAltitude, EndAltitude) = _planeTweenPairs[selectedPlane];

                var stepsPerSecond = Math.Abs(EndAltitude - StartAltitude) / AltitudeTween.Duration();
                var newDuration = Math.Abs(targetAltitude - selectedPlane.altitude) / stepsPerSecond;

                AltitudeTween.ChangeEndValue(targetAltitude, newDuration, true);
                RotateTween.ChangeEndValue(targetAngle, ATTACK_ANGLE_CHANGE_TIME, true).Restart();

                _planeTweenPairs[selectedPlane] = (AltitudeTween, RotateTween, InnerTween, selectedPlane.altitude, targetAltitude);
            }
        }

        private float GetPlaneY(Plane plane) => plane.transform.localPosition.y;
        private void SetPlaneY(Plane plane, float y)
        {
            float x = plane.transform.localPosition.x;
            plane.transform.localPosition = new(x, y);
        }

        public void AltitudeUp() => ChangeAltitude(ChangeDirection.Up);
        public void AltitudeDown() => ChangeAltitude(ChangeDirection.Down);
    }
}