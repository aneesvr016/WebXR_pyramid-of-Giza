using UnityEngine;
using Fusion.XR.Shared.Rig;
using Fusion.XR.Shared.Desktop;

namespace PyramidOfGiza
{
    [RequireComponent(typeof(Rigidbody))]
    public class CamelVehicleController : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        public WheelCollider frontLeftCollider;
        public WheelCollider frontRightCollider;
        public WheelCollider backLeftCollider;
        public WheelCollider backRightCollider;

        [Header("Wheel Visuals")]
        public Transform frontLeftVisual;
        public Transform frontRightVisual;
        public Transform backLeftVisual;
        public Transform backRightVisual;

        [Header("Camel Animator")]
        public Animator camelAnimator;

        [Header("Vehicle Settings")]
        public float maxMotorTorque = 600f;
        public float maxSteeringAngle = 25f;
        public float brakeTorque = 1500f;
        public float maxSpeed = 5f;
        public Transform centerOfMass;

        [Header("Seats & Exit Positions")]
        public Transform driversSeat;
        public Transform exitPosition;

        [Header("Dismount Button Settings")]
        public GameObject dismountCanvasPrefab;
        public Vector3 dismountCanvasLocalPosition = new Vector3(0f, 2.8f, 1.2f);
        public Vector3 dismountCanvasLocalRotation = new Vector3(15f, 180f, 0f);

        private GameObject instantiatedDismountCanvas;

        [Header("Audio Sources")]
        public AudioSource engineSound;
        public AudioSource engineStartSound;
        public AudioSource engineStopSound;
        public AudioSource honkSound;

        private Rigidbody rb;
        private HardwareRig currentDriver;
        private Transform originalDriverParent;
        private MonoBehaviour desktopController;
        private MonoBehaviour smoothLocomotion;

        public bool IsBeingRidden => currentDriver != null;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (centerOfMass != null)
            {
                rb.centerOfMass = centerOfMass.localPosition;
            }

            // Ensure we start with brakes applied if no driver
            ApplyBrakes(true);
        }

        private void Update()
        {
            if (currentDriver != null)
            {
                float forwardInput = 0f;
                float turnInput = 0f;

                // Read input based on WebXR vs Desktop vs Unity Input System
                if (WebXR.FusionBridge.WebXRFusionBridge.Active)
                {
                    Vector2 input = WebXR.FusionBridge.WebXRFusionBridge.GetMoveInput(true, false); // use left controller
                    forwardInput = input.y;
                    turnInput = input.x;
                }
                else
                {
#if ENABLE_INPUT_SYSTEM
                    var keyboard = UnityEngine.InputSystem.Keyboard.current;
                    if (keyboard != null)
                    {
                        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) forwardInput = 1f;
                        else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) forwardInput = -1f;

                        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) turnInput = 1f;
                        else if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) turnInput = -1f;

                        // Dismount on Shift key
                        if (keyboard.leftShiftKey.wasPressedThisFrame)
                        {
                            Dismount();
                            return;
                        }

                        // Honk on H key
                        if (keyboard.hKey.wasPressedThisFrame && honkSound != null)
                        {
                            honkSound.Play();
                        }
                    }
#else
                    // Fallback to standard GetAxis if input system is not enabled
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) forwardInput = 1f;
                    else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) forwardInput = -1f;

                    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) turnInput = 1f;
                    else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) turnInput = -1f;

                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        Dismount();
                        return;
                    }

                    if (Input.GetKeyDown(KeyCode.H) && honkSound != null)
                    {
                        honkSound.Play();
                    }
#endif
                }

                // Apply drive torque & steering
                Drive(forwardInput, turnInput);

                // Update wheel visuals
                UpdateWheelVisuals();

                // Keep player exactly pinned on the seat (since XR headset/rig can drift)
                if (driversSeat != null)
                {
                    currentDriver.transform.position = driversSeat.position;
                    // Retain looking controls, but match the camel's main forward direction
                    Vector3 forward = transform.forward;
                    forward.y = 0;
                    currentDriver.transform.forward = forward;
                }
            }
            else
            {
                ApplyBrakes(true);
                if (camelAnimator != null)
                {
                    camelAnimator.SetBool("Walk", false);
                }
            }
        }

        private void Drive(float verticalInput, float horizontalInput)
        {
            float currentSpeed = rb.linearVelocity.magnitude;

            if (verticalInput != 0f)
            {
                ApplyBrakes(false);
                
                // Limit speed by scaling down motor torque as speed approaches maxSpeed
                float torque = 0f;
                if (currentSpeed < maxSpeed)
                {
                    float speedFactor = Mathf.Clamp01(1f - (currentSpeed / maxSpeed));
                    torque = verticalInput * maxMotorTorque * speedFactor;
                }

                frontLeftCollider.motorTorque = torque;
                frontRightCollider.motorTorque = torque;
                backLeftCollider.motorTorque = torque;
                backRightCollider.motorTorque = torque;
            }
            else
            {
                // Decelerate smoothly
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                backLeftCollider.motorTorque = 0;
                backRightCollider.motorTorque = 0;
                ApplyBrakes(true);
            }

            // Apply steering
            float steering = horizontalInput * maxSteeringAngle;
            frontLeftCollider.steerAngle = steering;
            frontRightCollider.steerAngle = steering;

            // Update animator Walk state
            if (camelAnimator != null)
            {
                bool isWalking = Mathf.Abs(verticalInput) > 0.1f || Mathf.Abs(horizontalInput) > 0.1f || rb.linearVelocity.magnitude > 0.5f;
                camelAnimator.SetBool("Walk", isWalking);
            }
        }

        private void ApplyBrakes(bool brake)
        {
            float currentBrake = brake ? brakeTorque : 0f;
            frontLeftCollider.brakeTorque = currentBrake;
            frontRightCollider.brakeTorque = currentBrake;
            backLeftCollider.brakeTorque = currentBrake;
            backRightCollider.brakeTorque = currentBrake;
        }

        private void UpdateWheelVisuals()
        {
            UpdateWheelPose(frontLeftCollider, frontLeftVisual);
            UpdateWheelPose(frontRightCollider, frontRightVisual);
            UpdateWheelPose(backLeftCollider, backLeftVisual);
            UpdateWheelPose(backRightCollider, backRightVisual);
        }

        private void UpdateWheelPose(WheelCollider col, Transform visual)
        {
            if (col == null || visual == null) return;
            Vector3 pos;
            Quaternion rot;
            col.GetWorldPose(out pos, out rot);
            visual.position = pos;
            visual.rotation = rot;
        }

        public void Mount(HardwareRig driver)
        {
            if (currentDriver != null) return;

            currentDriver = driver;
            originalDriverParent = driver.transform.parent;

            // 1. Parent rig to seat
            driver.transform.SetParent(driversSeat);
            driver.transform.localPosition = Vector3.zero;
            driver.transform.localRotation = Quaternion.identity;

            // 2. Disable locomotion components
            desktopController = driver.GetComponentInChildren<Fusion.Addons.LocomotionValidation.LocomotionValidatedDesktopController>();
            if (desktopController == null)
            {
                // Fallback to basic desktop controller
                desktopController = driver.GetComponentInChildren<DesktopController>();
            }
            if (desktopController != null)
            {
                desktopController.enabled = false;
            }

            smoothLocomotion = driver.GetComponentInChildren<Fusion.XR.Shared.Locomotion.SmoothLocomotion>();
            if (smoothLocomotion != null)
            {
                smoothLocomotion.enabled = false;
            }

            // 3. Play audio start sequence
            if (engineStartSound != null) engineStartSound.Play();
            if (engineSound != null) engineSound.PlayDelayed(0.5f);

            // 4. Instantiate and configure the Dismount button near the camel's neck
            if (dismountCanvasPrefab != null)
            {
                instantiatedDismountCanvas = Instantiate(dismountCanvasPrefab, transform);
                instantiatedDismountCanvas.name = "DismountCanvas";
                instantiatedDismountCanvas.transform.localPosition = dismountCanvasLocalPosition;
                instantiatedDismountCanvas.transform.localRotation = Quaternion.Euler(dismountCanvasLocalRotation);
                instantiatedDismountCanvas.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);

                var tmp = instantiatedDismountCanvas.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "Dismount";
                }

                var btn = instantiatedDismountCanvas.GetComponentInChildren<UnityEngine.UI.Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(Dismount);
                }

                instantiatedDismountCanvas.SetActive(true);
            }

            Debug.Log($"[CamelVehicle] HardwareRig {driver.name} mounted camel {gameObject.name}");
        }

        public void Dismount()
        {
            if (currentDriver == null) return;

            // 1. Play audio stop sequence
            if (engineSound != null) engineSound.Stop();
            if (engineStopSound != null) engineStopSound.Play();

            // 2. Unparent rig and place at exit position
            currentDriver.transform.SetParent(originalDriverParent);
            if (exitPosition != null)
            {
                currentDriver.transform.position = exitPosition.position;
                currentDriver.transform.rotation = exitPosition.rotation;
            }
            else
            {
                // Fallback exit offset
                currentDriver.transform.position += transform.right * -1.5f + Vector3.up * 0.5f;
            }

            // 3. Re-enable locomotion components
            if (desktopController != null) desktopController.enabled = true;
            if (smoothLocomotion != null) smoothLocomotion.enabled = true;

            // 4. Destroy the Dismount button
            if (instantiatedDismountCanvas != null)
            {
                Destroy(instantiatedDismountCanvas);
                instantiatedDismountCanvas = null;
            }

            currentDriver = null;
            originalDriverParent = null;
            desktopController = null;
            smoothLocomotion = null;

            ApplyBrakes(true);
            if (camelAnimator != null)
            {
                camelAnimator.SetBool("Walk", false);
            }

            Debug.Log($"[CamelVehicle] HardwareRig dismounted camel {gameObject.name}");
        }
    }
}