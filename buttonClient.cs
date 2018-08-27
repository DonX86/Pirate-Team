using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class buttonClient : MonoBehaviour {

    // Use this for initialization
    NetworkStream myNetworkStream;
    Button myButton;
    Button startButton;
    Int32 port;
    TcpClient myClient;
    string ipAddress;
    Thread myReader;
    Thread myWriter;
    bool isPressed;
    bool recentlyPressed;
    StreamReader myStreamReader;
    StreamWriter myStreamWriter;

    void Start () {
        myButton = GetComponent<Button>();
        startButton = GameObject.Find("StartGame").GetComponent<Button>();
        startButton.onClick.AddListener(startGame);
        myButton.onClick.AddListener(startClient);
        port = 6789;
        isPressed = false;
        myReader = new Thread(new ThreadStart(serverReceiver));
        myWriter = new Thread(new ThreadStart(serverWriter));
        myStreamWriter = null;
        myNetworkStream = null;
        myStreamReader = null;
        recentlyPressed = false;
        ipAddress = "127.0.0.1";
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            myStreamWriter.Close();
            myStreamReader.Close();
            myNetworkStream.Close();
        }
	}

    public void startGame()
    {
        if (isPressed)
            isPressed = false;
        else
            isPressed = true;

        recentlyPressed = true;
    }

    public void startClient()
    {
        Debug.Log("Connecting...");
        myClient = new TcpClient(ipAddress, port);
        Debug.Log("Connected!");
        myNetworkStream = myClient.GetStream();
        myReader.Start();
        myWriter.Start();
    }

    public void serverWriter()
    {
        Debug.Log("Writer Started");
        myStreamWriter = new StreamWriter(myNetworkStream);
        myStreamWriter.AutoFlush = true;
        myStreamWriter.NewLine = "\n";
        while (true)
        {
            if (isPressed && recentlyPressed)
            {
                myStreamWriter.WriteLine("YO");
                isPressed = true;
                recentlyPressed = false;
            }
        }
    }


    public void serverReceiver()
    {
        myStreamReader = new StreamReader(myNetworkStream);
        string line;
        while (true)
        {
            if((line = myStreamReader.ReadLine()) != null){
                Debug.Log(line);
            }
        }
    }

}
