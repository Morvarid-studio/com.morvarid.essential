using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MorvaridEssential.Transition
{
    public class FadeTransition : Transition
    {
        [SerializeField] private Image transitionObject;


        private void Start()
        {
            if (blocker != null)
                blocker.SetActive(false);


            if (transitionObject == null) return;

            if (activeInInit)
            {
                var s = DOTween.Sequence();
                s.AppendCallback(() =>
                {
                    if (transitionObject != null)
                        transitionObject.gameObject.SetActive(true);
                });
                s.Append(transitionObject.DOFade(1, 0));
                s.Append(transitionObject.DOFade(0, duration / 2).SetEase(Ease.Linear)
                    .SetLink(transitionObject.gameObject));
            }
        }

        public override void ShowTransition(Action done)
        {
            if (transitionObject == null || blocker == null)
            {
                done?.Invoke();
                return;
            }

            // Kill any existing tweens on these objects to avoid conflicts
            DOTween.Kill(transitionObject.gameObject, complete: false);

            var s = DOTween.Sequence();

            s.AppendCallback(() =>
            {
                if (transitionObject != null)
                    transitionObject.gameObject.SetActive(true);

                if (blocker != null)
                    blocker.SetActive(true);
            });

            if (transitionObject != null)
            {
                s.Append(transitionObject.DOFade(1, duration / 2)
                    .SetEase(Ease.Linear)
                    .SetLink(transitionObject.gameObject));
            }

            s.AppendCallback(() =>
            {
                if (blocker != null)
                    blocker.SetActive(false);

                done?.Invoke();
            });

            if (transitionObject != null)
            {
                s.Append(transitionObject.DOFade(0, duration / 2)
                    .SetEase(Ease.Linear)
                    .SetLink(transitionObject.gameObject));
            }
        }
    }
}