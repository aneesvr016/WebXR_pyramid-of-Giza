using UnityEngine;
using System.Collections;
using Fusion.XR.Shared.Rig;

namespace PyramidOfGiza
{
    [RequireComponent(typeof(Collider))]
    public class GlyphTrigger : MonoBehaviour
    {
        [Header("Animation Settings")]
        public float flipDuration = 0.4f;
        public float showDuration = 3.0f;
        public Vector3 rotationAmount = new Vector3(180f, 0f, 0f);

        private Transform targetBlock;
        private Quaternion originalRotation;
        private Quaternion flippedRotation;
        private bool isFlipped = false;
        private bool isAnimating = false;

        private void Start()
        {
            // The model to rotate is the parent object of this trigger logic object
            targetBlock = transform.parent;
            if (targetBlock != null)
            {
                originalRotation = targetBlock.localRotation;
                flippedRotation = originalRotation * Quaternion.Euler(rotationAmount);
            }

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Detect local player HardwareRig
            HardwareRig rig = other.GetComponentInParent<HardwareRig>();
            if (rig != null && !isFlipped && !isAnimating && targetBlock != null)
            {
                StartCoroutine(FlipSequence());
            }
        }

        private IEnumerator FlipSequence()
        {
            isAnimating = true;

            // 1. Smoothly flip to reveal glyph
            yield return StartCoroutine(AnimateRotation(originalRotation, flippedRotation, flipDuration));
            isFlipped = true;
            isAnimating = false;

            // Register flip with Quest Manager
            if (GizaQuestManager.Instance != null && targetBlock != null)
            {
                GizaQuestManager.Instance.RegisterGlyphFlipped(targetBlock.name);
            }

            // 2. Stay flipped for show duration
            yield return new WaitForSeconds(showDuration);

            // Wait if the player is still inside or wait for animation clearance
            while (isAnimating)
            {
                yield return null;
            }

            isAnimating = true;

            // 3. Smoothly flip back
            yield return StartCoroutine(AnimateRotation(flippedRotation, originalRotation, flipDuration));
            isFlipped = false;
            isAnimating = false;
        }

        private IEnumerator AnimateRotation(Quaternion fromRot, Quaternion toRot, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                targetBlock.localRotation = Quaternion.Slerp(fromRot, toRot, t);
                yield return null;
            }
            targetBlock.localRotation = toRot;
        }
    }
}