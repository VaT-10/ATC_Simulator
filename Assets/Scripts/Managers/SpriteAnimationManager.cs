using UnityEngine;
using DG.Tweening;

namespace Managers
{
    public static class SpriteAnimationManager
    {
        /// <summary>
        /// метод совершает кроссфейд между двумя спрайтами.
        /// </summary>
        /// <param name="spriteToDisappear">спрайт, который будет исчезать</param>
        /// <param name="spriteToAppear">спрайт, который будет появляться</param>
        /// <param name="duration">продолжительность кроссфейда</param>
        public static void DoCrossfade(SpriteRenderer spriteToDisappear, SpriteRenderer spriteToAppear, float duration)
        {
            spriteToDisappear.DOFade(0f, duration);
            spriteToAppear.DOFade(1f, duration);
        }
    }
}