using System.Collections.Generic;
using UnityEngine;

public class LandscapeGenerator : MonoBehaviour
{
    /// <summary>
    /// √енератор гор.
    /// 
    /// Ћќ√» ј:
    ///    - Ѕольша€ гора Ч гора с высотой в диапазоне от MAX_HEIGHT - 10 000 до MAX_HEIGHT включительно.
    ///    Ч ћаленька€ гора Ч гора с высотой в диапазоне от MIN_HEIGHT до MAX_HEIGHT - 10000 невключительно.
    ///    Ч ≈сли гора одна, она больша€. ≈сли две Ч это комплекс из большой и маленькой. ≈сли три - комплекс и одна небольша€.
    ///    Ч Ўанс на 3 горы Ч 25%. Ќа две и одну Ч 50%. Ќа ноль Ч 20%. 
    /// </summary>
    public class MountainGenerator
    {
        [SerializeField] private GameObject mountainPrefab;

        private const int
            MIN_HEIGHT = 20000,  // футы
            MAX_HEIGHT = 35000,  // футы

            MIN_MOUNTAIN_COUNT = 0,
            MAX_MOUNTAIN_COUNT = 3;

        private const float
            MIN_SCALE = 0.164f,  // y, на котором находитс€ MIN_HEIGHT
            MIN_SPAWN_X = -1.85f,
            MAX_SPAWN_X = 3f,

            ASPECT = MIN_SCALE / MIN_HEIGHT,

            COMPLEX_X_OFFSET = 0.2f,
            MAX_X_OFFSET = 0.7f;

        private Vector3 _mountainLocalScale;
        private enum MountainType
        {
            BM,  // Big Mountain
            SM   // Small Mountain
        }

        public MountainGenerator() => _mountainLocalScale = mountainPrefab.transform.localScale;  // кэшируем дл€ быстрого доступа

        private void GenerateMountains()
        {
            var chance = Random.value;
            int mountainCount;

            if (chance < 0.2f) { mountainCount = 0; }
            else if (chance < 0.25f) { mountainCount = 3; }
            else if (chance < 0.5f) { mountainCount = 2; }
            else { mountainCount = 1; }

            GameObject bigMountain = null;
            if (mountainCount > 0)
            {
                bigMountain = InstantiateM(MountainType.BM);
                if (mountainCount > 1)
                {
                    AddSMToComplex(bigMountain);
                    if (mountainCount == 3) InstantiateM(MountainType.SM, canTouchAnotherMountains: false);
                }
            }
        }

        private Vector3 GetRandomMScale(MountainType mountainType)  // M здесь Ч Mountain
        {
            return new Vector3(
                x: _mountainLocalScale.x + Random.Range(-MAX_X_OFFSET, MAX_X_OFFSET),
                y: FeetToScale(
                    mountainType == MountainType.BM ?
                    Random.Range(MAX_HEIGHT - 10000, MAX_HEIGHT + 1) :
                    Random.Range(MIN_HEIGHT, MAX_HEIGHT - 10000)
                    ),
                z: _mountainLocalScale.z
                );
        }

        private Vector3 GetRandomMPosition()  // M здесь Ч Mountain
        {
            return new Vector3(
                x: Random.Range(MIN_SPAWN_X, MAX_SPAWN_X),
                y: mountainPrefab.transform.localPosition.y
                );
        }

        private GameObject InstantiateM(MountainType mountainType, Vector3? targetLocalScale = null, bool canTouchAnotherMountains = true)  // M здесь Ч Mountain
        {
            var mountain = Instantiate(
                original: mountainPrefab,
                position: GetRandomMPosition(),
                rotation: Quaternion.identity
                );
            mountain.transform.localScale = targetLocalScale ?? GetRandomMScale(mountainType);

            do mountain.transform.localPosition = GetRandomMPosition(); while (!canTouchAnotherMountains && IsTouchingAM(mountain));  // в бесконечный цикл не уйдет Ч ширина карты не позволит.

            return mountain;
        }

        private void AddSMToComplex(GameObject bigMountain)
        {
            var targetX = bigMountain.transform.localPosition.x + (Random.value < 0.5 ? -1 : 1) * COMPLEX_X_OFFSET;  // скобки дл€ €сности

            InstantiateM(MountainType.SM, new Vector3(
                x: targetX,
                y: bigMountain.transform.localPosition.y
                ))
            .GetSR()
            .sortingOrder = bigMountain.GetSR().sortingOrder + 1;
        }

        private float FeetToScale(float feet) => ASPECT * feet;

        private bool IsTouchingAM(GameObject mountain)  // AM здесь Ч Another Mountain
        {
            List<Collider2D> result = new();
            mountain.GetCollider2D().Overlap(result);

            bool isTouching = false;
            foreach (Collider2D collider in result)
            {
                if (collider.gameObject.layer == mountain.layer)
                {
                    isTouching = true;
                    break;
                }
            }
            return isTouching;
        }
    }

    private void Start()
    {
        // TODO;
    }
}
