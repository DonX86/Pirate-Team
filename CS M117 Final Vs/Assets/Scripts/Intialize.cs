using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intialize : MonoBehaviour {

    // string intialData = "0TB";

    private Transform myTransform;
    public Vector3[] locations;
    public GameObject[] buttons;
    public Vector3 size;

    public Vector3 rotateLoc;

    // Use this for initialization
    void Start () {
        //parse(intialData);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void parse(string inputData){
        if(inputData[0] == '0')
        {

        }
        else if(inputData[0] == '1'){

        }
    }

    void spawnButton(Vector3 location, GameObject button)
    {
        GameObject clone = Instantiate(button, location, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f)) as GameObject;
        clone.name = "Button";
        //clone.transform.localScale = size;
        //clone.transform.Rotate(rotateLoc);
    }


}
