using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace MorvaridEssential.Transition
{
    public class Transition : MonoBehaviour
    {
        static Transition Instance;

        private static Dictionary<int, Transition> allInstance = new Dictionary<int, Transition>();

        public static Transition GetInstance(int index = 0)
        {
            return allInstance.GetValueOrDefault(index);
        }
        

        [SerializeField] protected GameObject blocker;
        
        [SerializeField] protected float duration = 1;

        [SerializeField] protected bool activeInInit;

        [SerializeField] protected int index;

        private void Awake()
        {
            allInstance.Add(index,this);
        }

        public virtual void ShowTransition(Action done)
        {
            done.Invoke();
        }
    }
}