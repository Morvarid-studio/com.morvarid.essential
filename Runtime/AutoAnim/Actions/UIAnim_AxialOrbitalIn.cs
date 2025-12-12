// UIAnim_AxialOrbitalIn.cs

using UnityEngine;
using DG.Tweening;

namespace MorvaridEssential
{
    [CreateAssetMenu(menuName = "UI Anim/Axial Orbital In")]
    public class UIAnim_AxialOrbitalIn : UIAnimAction
    {
        [Header("Start Offset (from basePos)")]
        public float startOffsetX = -240f;

        public float startOffsetY = +120f;


        public float xDuration = 0.3f;

        public float yDuration = 0.3f;


        [Header("Easing per Axis")] public Ease xEase = Ease.OutElastic;
        public Ease yEase = Ease.OutElastic;

        [Header("Elastic (optional)")] public bool xElastic = false;
        public float xElasticAmplitude = 1.0f;
        public float xElasticPeriod = 0.35f;
        public bool yElastic = true;
        public float yElasticAmplitude = 1.0f;

        public float yElasticPeriod = 0.35f;

         public float startTiltDeg = -8f;
         public float tiltDurPortion = 0.60f;
        public Ease tiltEase = Ease.OutElastic;
        [SerializeField] public float rotationElasticAmplitude = 1.0f;
        [SerializeField] public float rotationElasticPeriod = 0.35f;

        public Ease scaleEase = Ease.OutElastic;
        public float scaleDuration = 1f;
        [SerializeField] public float scaleElasticAmplitude = 1.0f;
        [SerializeField] public float scaleElasticPeriod = 0.35f;

        public override Sequence Build(RectTransform target, Vector2 basePos, Vector3 baseScale, float baseRotZ,
            float delay)
        {
            var seq = DOTween.Sequence().SetAutoKill(false);
            seq.AppendInterval(delay);



            Vector2 startPos = basePos + new Vector2(startOffsetX, startOffsetY);
             target.anchoredPosition = startPos;
            target.localScale = Vector3.zero;
             target.localEulerAngles = new Vector3(target.localEulerAngles.x, target.localEulerAngles.y, baseRotZ + startTiltDeg);
            
            var tx = target.DOAnchorPosX(basePos.x, xDuration);
            if (xElastic) tx.SetEase(Ease.OutElastic, xElasticAmplitude, xElasticPeriod);
            else tx.SetEase(xEase);
            
            var ty = target.DOAnchorPosY(basePos.y, xDuration);
            if (yElastic) ty.SetEase(Ease.OutElastic, yElasticAmplitude, yElasticPeriod);
            else ty.SetEase(yEase);
            
            seq.Append(tx);
            seq.Join(ty);

            seq.Join(target.DOScale(baseScale, scaleDuration)
                            .SetEase(scaleEase, scaleElasticAmplitude,scaleElasticPeriod));


            seq.Join(target.DOLocalRotate(new Vector3(target.localEulerAngles.x, target.localEulerAngles.y, baseRotZ),
                    tiltDurPortion)
                .SetEase(tiltEase,rotationElasticAmplitude,rotationElasticPeriod)
                );

            return seq;
        }
    }
}