using UnityEngine;
using TMPro;
using Fusion.XR.Shared.Rig;

namespace MantaRayShoal
{
    [RequireComponent(typeof(Collider))]
    public class MantaCollectible : MonoBehaviour
    {
        [Tooltip("The canvas prefab to instantiate for interaction prompts")]
        public GameObject interactionCanvasPrefab;

        private GameObject interactionCanvas;
        private bool playerInside = false;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            SetupInteractionCanvas();
        }

        private void SetupInteractionCanvas()
        {
            // Search if we already have one
            Transform canvasT = transform.Find("InteractionCanvas");
            if (canvasT != null)
            {
                interactionCanvas = canvasT.gameObject;
                interactionCanvas.SetActive(false);
                
                // Enforce half size on existing canvases
                interactionCanvas.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
                
                var btn = interactionCanvas.GetComponentInChildren<UnityEngine.UI.Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(Interact);
                }
            }
            else if (interactionCanvasPrefab != null)
            {
                interactionCanvas = Instantiate(interactionCanvasPrefab, transform);
                interactionCanvas.name = "InteractionCanvas";
                
                // Position it float above the collectible
                interactionCanvas.transform.localPosition = new Vector3(0f, 0.4f, 0f);
                interactionCanvas.transform.localRotation = Quaternion.identity;
                
                // Enforce half size of the original 0.012f
                interactionCanvas.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
                
                // Update text to "Collect"
                var tmp = interactionCanvas.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "Collect";
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
            if (playerInside && interactionCanvas != null && interactionCanvas.activeSelf)
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
        }

        private void Interact()
        {
            Collect();
        }

        public void Collect()
        {
            // Only collect if the quest is actually in progress
            if (MantaQuestManager.Instance == null || MantaQuestManager.Instance.state != MantaQuestManager.QuestState.InProgress)
            {
                Debug.LogWarning("[MantaCollectible] Quest is not started/in-progress yet!");
                return;
            }

            Debug.Log($"[MantaCollectible] Collected: {gameObject.name}");

            // Trigger and play particle system
            ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
            if (ps == null && transform.parent != null)
            {
                // If not found in children (since script is on 'Trigger Event' child), look in the parent (Shrimp)'s children
                ps = transform.parent.GetComponentInChildren<ParticleSystem>();
            }

            if (ps != null)
            {
                // Deparent the particle system so it can play out fully even after we deactivate the parent
                ps.transform.SetParent(null);
                ps.gameObject.SetActive(true);
                ps.Play();
                Destroy(ps.gameObject, 2.0f); // Auto-destroy after playing
            }

            // Notify Quest Manager
            MantaQuestManager.Instance.CollectShrimp(this);

            // Hide canvas and disable parent (Shrimp gameobject) or this gameobject
            if (interactionCanvas != null)
            {
                interactionCanvas.SetActive(false);
            }

            // Set parent inactive as well (since parent is the Shrimp, and this is the Trigger Event)
            if (transform.parent != null && (transform.parent.name.StartsWith("Shrimp") || transform.parent.name.StartsWith("Trigger Event")))
            {
                transform.parent.gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Detect local player HardwareRig
            HardwareRig rig = other.GetComponentInParent<HardwareRig>();
            if (rig != null)
            {
                playerInside = true;
                if (interactionCanvas != null && !interactionCanvas.activeSelf)
                {
                    // Only show prompt if quest is in progress
                    if (MantaQuestManager.Instance != null && MantaQuestManager.Instance.state == MantaQuestManager.QuestState.InProgress)
                    {
                        interactionCanvas.SetActive(true);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            HardwareRig rig = other.GetComponentInParent<HardwareRig>();
            if (rig != null)
            {
                playerInside = false;
                if (interactionCanvas != null)
                {
                    interactionCanvas.SetActive(false);
                }
            }
        }
    }
}