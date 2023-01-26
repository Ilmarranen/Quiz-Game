using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    //UI
    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject endPanel;
    [SerializeField] GameObject scorePanel;
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI mistakesText;
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] TextMeshProUGUI endText;
    [SerializeField] TextMeshProUGUI mainMenuHighscoreText;
    [SerializeField] TextMeshProUGUI highscoreText;
    [SerializeField] Transform answerButtonParent;


    //References
    [SerializeField] ObjectPool answerButtonObjectPool;

    //Data
    [SerializeField] Question[] questions;
    private List<Question> unansweredQuestions; //List for unused questions for randomzation it's temp unlike questions massive
    private int mistakes=0;
    private float timer=0f;
    [SerializeField] int maxMistakes=3;
    private int questionIndex=0;
    private List<GameObject> answerButtonGameObjects = new List<GameObject>();
    private bool randomQuestions=true;
    private float highscore=0f;


    // Start is called before the first frame update
    void Start()
    {
        //Starting panel state initialization - not really required, just doublecheck
        MainMenu();

        //Get and show previously saved highscore
        LoadHighscore();
        ShowHighscore();
    }

    // Update is called once per frame
    void Update()
    {
        //If game is active (using panel not to create more variables) - timer is active too
        if (gamePanel.activeSelf)
        {
            timer += Time.deltaTime;
            timerText.text = "Time:" + Mathf.Round(timer);
        } 
    }

    //Reinitialize and start the game
    public void StartGame()
    {

        //Reinitialize required fields
        mistakes = 0;
        ShowMistakes();
        timer = 0;
        questionIndex = 0;
        unansweredQuestions = questions.ToList<Question>(); //using linq for easier conversion of array to list

        //Switch to game panel for answering questions and turn on score - all other off for universality
        endPanel.SetActive(false);
        gamePanel.SetActive(true);
        scorePanel.SetActive(true);
        mainMenuPanel.SetActive(false);

        //Show first question
        ShowRequiredQuestion();
    }

    //Show end game panel with results
    public void EndGame()
    {
        //Check if won for the endscreen
        CheckWinConditions();

        //Switch to end panel and turn on score - all other off for universality
        endPanel.SetActive(true);
        gamePanel.SetActive(false);
        scorePanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void ExitGame()
    {
        //And exit the game
        Application.Quit();
    }

    public void MainMenu()
    {
        //Switch to main menu panel - all other off for universality
        endPanel.SetActive(false);
        gamePanel.SetActive(false);
        scorePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    //A wrapper to ShowQuestion function to randomize if necessary
    private void ShowRequiredQuestion()
    {
        if (randomQuestions)
        {
            //Get random index to list
            questionIndex = Random.Range(0,unansweredQuestions.Count);
        }
        else
        {
            //Increment index to next question, but only if it isnt the first question. Then index is already initialized right.
            questionIndex = questionIndex == 0 ? 0 : questionIndex + 1;
        }

        ShowQuestion(questionIndex);
    }

    //Show question and answers according to index
    private void ShowQuestion(int currentIndex)
    {
        //Reset buttons
        RemoveAnswerButtons();

        //Get question and show it
        Question question = unansweredQuestions[currentIndex];
        questionText.text = question.questionText;

        //Create buttons for all the answers (by getting from the pool) and setup the buttons themself
        for (int i = 0; i < question.answers.Length; i++)
        {
            GameObject answerButtonGameObject = answerButtonObjectPool.GetObject();
            answerButtonGameObjects.Add(answerButtonGameObject);
            //Set parent. Coordinates on creation are all wrong, so we need to disable them for grouping to do its job
            answerButtonGameObject.transform.SetParent(answerButtonParent,false);
            //This is required as not to lose order of questions
            answerButtonGameObject.transform.SetSiblingIndex(i);

            AnswerButton answerButton = answerButtonGameObject.GetComponent<AnswerButton>();
            answerButton.Setup(question.answers[i]);
        }

        //Remove used question
        unansweredQuestions.RemoveAt(currentIndex);
    }

    //Return all used buttons to the pool
    private void RemoveAnswerButtons()
    {
        while(answerButtonGameObjects.Count > 0)
        {
            //Return
            answerButtonObjectPool.ReturnObject(answerButtonGameObjects[0]);
            //Remove from active list
            answerButtonGameObjects.RemoveAt(0);
        }
    }

    //Player clicked on the answer
    public void AnswerButtonClick(bool isCorrect)
    {
        //If it is a mistake - we increment counter and show new result
        if (!isCorrect)
        {
            mistakes++;
            ShowMistakes();
        }

        //If we get maximum amount of allowed mistakes or run out of questions - end the game
        if (mistakes >= maxMistakes || unansweredQuestions.Count == 0)
        {
            EndGame();
        }
        //Or we continue the game
        else
        {
            ShowRequiredQuestion();
        }
    }

    //Check and show win or not for future use if conditions change
    private void CheckWinConditions()
    {
        if(mistakes >= maxMistakes)
        {
            endText.text = "You lose. Better luck next time";
        }
        else
        {
            endText.text = "You win. Congratulations!";
            //Try submitting score to check for highscore. We use timer as score because mistake condition is the same for all tests.
            SubmitHighscore(Mathf.Round(timer));
        }
    }

    //Update mistakes text
    private void ShowMistakes()
    {
        mistakesText.text = "Mistakes:" + mistakes + "/" + maxMistakes;
    }

    //Toggle random questions flag
    public void ToggleRandom()
    {
        randomQuestions = !randomQuestions;
    }

    //Loads highscore from PlayerPrefs
    private void LoadHighscore()
    {
        if (PlayerPrefs.HasKey("highscore"))
        {
            Debug.Log("Quiz. Has key");
            highscore = PlayerPrefs.GetFloat("highscore");
        }
    }

    //Save highscore to PlayPrefs
    private void SaveHighscore()
    {
        PlayerPrefs.SetFloat("highscore", highscore);
        Debug.Log("Quiz. " + highscore);
        PlayerPrefs.Save();
    }

    //Check score and save if lower (as we use test time as score)
    private void SubmitHighscore(float newScore)
    {
        //submit always for new game (highscore is 0)
        if(newScore < highscore || highscore == 0)
        {
            highscore = newScore;
            ShowHighscore();
            SaveHighscore();
        }
    }

    //Update current highdcore
    private void ShowHighscore()
    {
        string highscoreString = "Best time: " + highscore;
        mainMenuHighscoreText.text = highscoreString;
        highscoreText.text = highscoreString;
    }
}
