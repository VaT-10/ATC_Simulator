using System.Runtime.CompilerServices;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class LandscapeGenerator : MonoBehaviour
{
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
            STEP = 0.092f,       // столько scale по y нужно прибавить, чтобы прибавить 10 000 футов
            MAX_X_OFFSET = 0.7f;

        private Vector3 _mountainLocalScale;

        public MountainGenerator() => _mountainLocalScale = mountainPrefab.transform.localScale;  // кэшируем дл€ быстрого доступа

        private void GenerateMountains()
        {
            // логика: если гора одна, она больша€. если две Ч это комплекс из большой и маленькой. если три - комплекс и одна небольша€

            var mountainCount = Random.Range(MIN_MOUNTAIN_COUNT, MAX_MOUNTAIN_COUNT + 1);

            switch (mountainCount) {
                case 0: return;
                case 1:
                    var mountain = Instantiate(mountainPrefab, position: GetRandomMPosition(), Quaternion.identity);
                    mountain.transform.localScale = GetRandomBMScale();
                    break;
            }
        }

        private Vector3 GetRandomBMScale()  // BM здесь Ч Big Mountain
        {
            return new Vector3(
                x: _mountainLocalScale.x + Random.Range(-MAX_X_OFFSET, MAX_X_OFFSET + 1),
                y: FeetToScale(Random.Range(MAX_HEIGHT - 10000, MAX_HEIGHT + 1)),
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

        private float FeetToScale(float feet) => ASPECT * feet;
    }

    private void Start()
    {
        // TODO;
    }
}
