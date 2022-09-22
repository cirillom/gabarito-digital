using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class QuizScript : MonoBehaviour
{
    public TextAsset answerKey;
    [Space]
    public TMP_Text totalTimeText;
    public TMP_Text timeInQuestionText;
    public TMP_Text currentQuestionText;
    [Space]
    public GameObject QuizCanvas;
    public GameObject ResultsCanvas;
    [Space]
    public TMP_Text resultText;
    public TMP_Text totalTimeResultText;
    public TMP_Text averageTimeResultText;
    [Space]
    public Button[] options = new Button[5];
    public Button doubtButton;

    [Space]
    public TMP_Dropdown subjectsDd;

    int currentQuestion;
    public string[] answerKeyList = new string[90];
    int score;

    public string[] myAnswersList = new string[90];
    float[] questionTimeList = new float[90];
    bool[] doubtList = new bool[90];
    int[] subjectList = new int[90];

    float totalTime;

    Dictionary<string, int> optionsDictionary = new Dictionary<string, int>();
    Dictionary<int, string> subjectsDictionary = new Dictionary<int, string>();


    void Start()
    {
        optionsDictionary.Add("A", 0);
        optionsDictionary.Add("B", 1);
        optionsDictionary.Add("C", 2);
        optionsDictionary.Add("D", 3);
        optionsDictionary.Add("E", 4);

        for (int i = 0; i < subjectsDd.options.Count; i++)
        {
            subjectsDictionary.Add(i, subjectsDd.options[i].text);
        }

        CreateSubjectTable();

        GetAnswerKey();

        for (int i = 0; i < subjectList.Length; i++)
        {
            subjectList[i] = 0;
        }

        Load();
    }

    void Update()
    {
        if (QuizCanvas.activeInHierarchy)
        {
            totalTime += Time.deltaTime;
            PlayerPrefs.SetFloat("totalTime", totalTime);

            questionTimeList[currentQuestion] += Time.deltaTime;
            questionTimeSave = String.Join(";", questionTimeList);
            PlayerPrefs.SetString("questionTime", questionTimeSave);

            currentQuestionText.text = (currentQuestion + 1).ToString();
            timeInQuestionText.text = TimeSpan.FromSeconds(questionTimeList[currentQuestion]).Minutes.ToString("00") + ":" + TimeSpan.FromSeconds(questionTimeList[currentQuestion]).Seconds.ToString("00");
            totalTimeText.text = TimeSpan.FromSeconds(totalTime).Hours.ToString("00") + ":" + TimeSpan.FromSeconds(totalTime).Minutes.ToString("00") + ":" + TimeSpan.FromSeconds(totalTime).Seconds.ToString("00");
        }

        //Debug.Log("|"+answerKeyList[89][0]+ "|");
        //Debug.Log(myAnswersList[89] + " == " + answerKeyList[89] + ":" + (myAnswersList[89] == answerKeyList[89])); 
    }

    void GetAnswerKey()
    {
        answerKeyList = answerKey.text.Split(';');
    }

    public void AnswerToCurrentQuestion(string answer)
    {
        myAnswersList[currentQuestion] = answer;
        NextQuestion();
    }

    public void NextQuestion() {
        if (currentQuestion < 89)
        {
            currentQuestion++;

            ChangeButtonColor();

            UpdateQuestionSubject();

            Save();
        }
        else
        {
            Save();

            FinishTest();
        }
    }

    public void PreviousQuestion()
    {
        if (currentQuestion > 0)
        {
            currentQuestion--;

            ChangeButtonColor();

            UpdateQuestionSubject();

            Save();
        }
    }

    void ChangeButtonColor()
    {
        foreach (Button button in options)
        {
            button.gameObject.GetComponent<Image>().color = new Color(204, 204, 204, 255);
        }

        if (doubtList[currentQuestion])
            doubtButton.gameObject.GetComponent<Image>().color = new Color(255, 0, 0, 255);
        else
            doubtButton.gameObject.GetComponent<Image>().color = new Color(204, 204, 204, 255);

        if(myAnswersList[currentQuestion] != null)
            if (optionsDictionary.ContainsKey(myAnswersList[currentQuestion]))
                options[optionsDictionary[myAnswersList[currentQuestion]]].gameObject.GetComponent<Image>().color = new Color(255, 0, 0, 255);
    }

    public void DoubtInExercise()
    {
        doubtList[currentQuestion] = !doubtList[currentQuestion];
        if (doubtList[currentQuestion])
            doubtButton.gameObject.GetComponent<Image>().color = new Color(255, 0, 0, 255);
        else
            doubtButton.gameObject.GetComponent<Image>().color = new Color(204, 204, 204, 255);

    }

    public void GetQuestionSubject()
    {
        subjectList[currentQuestion] = subjectsDd.value;
    }

    void UpdateQuestionSubject()
    {
        if(subjectList[currentQuestion] != 0)
            subjectsDd.value = subjectList[currentQuestion];
        else
            subjectsDd.value = subjectList[
                Mathf.Clamp(currentQuestion - 1, 0, 90)
                ];
    }

    public void FinishTest()
    {
        for (int i = 0; i <= 89; i++)
        {
            if(myAnswersList[i][0] == answerKeyList[i][0])
            {
                score++;
            }
        }

        resultText.text = score + "/90";
        totalTimeResultText.text = "Tempo Total: " + TimeSpan.FromSeconds(totalTime).Hours.ToString("00") + ":" + TimeSpan.FromSeconds(totalTime).Minutes.ToString("00") + ":" + TimeSpan.FromSeconds(totalTime).Seconds.ToString("00");
        float averageTime = totalTime / 90f;
        averageTimeResultText.text = "Tempo Médio: " + TimeSpan.FromSeconds(averageTime).Minutes.ToString("00") + ":" + TimeSpan.FromSeconds(averageTime).Seconds.ToString("00");

        FillQuestionsTable();

        FillSubjectsTable();

        QuizCanvas.SetActive(false);
        ResultsCanvas.SetActive(true);

        PlayerPrefs.SetInt("finished", 1);
    }

    [Space]
    TMP_Text[] questionsResultBySubjectList;
    public GameObject resultBySubjectTemplate;
    public GameObject subjectResultTableHolder;

    void CreateSubjectTable()
    {
        questionsResultBySubjectList = new TMP_Text[subjectsDictionary.Count];
        questionsResultBySubjectList[0] = resultBySubjectTemplate.GetComponentsInChildren<TMP_Text>()[1];

        //Inicia em 1 para que SEM MATERIA seja visivel
        for (int i = 1; i < subjectsDictionary.Count; i++)
        {
            GameObject newSubject = Instantiate(resultBySubjectTemplate, subjectResultTableHolder.transform);
            newSubject.name = subjectsDictionary[i];
            newSubject.GetComponent<TMP_Text>().text = subjectsDictionary[i];
            newSubject.GetComponent<TMP_Text>().fontStyle = FontStyles.UpperCase;
            newSubject.GetComponentsInChildren<TMP_Text>()[1].gameObject.name = "Results" + subjectsDictionary[i];
            questionsResultBySubjectList[i] = newSubject.GetComponentsInChildren<TMP_Text>()[1];
        }
    }
    void FillSubjectsTable()
    {
        int[] rightQuestions = new int [9];
        int[] totalQuestions = new int[9];

        for (int i = 0; i < 90; i++)
        {
            totalQuestions[subjectList[i]]++;
            if (myAnswersList[i][0] == answerKeyList[i][0])
            {
                rightQuestions[subjectList[i]]++;
            }
        }

        for (int i = 0; i < 9; i++)
        {
            questionsResultBySubjectList[i].text = rightQuestions[i].ToString() + "/" + totalQuestions[i].ToString();
        }
    }

    [Space]
    public Transform entryContainer;
    public GameObject entryTemplate;

    List<TMP_Text> questionData = new List<TMP_Text>();

    void FillQuestionsTable()
    {
        entryTemplate.SetActive(false);

        for (int i = 0; i < 90; i++)
        {
            GameObject entry = Instantiate(entryTemplate, entryContainer);
            questionData = entry.GetComponentsInChildren<TMP_Text>().ToList();

                if (myAnswersList[i][0] == answerKeyList[i][0])
                {
                    if (doubtList[i] == false)
                        entry.GetComponent<Image>().color = new Color(0, 204, 0, 255);
                    else
                        entry.GetComponent<Image>().color = new Color(204, 204, 0, 255);


                    foreach (TMP_Text questionText in questionData)
                    {
                        questionText.color = new Color32(51, 51, 51, 255);
                    }
                }
                else
                {
                    entry.GetComponent<Image>().color = new Color(150, 0, 0, 255);
                }

            questionData[0].text = subjectsDictionary[subjectList[i]][0].ToString();
            questionData[1].text = (i+1).ToString("00");
            questionData[2].text = myAnswersList[i];
            questionData[3].text = answerKeyList[i];
            questionData[4].text = TimeSpan.FromSeconds(questionTimeList[i]).Minutes.ToString("00") + ":" + TimeSpan.FromSeconds(questionTimeList[i]).Seconds.ToString("00");


            entry.SetActive(true);
            questionData.Clear();
        }
    }

    string myAnswerSave, subjectSave, doubtSave, questionTimeSave;
    void Save()
    {
        subjectList[currentQuestion] = subjectsDd.value;
        myAnswerSave = String.Join(";", myAnswersList);
        doubtSave = String.Join(";", doubtList);
        subjectSave = String.Join(";", subjectList);


        PlayerPrefs.SetString("myAnswer", myAnswerSave);
        PlayerPrefs.SetString("doubt", doubtSave);
        PlayerPrefs.SetInt("currentQuestion", currentQuestion);
        PlayerPrefs.SetString("subjectList", subjectSave);
    }

    void Load()
    {
        if (PlayerPrefs.HasKey("currentQuestion"))
        {
            totalTime = PlayerPrefs.GetFloat("totalTime");
            currentQuestion = PlayerPrefs.GetInt("currentQuestion");
            string myAnswer = PlayerPrefs.GetString("myAnswer");
            string questionTime = PlayerPrefs.GetString("questionTime");
            string doubt = PlayerPrefs.GetString("doubt");
            string subject = PlayerPrefs.GetString("subjectList");

            myAnswersList = myAnswer.Split(';');
            questionTimeList = questionTime.Split(';').Select(Convert.ToSingle).ToArray();
            subjectList = subject.Split(';').Select(int.Parse).ToArray();
            doubtList = doubt.Split(';').Select(Convert.ToBoolean).ToArray();


            if (optionsDictionary.ContainsKey(myAnswersList[currentQuestion]))
                options[optionsDictionary[myAnswersList[currentQuestion]]].Select();

            if (PlayerPrefs.GetInt("finished") == 1)
            {
                FinishTest();
            }

            ChangeButtonColor();
        }
    }
}