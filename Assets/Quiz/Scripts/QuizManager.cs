using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuizManager : MonoBehaviour
{
#pragma warning disable 649
    //ref to the QuizGameUI script
    [SerializeField] private QuizGameUI quizGameUI;
    //ref to the scriptableobject file
    [SerializeField] private List<QuizDataScriptable> quizDataList;
    [SerializeField] private float timeInSeconds;
#pragma warning restore 649

    private string currentCategory = "";
    private int correctAnswerCount = 0;
    //questions data
    private List<Question> questions;
    //current question data
    private Question selectedQuestion = new Question();
    private int gameScore;
    private int lifesRemaining;
    private float currentTime;
    private QuizDataScriptable dataScriptable;
    private int totalQuestions;

    private GameStatus gameStatus = GameStatus.NEXT;

    public GameStatus GameStatus { get { return gameStatus; } }

    public List<QuizDataScriptable> QuizData { get => quizDataList; }

    public void StartGame(int categoryIndex, string category)
    {
        currentCategory = category;
        correctAnswerCount = 0;
        gameScore = 0;
        lifesRemaining = 3;
        currentTime = timeInSeconds;
        //set the questions data
        questions = new List<Question>();
        dataScriptable = quizDataList[categoryIndex];
        questions.AddRange(dataScriptable.questions);
        totalQuestions = questions.Count;
        //select the question
        SelectQuestion();
        gameStatus = GameStatus.PLAYING;

    }

    /// Method used to randomly select the question form questions data
    private void SelectQuestion()
    {
        //get the random number
        int val = UnityEngine.Random.Range(0, questions.Count);
        //set the selectedQuestion
        selectedQuestion = questions[val];
        //send the question to quizGameUI
        quizGameUI.SetQuestion(selectedQuestion);
        questions.RemoveAt(val);
    }

    private void Update()
    {
        if (gameStatus == GameStatus.PLAYING)
        {
            currentTime -= Time.deltaTime;
            SetTime(currentTime);
        }
    }

    void SetTime(float value)
    {
        TimeSpan time = TimeSpan.FromSeconds(currentTime);                       //set the time value
        quizGameUI.TimerText.text = time.ToString("mm':'ss");   //convert time to Time format

        if (currentTime <= 0)
        {
            //Game Over
            GameEndBad();
        }
    }

    //get correct answer for Selectedquestion
    public List<string> getCorrect()
    {
        return selectedQuestion.correctAns;
    }

    //get number of answers
    public int getNumberAnswers()
    {
        return selectedQuestion.correctAns.Count;
    }

    public bool completed() {
        if (PlayerPrefs.GetInt(currentCategory, 0) == totalQuestions) return true;
        return false;
    }


    /// Method called to check the answer is correct or not
    public bool Answer(List<string> selectedOptions)
    {
        //set default to false
        bool correct = false;
        selectedOptions.Sort();
        List<string> aux = selectedQuestion.correctAns;
        aux.Sort();
        bool eq = true;
        for (int i = 0; i < aux.Count; ++i) {
            bool igual = false;
            for (int j = 0; j < selectedOptions.Count; ++j) {
                if (aux[i] == selectedOptions[j]) igual = true;
            }
            if (igual == false) {
                eq = false; 
                break;
            }
        }


        //if selected answer is similar to the correctAns
        if (eq)
        {
            //Yes, Ans is correct
            correctAnswerCount++;
            correct = true;
            gameScore += 50 * aux.Count;
            quizGameUI.ScoreText.text = "Puntuació:" + gameScore;
        }
        else
        {
            //No, Ans is wrong
            //Reduce Life
            lifesRemaining--;
            quizGameUI.ReduceLife(lifesRemaining);

            if (lifesRemaining == 0)
            {
                GameEndBad();
            }
        }

        if (gameStatus == GameStatus.PLAYING)
        {
            if (questions.Count > 0)
            {
                //call SelectQuestion method again after 1s
                Invoke("SelectQuestion", 0.4f);
            }
            else
            {
                PlayerPrefs.SetInt(currentCategory, correctAnswerCount);
                if (PlayerPrefs.GetInt(currentCategory, 0) == totalQuestions) GameEndGood();
                else quizGameUI.RetryButton();
            }
        }
        //return the value of correct bool
        return correct;
    }

    //retry button and save prefs
    public void retryManager() {
        PlayerPrefs.SetInt(currentCategory, correctAnswerCount);
        quizGameUI.RetryButton();
    }

    //game completed bad popup
    private void GameEndBad()
    {
        gameStatus = GameStatus.NEXT;
        quizGameUI.GameOverPanel.SetActive(true);

        //fi you want to save only the highest score then compare the current score with saved score and if more save the new score
        //eg:- if correctAnswerCount > PlayerPrefs.GetInt(currentCategory) then call below line

        //Save the score
        PlayerPrefs.SetInt(currentCategory, correctAnswerCount); //save the score for this category
    }

    //game completed good popup
    private void GameEndGood()
    {
        gameStatus = GameStatus.NEXT;
        quizGameUI.GameOverGoodPanel.SetActive(true);

    }

    //level completed popup
    public void CompletedLevel() {
       gameStatus = GameStatus.NEXT;
       quizGameUI.GameLevelCompletedPanel.SetActive(true);
    }

    //Reset button
    public void Reset() {
         for (int i = 0; i < quizDataList.Count; i++)
        {
            PlayerPrefs.SetInt(quizDataList[i].categoryName, 0);
            questions = new List<Question>();
            dataScriptable = quizDataList[i];
            questions.AddRange(dataScriptable.questions);
        }
    }
}


//Datastructure for storeing the quetions data
[System.Serializable]
public class Question
{
    public string questionInfo;         //question text
    public QuestionType questionType;   //type
    public Sprite questionImage;        //image for Image Type
    public AudioClip audioClip;         //audio for audio type
    public UnityEngine.Video.VideoClip videoClip;   //video for video type
    public List<string> options;        //options to select
    public List<string> correctAns;     //correct option
}

[System.Serializable]
public enum QuestionType
{
    TEXT,
    IMAGE,
    AUDIO,
    VIDEO
}

[SerializeField]
public enum GameStatus
{
    PLAYING,
    NEXT
}