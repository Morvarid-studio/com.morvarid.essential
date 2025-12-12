
namespace MorvaridEssential
{
// UIAnim_Pop.cs
    using UnityEngine;
    using DG.Tweening;

    [CreateAssetMenu(menuName = "UI Anim/Pop")]
    public class UIAnim_Pop : UIAnimAction
    {
        [Header("Scale Params")]
        public float fromScale = 0.6f;
        public float overshoot = 1.4f; // شدت Back

        public override Sequence Build(RectTransform target, Vector2 basePos, Vector3 baseScale, float baseRotZ, float delay)
        {
            // Use default parameters from ScriptableObject
            var params_obj = new PopParams
            {
                duration = duration,
                ease = ease,
                alsoFade = alsoFade,
                fromAlpha = fromAlpha,
                fromScale = fromScale,
                overshoot = overshoot
            };
            return Build(target, basePos, baseScale, baseRotZ, delay, params_obj);
        }

        public override Sequence Build(RectTransform target, Vector2 basePos, Vector3 baseScale, float baseRotZ, float delay, ActionParameters parameters)
        {
            // Use provided parameters or fallback to SO fields
            var p = parameters as PopParams ?? new PopParams
            {
                duration = duration,
                ease = ease,
                alsoFade = alsoFade,
                fromAlpha = fromAlpha,
                fromScale = fromScale,
                overshoot = overshoot
            };

            // شروع
            target.localScale = baseScale * Mathf.Max(0.0001f, p.fromScale);

            CanvasGroup cg = null;
            if (p.alsoFade)
            {
                cg = GetOrAddCG(target);
                cg.alpha = p.fromAlpha;
            }
            
            //Debug.Log(delay);
            
            var seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(target.DOScale(baseScale, p.duration).SetEase(Ease.OutBack, p.overshoot));

            if (p.alsoFade && cg != null)
                seq.Join(cg.DOFade(1f, p.duration).SetEase(p.ease));

            return seq;
        }
    }

}