using System;
using System.Collections.Generic;
using UnityEngine;

public struct Hill
{
    public enum HillType
    {
        BigHill,
        SmallHill
    }
    public HillType hillType;
    public Vector3 scale;

    public Hill(Vector3 scale, HillType hillType)
    {
        this.scale = scale;
        this.hillType = hillType;
    }
}

public class LandscapeGenerator : MonoBehaviour
{
    [SerializeField]
    private readonly GameObject
        map,
        mountainPrefab,
        hillPrefab,
        cloudPrefab,
        thunderCloudPrefab;
    private static int mountainCount;

    private const float
        MIN_SPAWN_X = -1.85f,
        MAX_SPAWN_X = 3f,
        Y_POS = 0.2993f;

    private static Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            x: UnityEngine.Random.Range(MIN_SPAWN_X, MAX_SPAWN_X),
            y: Y_POS
            );
    }

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
        private GameObject
            mountainPrefab,
            map;

        private const int
            MIN_HEIGHT = 20000,  // футы
            MAX_HEIGHT = 35000;  // футы

        private const float
            MIN_SCALE = 0.164f,  // y, на котором находитс€ MIN_HEIGHT
            ASPECT = MIN_SCALE / MIN_HEIGHT,

            COMPLEX_X_OFFSET = 0.7f,
            MAX_X_OFFSET = 0.06f,

            ZERO_MOUNTAIN_CHANCE = 0.2f,
            ONE_MOUNTAIN_CHANCE = 0.5f,
            THREE_MOUNTAIN_CHANCE = 0.25f;

        private Vector3 _mountainLocalScale;
        private enum MountainType
        {
            BM,  // Big Mountain
            SM   // Small Mountain
        }

        public MountainGenerator(GameObject mountainPrefab, GameObject map)
        {
            this.mountainPrefab = mountainPrefab;
            this.map = map;
            _mountainLocalScale = mountainPrefab.transform.localScale;  // кэшируем дл€ быстрого доступа
        }

        public void GenerateMountains()
        {
            var chance = UnityEngine.Random.value;

            if (chance < ZERO_MOUNTAIN_CHANCE) { mountainCount = 0; }
            else if (chance < THREE_MOUNTAIN_CHANCE) { mountainCount = 3; }
            else if (chance < ONE_MOUNTAIN_CHANCE) { mountainCount = 1; }
            else { mountainCount = 2; }

            if (mountainCount > 0)
            {
                var bigMountain = InstantiateM(MountainType.BM);
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
                x: _mountainLocalScale.x + UnityEngine.Random.Range(-MAX_X_OFFSET, MAX_X_OFFSET),
                y: FeetToScale(
                    mountainType == MountainType.BM ?
                    UnityEngine.Random.Range(MAX_HEIGHT - 10000, MAX_HEIGHT + 1) :
                    UnityEngine.Random.Range(MIN_HEIGHT, MAX_HEIGHT - 10000)
                    ),
                z: _mountainLocalScale.z
                );
        }

        private GameObject InstantiateM(MountainType mountainType, Vector3? targetLocalPosition = null, bool canTouchAnotherMountains = true)  // M здесь Ч Mountain
        {
            var temp = targetLocalPosition ?? GetRandomSpawnPosition();

            var mountain = Instantiate(
                parent: map.transform,
                original: mountainPrefab
                );
            mountain.transform.SetLocalPositionAndRotation(temp, Quaternion.identity);
            mountain.transform.localScale = GetRandomMScale(mountainType);

            while (!canTouchAnotherMountains && IsTouchingAM(mountain)) mountain.transform.localPosition = GetRandomSpawnPosition();  // в бесконечный цикл не уйдет Ч ширина карты не позволит.

            return mountain;
        }

        private void AddSMToComplex(GameObject bigMountain)
        {
            var targetX = bigMountain.transform.localPosition.x + (UnityEngine.Random.value < 0.5 ? -1 : 1) * COMPLEX_X_OFFSET;  // скобки дл€ €сности
            Debug.Log($"Target X: {targetX}");
            InstantiateM(MountainType.SM, targetLocalPosition: new Vector3(
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

    /// <summary>
    /// √енератор холмов. 
    /// 
    /// Ћќ√» ј:
    ///    Ч Ѕольшой холм - холм с шириной в диапазоне [MIN_BIG_HILL_SCALE; MAX_HILL_SCALE] (включительно  ).
    ///    - ћаленький холи - холм с шириной в диапазоне [MIN_X_SCALE; MIN_BIG_HILL_SCALE)  (невключительно).
    ///    Ч ≈сли гор нет, генерируетс€ один большой/два небольших холма (веро€тность 50/50).
    ///    Ч ≈сли гора одна, один большой.
    ///    Ч ≈сли горы две, один маленький или ноль (50/50).
    ///    Ч ≈сли горы три Ч ноль.
    /// </summary>
    public class HillGenerator
    {
        private const float
            MIN_Y_SCALE = 0.185f,
            MAX_Y_SCALE = 0.343f,

            MIN_X_SCALE = 0.15f,
            MAX_X_SCALE = 0.3f,

            MIN_BIG_HILL_SCALE = MIN_X_SCALE + ((MAX_X_SCALE - MIN_X_SCALE) / 2);

        private GameObject hillPrefab, map;
        
        public HillGenerator(GameObject hillPrefab, GameObject map)
        {
            this.hillPrefab = hillPrefab;
            this.map = map;
        }

        public void GenerateHills()
        {
            foreach (Hill hill in GetHills())
            {
                var generatedHill = Instantiate(parent: map.transform, original: hillPrefab, position: GetRandomSpawnPosition(), rotation: Quaternion.identity);
                generatedHill.transform.localScale = hill.scale;
            }
        }

        private List<Hill> GetHills()
        {
            List<Hill> result = new();
            mountainCount = GetMountainCount();

            switch (mountainCount)
            {
                case 0 when GetRandomBool(): AddSmallHill(result); AddSmallHill(result); break;
                case 0 or 1: AddBigHill(result); break;
                case 2 when GetRandomBool(): AddSmallHill(result); break;
                default: break;
            }

            return result;
        }

        private void AddSmallHill(List<Hill> list) => list.Add(new Hill(GetRandomScale(Hill.HillType.SmallHill), Hill.HillType.SmallHill));
        private void AddBigHill(List<Hill> list) => list.Add(new Hill(GetRandomScale(Hill.HillType.BigHill), Hill.HillType.BigHill));
        private bool GetRandomBool() => UnityEngine.Random.value < 0.5f;

        private int GetMountainCount() => LandscapeGenerator.mountainCount;

        private Vector3 GetRandomScale(Hill.HillType hillType)
        {
            return hillType switch
            {
                Hill.HillType.BigHill => new Vector3(
                    x: UnityEngine.Random.Range(MIN_BIG_HILL_SCALE, MAX_X_SCALE),
                    y: UnityEngine.Random.Range(MIN_Y_SCALE, MAX_Y_SCALE)
                    ),
                Hill.HillType.SmallHill => new Vector3(
                    x: UnityEngine.Random.Range(MIN_X_SCALE, MIN_BIG_HILL_SCALE - Mathf.Epsilon),
                    y: UnityEngine.Random.Range(MIN_Y_SCALE, MAX_Y_SCALE)
                    ),
                _ => throw new ArgumentException()
            };
        }
    }

    private void Start()
    {
        var mountainGenerator = new MountainGenerator(mountainPrefab, map);
        mountainGenerator.GenerateMountains();

        var hillGenerator = new HillGenerator(hillPrefab, map);
        hillGenerator.GenerateHills();
        // TODO;
    }

}