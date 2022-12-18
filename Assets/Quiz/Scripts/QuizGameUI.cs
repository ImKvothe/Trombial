using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuizGameUI : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] private QuizManager quizManager;               //QuizManager script
    [SerializeField] private CategoryBtnScript categoryBtnPrefab;
    [SerializeField] private GameObject scrollHolder;
    [SerializeField] private Text scoreText, timerText;
    [SerializeField] private List<Image> lifeImageList;
    [SerializeField] private GameObject gameOverPanel, mainMenu, gamePanel, gameOverGoodPanel, gameLevelCompletedPanel;
    [SerializeField] private Color correctCol, wrongCol, normalCol, defaultCol; //color of buttons
    [SerializeField] private Image questionImg;                     //image component to show image
    [SerializeField] private UnityEngine.Video.VideoPlayer questionVideo;   //to show video (optional)
    [SerializeField] private AudioSource questionAudio;             //audio source for audio clip
    [SerializeField] private Text questionInfoText;                 //text to show question
    [SerializeField] private List<Button> options;                  //options button reference
#pragma warning restore 649

    private float audioLength;          //store audio length
    private Question question;          //store current question data
    private bool answered = false;      //bool to keep track if answered or not
    private List<string> answers = new List<string>(); 
    public Text TimerText { get => timerText; }                     //getter
    public Text ScoreText { get => scoreText; }                     //getter
    public GameObject GameOverPanel { get => gameOverPanel; }                     //getter
    public GameObject GameOverGoodPanel { get => gameOverGoodPanel; }    //getter
    public GameObject GameLevelCompletedPanel { get => gameLevelCompletedPanel; }    //getter

    private void Start()
    {
        //add the listener to all the buttons
        for (int i = 0; i < options.Count; i++)
        {
            Button localBtn = options[i];
            localBtn.onClick.AddListener(() => OnClick(localBtn));
        }
        lifeImageList[1].enabled = false;
        lifeImageList[2].enabled = false;
        CreateCategoryButtons();
    }



    /// Method which populate the question on the screen
    /// <param name="question"></param>
    public void SetQuestion(Question question)
    {
        //set the question
        this.question = question;
        //check for questionType
        switch (question.questionType)
        {
            case QuestionType.TEXT:
                questionImg.transform.parent.gameObject.SetActive(false);   //deactivate image holder
                break;
            case QuestionType.IMAGE:
                questionImg.transform.parent.gameObject.SetActive(true);    //activate image holder
                questionVideo.transform.gameObject.SetActive(false);        //deactivate questionVideo
                questionImg.transform.gameObject.SetActive(true);           //activate questionImg
                questionAudio.transform.gameObject.SetActive(false);        //deactivate questionAudio

                questionImg.sprite = question.questionImage;                //set the image sprite
                break;
            case QuestionType.AUDIO:
                questionVideo.transform.parent.gameObject.SetActive(true);  //activate image holder
                questionVideo.transform.gameObject.SetActive(false);        //deactivate questionVideo
                questionImg.transform.gameObject.SetActive(false);          //deactivate questionImg
                questionAudio.transform.gameObject.SetActive(true);         //activate questionAudio
                
                audioLength = question.audioClip.length;                    //set audio clip
                StartCoroutine(PlayAudio());                                //start Coroutine
                break;
            case QuestionType.VIDEO:
                questionVideo.transform.parent.gameObject.SetActive(true);  //activate image holder
                questionVideo.transform.gameObject.SetActive(true);         //activate questionVideo
                questionImg.transform.gameObject.SetActive(false);          //deactivate questionImg
                questionAudio.transform.gameObject.SetActive(false);        //deactivate questionAudio

                questionVideo.clip = question.videoClip;                    //set video clip
                questionVideo.Play();                                       //play video
                break;
        }

        questionInfoText.text = question.questionInfo;                      //set the question text

        //suffle the list of options
        List<string> ansOptions = ShuffleList.ShuffleListItems<string>(question.options);
        //assign options to respective option buttons
        for (int i = 0; i < options.Count; i++) {
            options[i].GetComponentInChildren<Text>().text = ""; //reset
            options[i].name = "";    //reset
            options[i].image.color = defaultCol; //set color of button to normal
            options[i].enabled = false;
        }
        for (int i = 0; i < ansOptions.Count; i++)
        {
            //set the child text
            options[i].GetComponentInChildren<Text>().text = ansOptions[i];
            options[i].name = ansOptions[i];    //set the name of button
            options[i].image.color = normalCol;
            options[i].enabled = true;
        }
        answered = false;                       

    }

    public void ReduceLife(int remainingLife)
    {
        int currentLife = 2 - remainingLife;
        lifeImageList[currentLife].enabled = false;
        if (remainingLife != 0) lifeImageList[currentLife + 1].enabled = true;

    }

    /// IEnumerator to repeate the audio after some time
    IEnumerator PlayAudio()
    {
        //if questionType is audio
        if (question.questionType == QuestionType.AUDIO)
        {
            //PlayOneShot
            questionAudio.PlayOneShot(question.audioClip);
            //wait for few seconds
            yield return new WaitForSeconds(audioLength + 0.5f);
            //play again
            StartCoroutine(PlayAudio());
        }
        else //if questionType is not audio
        {
            //stop the Coroutine
            StopCoroutine(PlayAudio());
            //return null
            yield return null;
        }
    }

    /// Method assigned to the buttons
    void OnClick(Button btn)
    {
        if (quizManager.GameStatus == GameStatus.PLAYING)
        {
            //if answered is false and we picked enough answers
            answers.Add(btn.name);
            if (!answered && answers.Count == quizManager.getNumberAnswers())
            {
                //set answered true
                answered = true;
                //get the bool value
                bool val = quizManager.Answer(answers);

                
                //if its true
                if (val)
                {
                    List<string> correct = quizManager.getCorrect();
                    for (int i = 0; i < options.Count; i++) {
                        for (int j = 0; j < correct.Count; ++j) {
                        if (options[i].name == correct[j])  StartCoroutine(BlinkImg(options[i].image));  //Start blinking correct answers
                        } 
                    }
                }
                else
                {
                    //show correct answer
                    List<string> correct = quizManager.getCorrect();
                    for (int i = 0; i < options.Count; i++) {
                        for (int j = 0; j < correct.Count; ++j) {
                        if (options[i].name == correct[j]) options[i].image.color = correctCol; //set color of button to correct
                        } 
                    }
                    //else set it to wrong color
                    for (int i = 0; i < answers.Count; i++) { 
                        for (int j = 0; j < options.Count; ++j) {
                            if (answers[i] == options[j].name && options[j].image.color != correctCol) options[j].image.color = wrongCol;
                        }
                    }
                }
                answers.Clear();
            }
        }
    }


    /// Method to create Category Buttons dynamically
    void CreateCategoryButtons()
    {
        //we loop through all the available catgories in our QuizManager
        for (int i = 0; i < quizManager.QuizData.Count; i++)
        {
            //Create new CategoryBtn
            CategoryBtnScript categoryBtn = Instantiate(categoryBtnPrefab, scrollHolder.transform);
            //Set the button default values
            categoryBtn.SetButton(quizManager.QuizData[i].categoryName, quizManager.QuizData[i].questions.Count);
            int index = i;
            //Add listner to button which calls CategoryBtn method
            categoryBtn.Btn.onClick.AddListener(() => CategoryBtn(index, quizManager.QuizData[index].categoryName));
        }
    }

    //Method called by Category Button
    private void CategoryBtn(int index, string category)
    {
        quizManager.StartGame(index, category); //start the game
        bool completed =  quizManager.completed();
        mainMenu.SetActive(false);              //deactivate mainMenu
        gamePanel.SetActive(true);              //activate game panel
        if (completed) {
        quizManager.CompletedLevel();
        }
    
    }

    //this give blink effect [if needed use or dont use]
    IEnumerator BlinkImg(Image img)
    {
        for (int i = 0; i < 2; i++)
        {
            img.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            img.color = correctCol;
            yield return new WaitForSeconds(0.1f);
        }
    }

    //retry button
    public void RetryButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //exit button
    public void ExitGame() {
        Application.Quit();
    }
    //reset button
    public void ResetScore() {
        quizManager.Reset();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
