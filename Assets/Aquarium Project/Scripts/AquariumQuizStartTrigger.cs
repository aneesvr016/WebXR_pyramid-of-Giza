using UnityEngine;
using Fusion.XR.Shared.Rig;

namespace AquariumProject
{
    [RequireComponent(typeof(Collider))]
    public class AquariumQuizStartTrigger : MonoBehaviour
    {
        private void Awake()
        {
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
            if (rig != null)
            {
                if (AquariumQuizController.Instance != null)
                {
                    Debug.Log("[AquariumQuizStartTrigger] Local player stepped on start block. Starting quiz.");
                    AquariumQuizController.Instance.StartQuiz();
                    
                    // Optional audio feedback
                    AudioSource audio = GetComponent<AudioSource>();
                    if (audio != null)
                    {
                        audio.Play();
                    }
                }
            }
        }
    }
}
