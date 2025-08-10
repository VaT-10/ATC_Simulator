using UnityEngine;
using DG.Tweening;

public class LigtningAnimation : MonoBehaviour
{
    void Start()
    {
        GetComponent<SpriteRenderer>().DOFade(0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }
}
