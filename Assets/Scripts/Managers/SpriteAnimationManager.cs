using System.Collections;
using UnityEngine;

namespace Managers
{
    public class SpriteAnimationManager
    {
        public IEnumerator SpriteSmoothTransition(SpriteRenderer spriteToDisappear, SpriteRenderer spriteToAppear, float seconds, int steps = 100) 
        {
            var curColorToDisappear = spriteToDisappear.color;
            var curColorToAppear = spriteToAppear.color;

            spriteToDisappear.enabled = true;
            curColorToDisappear.a = 1f;

            spriteToAppear.enabled = true;
            curColorToAppear.a = 0f;


            var timer = 0f;
            var step = 1f / steps;
            var stepPerSecond = step * (steps / seconds);

            var targetAlpha1 = 0f;
            var targetAlpha2 = 1f;

            var counter = 0;
            

            while (timer < seconds)
            {
                counter += 1;
                spriteToDisappear.color = curColorToDisappear;
                spriteToAppear.color = curColorToAppear;

                curColorToDisappear.a = Mathf.MoveTowards(curColorToDisappear.a, targetAlpha1, stepPerSecond * Time.deltaTime);
                curColorToAppear.a = Mathf.MoveTowards(curColorToAppear.a, targetAlpha2, stepPerSecond * Time.deltaTime);

                timer += Time.deltaTime;
                yield return null;
            }
            curColorToDisappear.a = targetAlpha1;
            curColorToAppear.a = targetAlpha2;
            spriteToDisappear.color = curColorToDisappear;
            spriteToAppear.color = curColorToAppear;

        }
    }
}