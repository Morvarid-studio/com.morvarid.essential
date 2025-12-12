using System;
using DG.Tweening;
using UnityEngine;

namespace MorvaridEssential
{
    // Parameter Object Pattern - Base class for all action parameters
    [Serializable]
    public abstract class ActionParameters
    {
        [Header("Common")]
        public float duration = 0.45f;
        public Ease ease = Ease.OutCubic;
        public bool alsoFade = false;
        [Range(0f, 1f)] public float fromAlpha = 0f;

        // Factory method to create parameters from action
        public static ActionParameters FromAction(UIAnimAction action)
        {
            if (action == null) return null;

            return action switch
            {
                UIAnim_SlideFromOffset slideFrom => new SlideFromOffsetParams
                {
                    duration = slideFrom.duration,
                    ease = slideFrom.ease,
                    alsoFade = slideFrom.alsoFade,
                    fromAlpha = slideFrom.fromAlpha,
                    offsetX = slideFrom.offsetX,
                    offsetY = slideFrom.offsetY,
                    overshoot = slideFrom.overshoot
                },
                UIAnim_SlideToOffset slideTo => new SlideToOffsetParams
                {
                    duration = slideTo.duration,
                    ease = slideTo.ease,
                    alsoFade = slideTo.alsoFade,
                    fromAlpha = slideTo.fromAlpha,
                    offsetX = slideTo.offsetX,
                    offsetY = slideTo.offsetY,
                    overshoot = slideTo.overshoot
                },
                UIAnim_Pop pop => new PopParams
                {
                    duration = pop.duration,
                    ease = pop.ease,
                    alsoFade = pop.alsoFade,
                    fromAlpha = pop.fromAlpha,
                    fromScale = pop.fromScale,
                    overshoot = pop.overshoot
                },
                UIAnim_Fade fade => new FadeParams
                {
                    duration = fade.duration,
                    ease = fade.ease,
                    alsoFade = fade.alsoFade,
                    fromAlpha = fade.fromAlpha,
                    from = fade.from,
                    to = fade.to
                },
                // Add more action types here as needed
                _ => new CommonParams
                {
                    duration = action.duration,
                    ease = action.ease,
                    alsoFade = action.alsoFade,
                    fromAlpha = action.fromAlpha
                }
            };
        }

        // Merge override values into base parameters
        // Only override if the value is different from default (smart merge)
        public virtual void Merge(ActionParameters overrideParams)
        {
            if (overrideParams == null) return;

            // Merge common fields - only if override has meaningful values
            if (overrideParams.duration > 0) duration = overrideParams.duration;
            // For ease, always merge if provided
            ease = overrideParams.ease;
            alsoFade = overrideParams.alsoFade;
            // For fromAlpha, only merge if it's different from 0 (default)
            if (overrideParams.fromAlpha != 0f) fromAlpha = overrideParams.fromAlpha;
        }
    }

    // Common parameters for actions that don't have specific parameters
    [Serializable]
    public class CommonParams : ActionParameters { }

    // Parameters for SlideFromOffset
    [Serializable]
    public class SlideFromOffsetParams : ActionParameters
    {
        [Header("Slide Params")]
        public float offsetX;
        public float offsetY;
        public float overshoot = 1.4f;

        public override void Merge(ActionParameters overrideParams)
        {
            base.Merge(overrideParams);
            if (overrideParams is SlideFromOffsetParams slideParams)
            {
                offsetX = slideParams.offsetX;
                offsetY = slideParams.offsetY;
                overshoot = slideParams.overshoot;
            }
        }
    }

    // Parameters for SlideToOffset
    [Serializable]
    public class SlideToOffsetParams : ActionParameters
    {
        [Header("Slide Params")]
        public float offsetX;
        public float offsetY;
        public float overshoot = 1.4f;

        public override void Merge(ActionParameters overrideParams)
        {
            base.Merge(overrideParams);
            if (overrideParams is SlideToOffsetParams slideParams)
            {
                offsetX = slideParams.offsetX;
                offsetY = slideParams.offsetY;
                overshoot = slideParams.overshoot;
            }
        }
    }

    // Parameters for Pop
    [Serializable]
    public class PopParams : ActionParameters
    {
        [Header("Scale Params")]
        public float fromScale = 0.6f;
        public float overshoot = 1.4f;

        public override void Merge(ActionParameters overrideParams)
        {
            base.Merge(overrideParams);
            if (overrideParams is PopParams popParams)
            {
                fromScale = popParams.fromScale;
                overshoot = popParams.overshoot;
            }
        }
    }

    // Parameters for Fade
    [Serializable]
    public class FadeParams : ActionParameters
    {
        [Header("Fade Params")]
        [Range(0f, 1f)] public float from = 0f;
        [Range(0f, 1f)] public float to = 1f;

        public override void Merge(ActionParameters overrideParams)
        {
            base.Merge(overrideParams);
            if (overrideParams is FadeParams fadeParams)
            {
                from = fadeParams.from;
                to = fadeParams.to;
            }
        }
    }
}

