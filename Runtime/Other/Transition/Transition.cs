using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace MorvaridEssential.Transition
{
    public class Transition : MonoBehaviour
    {
        public static Transition Instance;
        
        [FormerlySerializedAs("clouds")] [SerializeField] private GameObject trasitionObject;
        [SerializeField] private GameObject blocker;
        
        [SerializeField] private Vector3 offset =  new Vector3(2500,0,0);
        [SerializeField] private float duration = 1;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            trasitionObject.transform.position = transform.position;
            var s = DOTween.Sequence();
            s.AppendCallback(() => { trasitionObject.gameObject.SetActive(true); });
            s.Append(trasitionObject.transform.DOMove(transform.position - offset, duration / 2).SetEase(Ease.Linear));
        }

        public void ShowTransition(Action done)
        {
            trasitionObject.transform.position = transform.position + offset;
            var s = DOTween.Sequence();
            s.AppendCallback(() =>
            {
                trasitionObject.gameObject.SetActive(true);
                blocker.gameObject.SetActive(true);
            });

            s.Append(trasitionObject.transform.DOMove(transform.position, duration / 2).SetEase(Ease.Linear));

            s.AppendCallback(() =>
            {
                blocker.gameObject.SetActive(false);
                done.Invoke();
                
            });
            s.Append(trasitionObject.transform.DOMove(transform.position - offset, duration / 2).SetEase(Ease.Linear));
        }
    }
}