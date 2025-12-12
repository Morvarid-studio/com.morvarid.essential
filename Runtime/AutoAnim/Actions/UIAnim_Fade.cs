// UIAnim_Fade.cs

using UnityEngine;
using DG.Tweening;

namespace MorvaridEssential
{
    [CreateAssetMenu(menuName = "UI Anim/Fade")]
    public class UIAnim_Fade : UIAnimAction
    {
        [Header("Fade Params")]
        [Range(0f,1f)] public float from = 0f;
        [Range(0f,1f)] public float to   = 1f;

        public override Sequence Build(RectTransform target, Vector2 basePos, Vector3 baseScale, float baseRotZ, float delay)
        {
            // Use default parameters from ScriptableObject
            var params_obj = new FadeParams
            {
                duration = duration,
                ease = ease,
                alsoFade = alsoFade,
                fromAlpha = fromAlpha,
                from = from,
                to = to
            };
            return Build(target, basePos, baseScale, baseRotZ, delay, params_obj);
        }

        public override Sequence Build(RectTransform target, Vector2 basePos, Vector3 baseScale, float baseRotZ, float delay, ActionParameters parameters)
        {
            // Use provided parameters or fallback to SO fields
            var p = parameters as FadeParams ?? new FadeParams
            {
                duration = duration,
                ease = ease,
                alsoFade = alsoFade,
                fromAlpha = fromAlpha,
                from = from,
                to = to
            };

            var seq = DOTween.Sequence().SetAutoKill(false);
            seq.AppendInterval(delay);

            // گرفتن CanvasGroup (یا اضافه کردن اگر نباشه)
            var cg = GetOrAddCG(target);

            // حالت اولیه
            cg.alpha = p.from;

            // تویین
            seq.Append(cg.DOFade(p.to, p.duration).SetEase(p.ease));

            return seq;
        }
    }
}