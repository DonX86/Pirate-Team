using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net.Sockets;
using System.Threading;

public class Manager1 : MonoBehaviour
{

    // Use this for initialization
    public static Gameboard grid1GameBoard;
    public static Gridboard grid1GridBoard;
    public static TextMesh commandText;
    private System.Object myLock;
    Queue<string> data;
    Thread myReaderThread;
    void Start()
    {
        buttonClient.myStreamWriter.Flush();
        buttonClient.myStreamWriter.AutoFlush = true;
        buttonClient.myStreamReader.DiscardBufferedData();
        grid1GridBoard.populateGrid();
        commandText = GameObject.Find("Instructions").transform.Find("Text").GetComponent<TextMesh>();
        commandText.text = grid1GameBoard.initializeCommand();
        myLock = new System.Object();
        data = new Queue<string>();
        //myReaderThread = new Thread(serverReceiver);
        //myReaderThread.Start();

    }

    // Update is called once per frame
    /*void Update()
    {
        lock (myLock)
        {
            while (data.Count > 0)
            {
                string line = data.Dequeue();
                Debug.Log(line);
            }
        }
    }*/

    /*public void serverReceiver()
    {
        string line;
        char[] buffer = new char[1024];
        while (true)
        {

            if ((line = buttonClient.myStreamReader.ReadLine()) != null)
            {
                Debug.Log(line);
                lock (myLock)
                {
                    //data.Enqueue(line);
                }
            }


        }
    }*/

}
