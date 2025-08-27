using UnityEngine;
using DG.Tweening;
using UnityEngine.Animations;

namespace Managers
{
    public static class SpriteAnimationManager
    {
        /// <summary>
        /// метод совершает кроссфейд между двум€ спрайтами.
        /// </summary>
        /// <param name="spriteToDisappear">спрайт, который будет исчезать</param>
        /// <param name="spriteToAppear">спрайт, который будет по€вл€тьс€</param>
        /// <param name="duration">продолжительность кроссфейда</param>
        public static void DoCrossFade(SpriteRenderer spriteToDisappear, SpriteRenderer spriteToAppear, float duration)
        {
            spriteToDisappear.DOFade(0f, duration);
            spriteToAppear.DOFade(1f, duration);
        }

        public static void Blink(SpriteRenderer sprite, float duration)
        {
            var halfDuration = duration / 2;
            sprite.DOFade(0, halfDuration).OnComplete(() => { sprite.DOFade(1, halfDuration); });
        }

        // класс будет расшир€тьс€
    }

    public static class UIAnimationManager
    {
        public enum YSlides
        {
            SlideIn,
            SlideOut
        }
        public static void YSlideScreen(GameObject UIElement, YSlides slideType, float duration, bool doOpacityChange = true)
        {

            float targetY = slideType == YSlides.SlideOut ? -UISizeManager.GetElementSizeAlongAxis(UIElement, Axis.Y) : 0;
            float targetOpacity = slideType == YSlides.SlideOut ? 0f : 1f;

            var rectTransform = UIElement.GetComponent<RectTransform>();  // если бы RectTransform отсутствовал, то ошибка бы выбросилась в GetElementSizeAlongAxis
            rectTransform.DOKill();  // останавливаем все предыдущие анимации, чтобы избежать конфликтов

            var oldAnchorMin = rectTransform.anchorMin;
            var oldAnchorMax = rectTransform.anchorMax;
            var oldPivot = rectTransform.pivot;

            // устанавливаем €кор€ и пивот дл€ корректного перемещени€
            UISizeManager.SetDownCenterAnchors(UIElement);
            UISizeManager.SetPivot(new Vector2(0.5f, 0f), UIElement);

            rectTransform.DOAnchorPosY(targetY, duration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    // возвращаем старые €кор€ и пивот
                    rectTransform.anchorMin = oldAnchorMin;
                    rectTransform.anchorMax = oldAnchorMax;
                    rectTransform.pivot = oldPivot;
                });

            if (doOpacityChange)
            {
                if (UIElement.TryGetComponent<CanvasGroup>(out var canvasGroup))
                {
                    canvasGroup.DOFade(targetOpacity, duration).SetEase(Ease.InOutSine);
                }
                else
                {
                    Debug.LogWarning("UIElement does not have a CanvasGroup component. Opacity change will not be applied.");
                }
            }
        }
    }
}