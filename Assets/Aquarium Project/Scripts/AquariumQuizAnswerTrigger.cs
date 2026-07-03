using UnityEngine;
using Fusion.XR.Shared.Rig;

namespace AquariumProject
{
    [RequireComponent(typeof(Collider))]
    public class AquariumQuizAnswerTrigger : MonoBehaviour
    {
        [Tooltip("The answer option this floor pad represents: A, B, C, or D")]
        public string answerOption;

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
            if (rig != null && !string.IsNullOrEmpty(answerOption))
            {
                if (AquariumQuizController.Instance != null)
                {
                    Debug.Log($"[AquariumQuizAnswerTrigger] Player stepped on option pad: {answerOption}");
                    AquariumQuizController.Instance.SubmitAnswer(answerOption);
                    
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
