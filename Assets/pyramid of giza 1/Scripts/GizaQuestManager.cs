using UnityEngine;
using TMPro;
using System.Collections;

namespace PyramidOfGiza
{
    public class GizaQuestManager : MonoBehaviour
    {
        public static GizaQuestManager Instance { get; private set; }

        public enum QuestState { NotStarted, InProgress, Completed }

        [Header("Quest State")]
        public QuestState state = QuestState.NotStarted;

        [Header("Correct Glyphs")]
        public Transform khGlyphBlock;
        public Transform fGlyphBlock;
        public Transform uGlyphBlock;

        [Header("Target Door to Unlock")]
        public GameObject pyramidDoor;
        public float doorSlideDuration = 3.0f;
        public Vector3 doorOpenOffset = new Vector3(0f, -8f, 0f);

        [Header("Visual & Audio Effects")]
        public AudioClip successSound;
        public GameObject successParticlePrefab;

        [Header("Quest UI Settings")]
        public GameObject questBoardPrefab;
        public Vector3 boardPosition = new Vector3(293.8f, 41.5f, 331.1f);
        public Vector3 boardRotation = new Vector3(0f, -90f, 0f);

        private GameObject instantiatedBoard;
        private TextMeshProUGUI boardText;

        private bool khFound = false;
        private bool fFound = false;
        private bool uFound = false;

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

            SetupQuestBoard();
        }

        private void SetupQuestBoard()
        {
            if (questBoardPrefab != null)
            {
                instantiatedBoard = Instantiate(questBoardPrefab, boardPosition, Quaternion.Euler(boardRotation));
                instantiatedBoard.name = "QuestBoard";
                instantiatedBoard.transform.localScale = new Vector3(0.015f, 0.035f, 0.015f);

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
                Debug.Log("[GizaQuestManager] Quest started!");
            }
        }

        public void RegisterGlyphFlipped(string glyphName)
        {
            if (state != QuestState.InProgress) return;

            bool newFound = false;
            if (glyphName == "KhGlyph" && !khFound)
            {
                khFound = true;
                newFound = true;
                Debug.Log("[GizaQuestManager] Found 'Kh' Glyph!");
            }
            else if (glyphName == "FGlyph" && !fFound && khFound)
            {
                fFound = true;
                newFound = true;
                Debug.Log("[GizaQuestManager] Found 'F' Glyph!");
            }
            else if (glyphName == "UGlyph" && !uFound && fFound)
            {
                uFound = true;
                newFound = true;
                Debug.Log("[GizaQuestManager] Found 'U' Glyph!");
            }

            if (newFound)
            {
                UpdateBoardText();
                CheckCompletion();
            }
        }

        public bool IsGlyphCorrect(string glyphName)
        {
            return glyphName == "KhGlyph" || glyphName == "FGlyph" || glyphName == "UGlyph";
        }

        private void UpdateBoardText()
        {
            if (boardText == null) return;

            if (state == QuestState.NotStarted)
            {
                boardText.text = "<color=#FFCC00><size=150%>Ancient Puzzle</size></color>\n\nTalk to the Archaeologist nearby to begin the quest.";
            }
            else if (state == QuestState.InProgress)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("<color=#FFCC00><size=150%>Spell KHUFU</size></color>\n");
                sb.AppendLine("Find the hidden sacred letters:\n");

                // Always show KH Glyph
                if (khFound)
                {
                    sb.AppendLine("<color=green>[✔] KH GLYPH</color>");
                }
                else
                {
                    sb.AppendLine("<color=red>[ ] KH GLYPH</color>");
                }

                // Show F Glyph only after KH is found
                if (khFound)
                {
                    if (fFound)
                    {
                        sb.AppendLine("<color=green>[✔] F GLYPH</color>");
                    }
                    else
                    {
                        sb.AppendLine("<color=red>[ ] F GLYPH</color>");
                    }
                }

                // Show U Glyph only after F is found
                if (fFound)
                {
                    if (uFound)
                    {
                        sb.AppendLine("<color=green>[✔] U GLYPH</color>");
                    }
                    else
                    {
                        sb.AppendLine("<color=red>[ ] U GLYPH</color>");
                    }
                }

                boardText.text = sb.ToString();
            }
            else if (state == QuestState.Completed)
            {
                boardText.text = "<color=#00FF00><size=150%>Quest Completed!</size></color>\n\nThe door of Pharaoh Khufu's pyramid is unlocked.\n\nYou may now enter!";
            }
        }

        private void CheckCompletion()
        {
            if (khFound && fFound && uFound)
            {
                CompleteQuest();
            }
        }

        private void CompleteQuest()
        {
            state = QuestState.Completed;
            UpdateBoardText();

            Debug.Log("[GizaQuestManager] Quest Completed! Opening Pyramid Door.");

            // Play success sound
            if (successSound != null && GetComponent<AudioSource>() != null)
            {
                GetComponent<AudioSource>().PlayOneShot(successSound);
            }

            // Spawn success particles at the board or door position
            if (successParticlePrefab != null && pyramidDoor != null)
            {
                GameObject particles = Instantiate(successParticlePrefab, pyramidDoor.transform.position + Vector3.up * 2f, Quaternion.identity);
                Destroy(particles, 6.0f);
            }

            // Smoothly slide the door open
            if (pyramidDoor != null)
            {
                StartCoroutine(SlideDoorOpen());
            }
        }

        private IEnumerator SlideDoorOpen()
        {
            Vector3 startPos = pyramidDoor.transform.position;
            Vector3 targetPos = startPos + doorOpenOffset;
            float elapsed = 0f;

            // We can also disable the door collider immediately or at the end of the slide
            var col = pyramidDoor.GetComponent<Collider>();

            while (elapsed < doorSlideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / doorSlideDuration);
                pyramidDoor.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            pyramidDoor.transform.position = targetPos;
            
            if (col != null)
            {
                col.enabled = false; // Disable collider to make sure player can walk in
            }
        }
    }
}