using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Switch : MonoBehaviour {

    public GameObject toggle;
    float x_change = (float)73;
    public StreamWriter buttonWriter;
    public Gameboard buttonGameboard;
    public string name;
    //float x_diff = (float)13;

    public enum States { ON, OFF };
    public static States currState = States.OFF;

    public void TaskOnStart()
    {
        //rotate up from off position
        if (currState == States.OFF)
        {
            Debug.Log("Clicked the ON button!");
            toggle.transform.Rotate(-x_change, 0, 0);
            buttonClient.myStreamWriter.WriteLine("c&" + name + ":1");
            Manager1.commandText.text = Manager1.grid1GameBoard.updateGameBoard(name, "1");

            //Manager1.commandText = Manager1.grid1GameBoard.updateGameBoard(name, "1");
            currState = States.ON;
        }
    }

    public void TaskOffStart()
    {
        //rotate down from up
        if (currState == States.ON)
        {
            Debug.Log("Clicked the OFF button!");
            buttonClient.myStreamWriter.WriteLine("c&" + name + ":0");
            Manager1.commandText.text = Manager1.grid1GameBoard.updateGameBoard(name, "0");
            
            toggle.transform.Rotate(x_change, 0, 0);
            currState = States.OFF;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}


;


   