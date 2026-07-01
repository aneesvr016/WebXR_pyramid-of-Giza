using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace MantaRayShoal
{
    public class MantaQuestManager : MonoBehaviour
    {
        public static MantaQuestManager Instance { get; private set; }

        public enum QuestState { NotStarted, InProgress, Completed }

        [Header("Quest State")]
        public QuestState state = QuestState.NotStarted;

        [Header("Quest UI Settings")]
        public GameObject questBoardPrefab;
        public Vector3 boardPosition = new Vector3(11.0f, -0.5f, 0.0f);
        public Vector3 boardRotation = new Vector3(0f, -90f, 0f);

        [Header("Visual & Audio Effects")]
        public AudioClip successSound;
        public GameObject successParticlePrefab;

        private GameObject instantiatedBoard;
        private TextMeshProUGUI boardText;

        private int totalShrimps = 0;
        private int collectedShrimps = 0;

        private List<MantaCollectible> collectibles = new List<MantaCollectible>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            SetupQuestBoard();
            RegisterAllCollectibles();
        }

        private void SetupQuestBoard()
        {
            if (questBoardPrefab != null)
            {
                instantiatedBoard = Instantiate(questBoardPrefab, boardPosition, Quaternion.Euler(boardRotation));
                instantiatedBoard.name = "MantaQuestBoard";
                instantiatedBoard.transform.localScale = new Vector3(0.0076f, 0.0076f, 0.0076f);

                // Disable button and interactions on this status board
                var btn = instantiatedBoard.GetComponentInChildren<UnityEngine.UI.Button>();
                if (btn != null)
                {
                    Destroy(btn);
                }

                boardText = instantiatedBoard.GetComponentInChildren<TextMeshProUGUI>();
                UpdateBoardText();

                // Set active based on current state (only active if quest started/in progress or completed)
                instantiatedBoard.SetActive(state != QuestState.NotStarted);
            }
        }

        private void RegisterAllCollectibles()
        {
            collectibles.Clear();
            var found = FindObjectsByType<MantaCollectible>(FindObjectsSortMode.None);
            foreach (var col in found)
            {
                collectibles.Add(col);
            }
            totalShrimps = collectibles.Count;
            Debug.Log($"[MantaQuestManager] Registered {totalShrimps} collectibles.");
            UpdateBoardText();
        }

        public void StartQuest()
        {
            if (state == QuestState.NotStarted)
            {
                state = QuestState.InProgress;
                if (instantiatedBoard != null)
                {
                    instantiatedBoard.SetActive(true);
                }
                UpdateBoardText();
                Debug.Log("[MantaQuestManager] Quest started!");
            }
        }

        public void CollectShrimp(MantaCollectible collectible)
        {
            if (state != QuestState.InProgress) return;

            collectedShrimps++;
            Debug.Log($"[MantaQuestManager] Collected: {collectedShrimps} / {totalShrimps}");
            UpdateBoardText();

            if (collectedShrimps >= totalShrimps)
            {
                CompleteQuest();
            }
        }

        private void UpdateBoardText()
        {
            if (boardText == null) return;

            if (state == QuestState.NotStarted)
            {
                boardText.text = "<color=#FFCC00><size=120%>Manta Ray Shoal Quest</size></color>\n\nTalk to Carlos nearby to begin the quest.";
            }
            else if (state == QuestState.InProgress)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("<color=#FFCC00><size=120%>Manta Ray Shoal Quest</size></color>\n");
                sb.AppendLine("Help feed the Manta Ray by collecting glowing shrimps:\n");
                sb.AppendLine($"Collected: <color=green>{collectedShrimps}</color> / <color=yellow>{totalShrimps}</color> Shrimps");
                
                if (collectedShrimps == totalShrimps)
                {
                    sb.AppendLine("\n<color=green>[✔] All Shrimps collected!</color>");
                }
                else
                {
                    sb.AppendLine("\n<color=red>Keep searching the shoal...</color>");
                }

                boardText.text = sb.ToString();
            }
            else if (state == QuestState.Completed)
            {
                boardText.text = "<color=#00FF00><size=120%>Quest Completed!</size></color>\n\nYou have collected all the shrimps and fed the Manta Ray!\n\nThank you for exploring!";
            }
        }

        private void CompleteQuest()
        {
            state = QuestState.Completed;
            UpdateBoardText();

            Debug.Log("[MantaQuestManager] Manta Shoal Quest Completed!");

            // Play success sound
            if (successSound != null)
            {
                AudioSource.PlayClipAtPoint(successSound, boardPosition);
            }
            else
            {
                var audioSource = GetComponent<AudioSource>();
                if (audioSource != null && successSound != null)
                {
                    audioSource.PlayOneShot(successSound);
                }
            }

            // Spawn success particles
            if (successParticlePrefab != null)
            {
                GameObject particles = Instantiate(successParticlePrefab, boardPosition + Vector3.up * 1f, Quaternion.identity);
                Destroy(particles, 6.0f);
            }
        }
    }
}