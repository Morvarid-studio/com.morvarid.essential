
namespace MorvaridEssential
{
    // UIAnim_SlideFromBottom.cs
    using UnityEngine;
    using DG.Tweening;

    [CreateAssetMenu(menuName = "UI Anim/Slide From Offset")]
    public class UIAnim_SlideFromOffset : UIAnimAction
    {
        [Header("Slide Params")] public float offsetX;
        public float offsetY;

        public float overshoot = 1.4f;

        public override Sequence Build(RectTransform target, Vector2 basePos, Vector3 baseScale, float baseRotZ,
            float delay)
        {
            // Use default parameters from ScriptableObject
            var params_obj = new SlideFromOffsetParams
            {
                duration = duration,
                ease = ease,
                alsoFade = alsoFade,
                fromAlpha = fromAlpha,
                offsetX = offsetX,
                offsetY = offsetY,
                overshoot = overshoot
            };
            return Build(target, basePos, baseScale, baseRotZ, delay, params_obj);
        }

        public override Sequence Build(RectTransform target, Vector2 basePos, Vector3 baseScale, float baseRotZ,
            float delay, ActionParameters parameters)
        {
            // Use provided parameters or fallback to SO fields
            var p = parameters as SlideFromOffsetParams ?? new SlideFromOffsetParams
            {
                duration = duration,
                ease = ease,
                alsoFade = alsoFade,
                fromAlpha = fromAlpha,
                offsetX = offsetX,
                offsetY = offsetY,
                overshoot = overshoot
            };

            target.anchoredPosition = basePos + new Vector2(p.offsetX, p.offsetY);

            CanvasGroup cg = null;
            if (p.alsoFade)
            {
                cg = GetOrAddCG(target);
                cg.alpha = p.fromAlpha;
            }

            var seq = DOTween.Sequence().SetDelay(delay);
            seq.Append(target.DOAnchorPos(basePos, p.duration).SetEase(p.ease, p.overshoot));

            if (p.alsoFade && cg != null)
                seq.Join(cg.DOFade(1f, p.duration).SetEase(p.ease));

            return seq;
        }
    }
}