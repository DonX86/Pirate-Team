using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boat : MonoBehaviour {

	float x;
    float y;
    float z;
    public static float x_change;
 

    float x_min = 247f;
    float x_max = 347f;

    // Use this for initialization
	void Start () {

        x = gameObject.transform.position.x;
        y = gameObject.transform.position.y;
        z = gameObject.transform.position.z;

        x_change = 0;
	}

    public void IncrLocation()
    {
        x_change++;
    }

    public void DecrLocation()
    {
        x_change--;
    }

    public void MoveBoat()
    {
        float newLocation = x + (x_change * 5f);

        if (newLocation < x_min)
        {
            newLocation = x_min;
        }
        
        if (newLocation > (x_max))
        {
            newLocation = x_max;
        }

        gameObject.transform.position = new Vector3(newLocation, y, z);
        if(x_change >= 10)
        {
            //load level for winning
            Debug.Log("won!");
            SceneManager.LoadScene("Victory");
        }
        else if (x_change <= -10)
        {
            // load level for losing
            Debug.Log("lost!");
            SceneManager.LoadScene("GameOver");
        }
    }
	// Update is called once per frame
	void Update () {
        MoveBoat();
	}
}
