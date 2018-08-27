using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderAll : MonoBehaviour
{

    public Slider slider;
    public GameObject toggle;
    float finalValue;
    float sliderValue;

    float pos3 = (float)-70;
    float pos2 = (float)-20;
    float pos1 = (float)20;
    float pos0 = (float)70;

    // Use this for initialization
    void Start()
    {
        slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        finalValue = 0;
    }

    public void ValueChangeCheck()
    {
        sliderValue = slider.value;

        if (sliderValue == 0)
        {
            Debug.Log("Slider on 0!");
            toggle.transform.localRotation = Quaternion.Euler(pos0, 0, 0);
            finalValue = sliderValue;
        }

        if (sliderValue == 1)
        {
            Debug.Log("Slider on 1!");
            toggle.transform.localRotation = Quaternion.Euler(pos1, 0, 0);
            finalValue = sliderValue;
        }

        if (sliderValue == 2)
        {
            Debug.Log("Slider on 2!");
            toggle.transform.localRotation = Quaternion.Euler(pos2, 0, 0);
            finalValue = sliderValue;
        }

        if (sliderValue == 3)
        {
            Debug.Log("Slider on 3!");
            toggle.transform.localRotation = Quaternion.Euler(pos3, 0, 0);
            finalValue = sliderValue;
        }
    }

    public void OnPointerUp()
    {
        buttonClient.myStreamWriter.WriteLine("c&" + name + ":" + finalValue.ToString());
        Manager1.commandText.text = Manager1.grid1GameBoard.updateGameBoard(name, finalValue.ToString());
        
        //return finalValue;
    }


    // Update is called once per frame
    void Update()
    {

    }
}
