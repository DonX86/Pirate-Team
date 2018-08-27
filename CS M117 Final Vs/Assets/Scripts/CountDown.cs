using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour {

    public static float timeLeft = 0.0f;
    //public GameObject myslider;
    public Slider mainSlider;

    public void updateSlider()
    {
        mainSlider.value = 0.0f;
    }

    // Use this for initialization
    void Start () {
        mainSlider.maxValue = 10;
    }
	
	// Update is called once per frame
	void Update () {
        
        //Debug.Log(timeLeft
        if(timeLeft == 0.0f)
        {
            updateSlider();
        }
        timeLeft += Time.deltaTime;
        mainSlider.value = timeLeft;
        if (timeLeft > 10.0f )
        {
            GenerateCmd();
            Boat.x_change--;
            timeLeft = 0.0f;
            buttonClient.myStreamWriter.WriteLine("s&s:0");
            mainSlider.value = 0.0f;
            Debug.Log("Timer Ended");
        }
    }

    void GenerateCmd()
    {
        Manager1.commandText.text = Manager1.grid1GameBoard.lateCommand();
    }
}
