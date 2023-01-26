using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnswerButton : MonoBehaviour
{
    //Reference
    GameManager gameManager;

    //Data
    [SerializeField] TextMeshProUGUI answerText;
    private Answer answer;

    // Start is called before the first frame update
    void Start()
    {
        //Find game manager. Should be present in scene though
        gameManager = FindObjectOfType<GameManager>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Setup button according to Answer class instance passed
    public void Setup(Answer data)
    {
        answer = data;
        answerText.text = data.answerText;
    }

    public void OnClick()
    {
        gameManager.AnswerButtonClick(answer.isCorrect);
    }

}
