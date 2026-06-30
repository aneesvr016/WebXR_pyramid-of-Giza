using UnityEngine;
using TMPro;
using Fusion.XR.Shared.Rig;

namespace PyramidOfGiza
{
    [RequireComponent(typeof(Collider))]
    public class CamelRideTrigger : MonoBehaviour
    {
        [Tooltip("The canvas prefab to instantiate")]
        public GameObject interactionCanvasPrefab;

        private CamelVehicleController vehicleController;
        private GameObject interactionCanvas;
        private bool playerInside = false;
        private HardwareRig activePlayerRig;

        private void Awake()
        {
            vehicleController = GetComponentInParent<CamelVehicleController>();
            
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
                
                // Position it float above the drive interactable transform
                interactionCanvas.transform.localPosition = new Vector3(-0.48f, 0.1f, 0f);
                interactionCanvas.transform.localRotation = Quaternion.identity;
                interactionCanvas.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
                
                // Update text to "Ride Camel"
                var tmp = interactionCanvas.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "Ride Camel";
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
            if (playerInside && vehicleController != null && !vehicleController.IsBeingRidden)
            {
                if (interactionCanvas != null && !interactionCanvas.activeSelf)
                {
                    interactionCanvas.SetActive(true);
                }

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
            if (activePlayerRig != null && vehicleController != null && !vehicleController.IsBeingRidden)
            {
                // Disable canvas and mount the camel
                if (interactionCanvas != null)
                {
                    interactionCanvas.SetActive(false);
                }
                playerInside = false;

                vehicleController.Mount(activePlayerRig);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HardwareRig rig = other.GetComponentInParent<HardwareRig>();
            if (rig != null && vehicleController != null && !vehicleController.IsBeingRidden)
            {
                playerInside = true;
                activePlayerRig = rig;

                if (interactionCanvas != null)
                {
                    interactionCanvas.SetActive(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            HardwareRig rig = other.GetComponentInParent<HardwareRig>();
            if (rig != null && activePlayerRig == rig)
            {
                playerInside = false;
                activePlayerRig = null;

                if (interactionCanvas != null)
                {
                    interactionCanvas.SetActive(false);
                }
            }
        }
    }
}