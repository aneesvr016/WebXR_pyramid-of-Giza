using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace AquariumProject
{
    [System.Serializable]
    public class QuizQuestion
    {
        [TextArea(2, 5)]
        public string questionText;
        public string optionA;
        public string optionB;
        public string optionC;
        public string optionD;
        [Tooltip("Correct option: A, B, C, or D")]
        public string correctOption;
    }

    public class AquariumQuizController : MonoBehaviour
    {
        public static AquariumQuizController Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("The main TextMeshProUGUI component that displays the questions and feedback")]
        public TextMeshProUGUI displayText;
        [Tooltip("The button used to start the quiz")]
        public UnityEngine.UI.Button startButton;

        [Header("Quiz Data")]
        public List<QuizQuestion> questions = new List<QuizQuestion>();

        [Header("Settings")]
        public float feedbackDuration = 2f;

        private int currentQuestionIndex = 0;
        private int correctCount = 0;
        private int wrongCount = 0;
        private bool isQuizActive = false;
        private bool isShowingFeedback = false;

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

            // Populate default questions if list is empty
            if (questions == null || questions.Count == 0)
            {
                PopulateDefaultQuestions();
            }
        }

        private void Start()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(StartQuiz);
            }
            ShowWelcomeMessage();
        }

        public void ShowWelcomeMessage()
        {
            if (displayText != null)
            {
                displayText.text = "<color=#FFCC00><size=130%>Ocean of the World Aquarium Quiz</size></color>\n\n" +
                                   "Test your knowledge of marine biology and ocean ecosystems!\n\n" +
                                   "<color=#00FFCC>Press the button below to begin the quiz!</color>";
            }
            if (startButton != null)
            {
                startButton.gameObject.SetActive(true);
            }
        }

        public void StartQuiz()
        {
            if (isQuizActive) return;

            isQuizActive = true;
            currentQuestionIndex = 0;
            correctCount = 0;
            wrongCount = 0;
            isShowingFeedback = false;

            if (startButton != null)
            {
                startButton.gameObject.SetActive(false);
            }

            ShowCurrentQuestion();
        }

        private void ShowCurrentQuestion()
        {
            if (displayText == null) return;

            if (currentQuestionIndex >= questions.Count)
            {
                ShowResults();
                return;
            }

            QuizQuestion q = questions[currentQuestionIndex];
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"<size=120%>Question {currentQuestionIndex + 1} of {questions.Count}</size>");
            sb.AppendLine();
            sb.AppendLine($"<size=110%>{q.questionText}</size>");
            sb.AppendLine();
            sb.AppendLine($"<color=#00FFCC><b>A)</b></color> {q.optionA}");
            sb.AppendLine($"<color=#00FFCC><b>B)</b></color> {q.optionB}");
            sb.AppendLine($"<color=#00FFCC><b>C)</b></color> {q.optionC}");
            sb.AppendLine($"<color=#00FFCC><b>D)</b></color> {q.optionD}");
            sb.AppendLine();
            sb.AppendLine("<Run onto the floor pad (A, B, C, or D) corresponding to your answer!");

            displayText.text = sb.ToString();
            isShowingFeedback = false;
        }

        public void SubmitAnswer(string option)
        {
            if (!isQuizActive || isShowingFeedback) return;

            QuizQuestion q = questions[currentQuestionIndex];
            bool isCorrect = option.Trim().ToUpper() == q.correctOption.Trim().ToUpper();

            if (isCorrect)
            {
                correctCount++;
                StartCoroutine(ShowFeedback(true));
            }
            else
            {
                wrongCount++;
                StartCoroutine(ShowFeedback(false, q.correctOption));
            }
        }

        private IEnumerator ShowFeedback(bool isCorrect, string correctAnswer = "")
        {
            isShowingFeedback = true;

            if (displayText != null)
            {
                if (isCorrect)
                {
                    displayText.text = "\n\n\n<color=green><size=150%><b>✔ CORRECT ANSWER!</b></size></color>\n\n<color=white>Great job! Keep going.</color>";
                }
                else
                {
                    displayText.text = $"\n\n\n<color=red><size=150%><b>✘ WRONG ANSWER</b></size></color>\n\n<color=white>The correct answer was: </color><color=yellow><b>{correctAnswer}</b></color>";
                }
            }

            yield return new WaitForSeconds(feedbackDuration);

            currentQuestionIndex++;
            ShowCurrentQuestion();
        }

        private void ShowResults()
        {
            isQuizActive = false;
            if (displayText != null)
            {
                float percentage = ((float)correctCount / questions.Count) * 100f;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("<color=#FFCC00><size=130%>Quiz Completed!</size></color>\n");
                sb.AppendLine($"Your Score: <color=green><b>{correctCount}</b></color> / {questions.Count} ({percentage:F0}%)");
                sb.AppendLine();

                if (percentage >= 80f)
                {
                    sb.AppendLine("<color=green><b>Excellent! You are a true Marine Biologist!</b></color>");
                }
                else if (percentage >= 50f)
                {
                    sb.AppendLine("<color=yellow><b>Well done! You have a solid understanding of the oceans.</b></color>");
                }
                else
                {
                    sb.AppendLine("<color=red><b>Nice try! Keep exploring the aquarium to learn more!</b></color>");
                }

                sb.AppendLine();
                sb.AppendLine("<color=#00FFCC>Press the button below to try again!</color>");

                displayText.text = sb.ToString();
            }

            if (startButton != null)
            {
                startButton.gameObject.SetActive(true);
            }
        }

        public void PopulateDefaultQuestions()
        {
            questions = new List<QuizQuestion>()
            {
                new QuizQuestion {
                    questionText = "What percentage of Earth's surface is covered by oceans?",
                    optionA = "Approximately 50%",
                    optionB = "Approximately 60%",
                    optionC = "Approximately 70%",
                    optionD = "Approximately 80%",
                    correctOption = "C"
                },
                new QuizQuestion {
                    questionText = "Which of the following is the largest ocean on Earth?",
                    optionA = "Atlantic Ocean",
                    optionB = "Indian Ocean",
                    optionC = "Pacific Ocean",
                    optionD = "Southern Ocean",
                    correctOption = "C"
                },
                new QuizQuestion {
                    questionText = "Which marine habitat is characterized by high levels of salinity?",
                    optionA = "Estuary",
                    optionB = "Coral reef",
                    optionC = "Intertidal zone",
                    optionD = "Deep sea",
                    correctOption = "B"
                },
                new QuizQuestion {
                    questionText = "Which of the following is a cold-blooded marine animal?",
                    optionA = "Penguin",
                    optionB = "Dolphin",
                    optionC = "Jellyfish",
                    optionD = "Tuna",
                    correctOption = "C"
                },
                new QuizQuestion {
                    questionText = "What is the name for the process by which marine organisms release eggs and sperm into the water for fertilization?",
                    optionA = "Oviparous reproduction",
                    optionB = "Viviparous reproduction",
                    optionC = "External fertilization",
                    optionD = "Internal fertilization",
                    correctOption = "C"
                },
                new QuizQuestion {
                    questionText = "Which of the following is not a type of marine mammal?",
                    optionA = "Sea lion",
                    optionB = "Sea turtle",
                    optionC = "Dolphin",
                    optionD = "Whale",
                    correctOption = "B"
                },
                new QuizQuestion {
                    questionText = "What is the name for the upper layer of the ocean where sunlight penetrates, and photosynthesis occurs?",
                    optionA = "Bathyal zone",
                    optionB = "Mesopelagic zone",
                    optionC = "Epipelagic zone",
                    optionD = "Abyssal zone",
                    correctOption = "C"
                },
                new QuizQuestion {
                    questionText = "Which marine organism is responsible for producing most of Earth's oxygen?",
                    optionA = "Seaweed",
                    optionB = "Phytoplankton",
                    optionC = "Coral",
                    optionD = "Seagrass",
                    correctOption = "B"
                },
                new QuizQuestion {
                    questionText = "Which of the following is not a characteristic of marine reptiles?",
                    optionA = "Cold-blooded",
                    optionB = "Lay eggs on land",
                    optionC = "Breathe air",
                    optionD = "Live exclusively in freshwater",
                    correctOption = "D"
                },
                new QuizQuestion {
                    questionText = "Which of the following is the largest fish species in the ocean?",
                    optionA = "Great white shark",
                    optionB = "Blue whale",
                    optionC = "Whale shark",
                    optionD = "Manta ray",
                    correctOption = "C"
                },
                new QuizQuestion {
                    questionText = "What is the term for the process by which marine organisms absorb carbon dioxide from seawater?",
                    optionA = "Respiration",
                    optionB = "Photosynthesis",
                    optionC = "Carbon fixation",
                    optionD = "Diffusion",
                    correctOption = "C"
                },
                new QuizQuestion {
                    questionText = "Which of the following is not a threat to coral reefs?",
                    optionA = "Overfishing",
                    optionB = "Ocean acidification",
                    optionC = "Pollution",
                    optionD = "Desertification",
                    correctOption = "D"
                },
                new QuizQuestion {
                    questionText = "What is the name for the opening on a shark's body through which it breathes and expels water?",
                    optionA = "Gills",
                    optionB = "Spiracles",
                    optionC = "Nostrils",
                    optionD = "Mouth",
                    correctOption = "B"
                },
                new QuizQuestion {
                    questionText = "Which marine mammal is known for its vocalizations and complex social behaviors?",
                    optionA = "Seal",
                    optionB = "Dolphin",
                    optionC = "Manatee",
                    optionD = "Walrus",
                    correctOption = "B"
                },
                new QuizQuestion {
                    questionText = "Which of the following is not a type of marine algae?",
                    optionA = "Kelp",
                    optionB = "Seaweed",
                    optionC = "Phytoplankton",
                    optionD = "Jellyfish",
                    correctOption = "D"
                },
                new QuizQuestion {
                    questionText = "What is the term for the area where a river meets the ocean?",
                    optionA = "Estuary",
                    optionB = "Delta",
                    optionC = "Wetland",
                    optionD = "Mangrove",
                    correctOption = "A"
                },
                new QuizQuestion {
                    questionText = "Which of the following marine animals has the ability to change its color and texture for camouflage?",
                    optionA = "Sea cucumber",
                    optionB = "Octopus",
                    optionC = "Seahorse",
                    optionD = "Clownfish",
                    correctOption = "B"
                },
                new QuizQuestion {
                    questionText = "What is the name for the process by which marine organisms convert chemical energy into heat and motion?",
                    optionA = "Photosynthesis",
                    optionB = "Respiration",
                    optionC = "Chemiosmosis",
                    optionD = "Metabolism",
                    correctOption = "B"
                },
                new QuizQuestion {
                    questionText = "Which of the following is not a type of marine pollution?",
                    optionA = "Oil spills",
                    optionB = "Plastic debris",
                    optionC = "Greenhouse gases",
                    optionD = "Noise pollution",
                    correctOption = "C"
                },
                new QuizQuestion {
                    questionText = "What is the term for the study of marine organisms and their interactions with the environment?",
                    optionA = "Oceanography",
                    optionB = "Marine biology",
                    optionC = "Aquaculture",
                    optionD = "Marine ecology",
                    correctOption = "B"
                }
            };
        }
    }
}
