using UnityEngine;
using TMPro;
using System.Collections;
using Fusion.XR.Shared.Rig;

namespace PyramidOfGiza
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    public class GizaNPCInteraction : MonoBehaviour
    {
        [Tooltip("The canvas prefab to instantiate for interaction prompts")]
        public GameObject interactionCanvasPrefab;

        [Tooltip("Name of the animator state to play when talking")]
        public string talkAnimationState;

        [Tooltip("Name of the animator state to play when idle")]
        public string idleAnimationState = "Idle";

        [Tooltip("Height of the interaction canvas above the NPC pivot")]
        public float canvasHeight = 2.0f;

        [Tooltip("If true, interacting with this NPC will start the ancient puzzle quest")]
        public bool startsQuestOnInteraction = false;

        private Animator animator;
        private AudioSource audioSource;
        private GameObject interactionCanvas;
        private bool playerInside = false;
        private bool isTalking = false;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            SetupInteractionCanvas();
        }

        private void SetupInteractionCanvas()
        {
            if (interactionCanvasPrefab != null)
            {
                interactionCanvas = Instantiate(interactionCanvasPrefab, transform);
                interactionCanvas.name = "InteractionCanvas";
                
                // Position it float above the NPC
                interactionCanvas.transform.localPosition = new Vector3(0f, canvasHeight, 0f);
                interactionCanvas.transform.localRotation = Quaternion.identity;
                interactionCanvas.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
                
                // Update text to "Talk"
                var tmp = interactionCanvas.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "Talk";
                }

                interactionCanvas.SetActive(false);
                
                // Wire up listener
                var btn = interactionCanvas.GetComponentInChildren<UnityEngine.UI.Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(Interact);
                }
            }
        }

        private void Update()
        {
            if (playerInside && !isTalking && interactionCanvas != null && interactionCanvas.activeSelf)
            {
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Interact();
                }
#else
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Interact();
                }
#endif
            }

            // Check if audio has finished playing
            if (isTalking && audioSource != null && !audioSource.isPlaying)
            {
                StopTalking();
            }
        }

        private void Interact()
        {
            if (!isTalking)
            {
                StartTalking();
            }
        }

        private void StartTalking()
        {
            isTalking = true;

            // Hide the interaction button once the talk starts
            if (interactionCanvas != null)
            {
                interactionCanvas.SetActive(false);
            }

            // Play voice clip
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }

            // Set animator parameter to loop talk animation properly
            SetAnimatorTalking(true);

            // Play talking animation
            if (animator != null && !string.IsNullOrEmpty(talkAnimationState))
            {
                animator.CrossFade(talkAnimationState, 0.15f);
            }

            // Start quest if flagged
            if (startsQuestOnInteraction && GizaQuestManager.Instance != null)
            {
                GizaQuestManager.Instance.StartQuest();
            }
        }

        private void StopTalking()
        {
            isTalking = false;

            if (audioSource != null)
            {
                audioSource.Stop();
            }

            // Reset animator parameter to return to Idle
            SetAnimatorTalking(false);

            // Return to idle animation
            if (animator != null && !string.IsNullOrEmpty(idleAnimationState))
            {
                animator.CrossFade(idleAnimationState, 0.15f);
            }

            // Re-show interaction prompt if player is still close
            if (playerInside && interactionCanvas != null)
            {
                interactionCanvas.SetActive(true);
            }
        }

        private void SetAnimatorTalking(bool talking)
        {
            if (animator != null)
            {
                foreach (var param in animator.parameters)
                {
                    if (param.name == "isTalking")
                    {
                        animator.SetBool("isTalking", talking);
                        break;
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Detect local player HardwareRig
            HardwareRig rig = other.GetComponentInParent<HardwareRig>();
            if (rig != null)
            {
                playerInside = true;

                if (interactionCanvas != null && !isTalking)
                {
                    interactionCanvas.SetActive(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            HardwareRig rig = other.GetComponentInParent<HardwareRig>();
            if (rig != null)
            {
                playerInside = false;
                
                if (isTalking)
                {
                    StopTalking();
                }

                if (interactionCanvas != null)
                {
                    interactionCanvas.SetActive(false);
                }
            }
        }
    }
}