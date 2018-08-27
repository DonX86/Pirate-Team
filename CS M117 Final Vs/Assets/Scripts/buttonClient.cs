using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class buttonClient : MonoBehaviour
{
    // Use this for initialization
    enum State { CONNECT, INIT, GAME };
    State myState;
    Dictionary<string, Dictionary<string, string>> typeDictionary;
    Dictionary<string, string> pressNames;
    Dictionary<string, string> sliderNames;
    Dictionary<string, string> switchNames;
    NetworkStream myNetworkStream;
    Button myButton;
    Button startButton;
    AudioSource myAudio;
    Int32 port;
    TcpClient myClient;
    string ipAddress;
    Thread myReader;
    Thread myWriter;
    bool isPressed;
    Gameboard myGameBoard;
    GameObject temp2;
    GameObject temp;
    public static StreamReader myStreamReader;
    public static StreamWriter myStreamWriter;
    string playerNumber;
    Toggle connectedToggle;
    Toggle iReadyToggle;
    Toggle theyReadyToggle;
    int buttonNum;
    private System.Object myLock;
    Queue<string> data;
    int lineNumber;
    string gridNumber;
    string initialState;
    Gridboard myGridBoard;
    // Create Dictionary for press names to its own dictionary
    Dictionary<string, string> pressDictionary;

    // Dictionaries of commands for press options

    // Button Types for all buttons on playing field
    //List<ButtonBase> buttonList;


    // Struct for Buttons on Grid (not to be confused with Unity Button class)


    /*class GridLayout
    {
        Dictionary<string, GridButton> buttonDict;
        List<GridButton> myButtons;
        int gridNum;

        public GridLayout(int g)
        {
            buttonDict = new Dictionary<string, GridButton>();
            myButtons = new List<GridButton>();
            gridNum = g;
        }

        public void addEntry(GridButton b)
        {
            myButtons.Add(b);
            buttonDict.Add(b.getName(), b);
        }
    }*/


    // Array of all GridButtons
    List<GridButton> gridButtonList;
    string startGamePath = "prefabs/HomeScreen/StartGame";
    string networkPath = "prefabs/HomeScreen/NetworkClient";
    public static int buttonPress;


    void Start()
    {
        buttonPress = 0;
        startButton = GameObject.Find("StartGame").GetComponent<Button>();
        myButton = GameObject.Find("NetworkClient").GetComponent<Button>();
        startButton.onClick.AddListener(startGame);
        myButton.onClick.AddListener(startClient);
        connectedToggle = GameObject.Find("ConnectedToggle").GetComponent<Toggle>();
        iReadyToggle = GameObject.Find("IReady").GetComponent<Toggle>();
        theyReadyToggle = GameObject.Find("OtherReady").GetComponent<Toggle>();
        typeDictionary = new Dictionary<string, Dictionary<string, string>>();
        pressNames = new Dictionary<string, string>();
        sliderNames = new Dictionary<string, string>();
        switchNames = new Dictionary<string, string>();
        port = 6789;
        isPressed = false;
        myReader = new Thread(new ThreadStart(serverReceiver));
        myStreamWriter = null;
        myNetworkStream = null;
        myLock = new System.Object();
        myStreamReader = null;
        myGameBoard = new Gameboard();
        ipAddress = "192.168.137.216";
        myState = State.CONNECT;
        myAudio = GameObject.Find("Title").GetComponent<AudioSource>();
        gridButtonList = new List<GridButton>();
        buttonNum = 4;
        data = new Queue<string>();
        lineNumber = 0;
        playerNumber = "";
        gridNumber = "";
        //buttonList = new List<ButtonBase>();

        // TODO: FINISH UI INSTANTIATIONS
        // create a canvas
        // set reference mode to "Screen Space - Overlay"
        // set UI Scale mode to "Scale With Screen Size"

        typeDictionary.Add("f", pressNames);
        typeDictionary.Add("t", switchNames);
        typeDictionary.Add("s", switchNames);
        typeDictionary.Add("h", sliderNames);
        typeDictionary.Add("v", sliderNames);

        pressNames.Add("a", "Arghh");
        pressNames.Add("b", "Booty");
        pressNames.Add("c", "Cannon");
        pressNames.Add("d", "Leak");
        pressNames.Add("e", "Plank");
        pressNames.Add("f", "Poop Deck");

        sliderNames.Add("a", "Direction");
        sliderNames.Add("b", "Speed");
        sliderNames.Add("c", "Parrot");
        sliderNames.Add("d", "Heave Ho");
        sliderNames.Add("e", "Timbers");
        sliderNames.Add("f", "Swab Deck");

        switchNames.Add("a", "Anchor");
        switchNames.Add("b", "Cockpit");
        switchNames.Add("c", "Sails");
        switchNames.Add("d", "Cockswain");
        switchNames.Add("e", "Barnacles");
        switchNames.Add("f", "Enemy Ship");
        myGridBoard = new Gridboard();
        myAudio.Play();
        myAudio.loop = true;

       /* myGridBoard.updateGridNum("1");
        myGridBoard.addButton(new GridButton("h", "a", 0, "1",sliderNames, switchNames, pressNames));
        myGridBoard.addButton(new GridButton("s", "b", 1, "1", sliderNames, switchNames, pressNames));
        myGridBoard.addButton(new GridButton("t", "c", 2, "1", sliderNames, switchNames, pressNames));
        myGridBoard.addButton(new GridButton("f", "d", 3, "1", sliderNames, switchNames, pressNames));
        Manager1.grid1GameBoard = myGameBoard;
        Manager1.grid1GridBoard = myGridBoard;
        Manager1.myWriter = myStreamWriter;
        Manager1.myReader = myStreamReader;
        myGridBoard.switchToGrid();*/



    }

    // Update is called once per frame
    void Update()
    {

        lock (myLock)
        {
            while (data.Count > 0)
            {
                string line = data.Dequeue();
                if (myState == State.CONNECT)
                {
                    if (line == "0")
                    {
                        theyReadyToggle.transform.Find("Background").Find("Checkmark").GetComponent<Image>().enabled = false;
                    }
                    else if (line == "1")
                    {
                        theyReadyToggle.transform.Find("Background").Find("Checkmark").GetComponent<Image>().enabled = true;
                    }
                    else if (line == "Ready")
                    {
                        startButton.enabled = true;
                        startButton.transform.Find("Text").GetComponent<Text>().text = "Start Game";
                        startButton.interactable = true;
                    }
                    else if (line == "Not Ready")
                    {
                        startButton.enabled = false;
                        startButton.transform.Find("Text").GetComponent<Text>().text = "Waiting for Players...";
                        startButton.interactable = false;
                    }
                    else
                    {
                        myState = State.INIT;
                        startButton.GetComponent<Image>().color = Color.green;
                        startButton.transform.Find("Text").GetComponent<Text>().text = "Game Starting!";
                    }

                }

                else if (myState == State.INIT)
                {
                    Debug.Log(lineNumber.ToString());
                    if (lineNumber == 0) // Player Number
                    {
                        playerNumber = line;
                        Debug.Log("I am Player " + playerNumber);
                    }

                    if (lineNumber == 1) // First String to parse
                    {
                        Debug.Log(line);
                        Debug.Log("Entering parseString");
                        parseString(line);
                    }

                    if (lineNumber == 2) // Second String to parse
                    {
                        Debug.Log(line);
                        parseString(line);
                    }


                    if (lineNumber == 3)
                    {
                        Debug.Log(line);
                        if (line == "END") // Will be used to end the initialization phase
                        {
                            /*Debug.Log("Total Press Buttons on the Playing Field");
                            // TODO
                            for (int i = 0; i < buttonList.Count; i++)
                            {
                                if (buttonList[i] is pressButton)
                                {
                                    pressButton pButton = (pressButton)buttonList[i];
                                    Debug.Log(pButton.getNewCommand());
                                }
                                else if (buttonList[i] is switchButton)
                                {
                                    switchButton sButton = (switchButton)buttonList[i];
                                    Debug.Log(sButton.getNewCommand());
                                }
                                else
                                {
                                    sliderButton sliButton = (sliderButton)buttonList[i];
                                    Debug.Log(sliButton.getNewCommand());
                                }
                            }*/

                            Debug.Log("Buttons for this player");
                            for (int i = 0; i < gridButtonList.Count; i++)
                            {
                                Debug.Log(gridButtonList[i].name); // where we want to add to the gridboard
                            }

                            //myGameBoard.addGrid(myGridBoard);
                            myGameBoard.addtheirButtons();
                            Manager1.grid1GameBoard = myGameBoard;
                            Manager1.grid1GridBoard = myGridBoard;
                            myState = State.GAME;
                            myGridBoard.switchToGrid();
                        }



                    }

                    lineNumber++;

                }

                else
                {
                    string[] buttonUpdate;
                    string[] separators = { "&" };
                    buttonUpdate = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    string[] newSeparators = { ":"};
                    string[] buttonUpdate2;
                    buttonUpdate2 = buttonUpdate[1].Split(newSeparators, StringSplitOptions.RemoveEmptyEntries);
                    Debug.Log(buttonUpdate[0] + " : " + buttonUpdate[1]);
                    Debug.Log("option: " + buttonUpdate2[0]);
                    Debug.Log("command: " + buttonUpdate2[1]);

                    if (buttonUpdate[0] == "c")
                    {
                        Manager1.commandText.text = myGameBoard.updateGameBoard(buttonUpdate2[0], buttonUpdate2[1]);
                    }
                    else if (buttonUpdate[0] == "s")
                    {
                        if (buttonUpdate2[1] == "1")
                        {
                            Boat.x_change++;
                        }
                        else if(buttonUpdate2[1] == "0")
                        {
                            Boat.x_change--;
                        }
                    }

                }



            }



        }
    }

    public void startGame()
    {
        if (isPressed)
        {
            buttonPress++;
            Debug.Log(buttonClient.buttonPress.ToString());
            myStreamWriter.WriteLine("0");
            isPressed = false;
            iReadyToggle.transform.Find("Background").Find("Checkmark").GetComponent<Image>().enabled = false;
        }
        else
        {
            buttonPress++;
            Debug.Log(buttonClient.buttonPress.ToString());
            myStreamWriter.WriteLine("1");
            isPressed = true;
            iReadyToggle.transform.Find("Background").Find("Checkmark").GetComponent<Image>().enabled = true;
        }

    }

    public void startClient()
    {
        buttonPress++;
        Debug.Log(buttonClient.buttonPress.ToString());
        Debug.Log("Connecting...");
        myClient = new TcpClient(ipAddress, port);
        Debug.Log("Connected!");
        connectedToggle.transform.Find("Background").Find("Checkmark").GetComponent<Image>().enabled = true;
        myButton.enabled = false;
        myNetworkStream = myClient.GetStream();
        myStreamWriter = new StreamWriter(myNetworkStream);
        myStreamWriter.AutoFlush = true;
        myStreamWriter.NewLine = "\n";
        myReader.Start();
    }


    public void parseString(string parse)
    {
        // Example String First Parse Format: "PlayerNumber&RestofString"


        string[] firstArray;
        string[] separators = { "&" };
        firstArray = parse.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        string playerNum = firstArray[0]; // now has the number of player which will be used to distinguish between clients
        string restOfString = firstArray[1]; // gets the rest of the string that will be parsed below

        // RestofString Format: "GridNumber%GridButtons"
        string[] secondArray;
        string[] secondSeparators = { "%" };
        secondArray = restOfString.Split(secondSeparators, StringSplitOptions.RemoveEmptyEntries);
        string gridNumber = secondArray[0]; // now has the grid number
        if (playerNumber == playerNum)
        {
            myGridBoard.updateGridNum(gridNumber);
        }
        string stringToParse = secondArray[1]; // has rest of buttons and names

        // GridButtons Format: "button:name?button:name?button:name?"
        string[] buttonlist;
        char[] thirdSeparators = { '?' };
        // returns 4 strings with button information
        buttonlist = stringToParse.Split(thirdSeparators, StringSplitOptions.RemoveEmptyEntries);

        // Info Format: "button:name"
        char[] fourthSeparators = { ':' };
        string[] info;

        // Fill array of GridButtons based off string of button info

        for (int i = 0; i < buttonNum; i++)
        {
            info = buttonlist[i].Split(fourthSeparators, StringSplitOptions.RemoveEmptyEntries);
            // TODO: match string type to proper value
            switchButton tempSwitch = new switchButton(switchNames[info[1]]);
            pressButton tempPress = new pressButton(pressNames[info[1]]);
            sliderButton tempSlider = new sliderButton(sliderNames[info[1]]);
            if (playerNumber == playerNum) // only populates its own buttons
            {
                //GridButton newGridButton = new GridButton(info[0], info[1]);
                //gridButtonList.Add(newGridButton);
                myGridBoard.addButton(new GridButton(info[0], info[1], i, gridNumber, sliderNames, switchNames, pressNames));


                if (info[0] == "f")
                {
                    // buttonList.Add(new pressButton(pressNames[info[1]]));
                    myGameBoard.addmyButtons(tempPress);
                }

                else if (info[0] == "t" || info[0] == "s")
                {
                    // buttonList.Add(new switchButton(switchNames[info[1]]));
                    myGameBoard.addmyButtons(tempSwitch);
                }

                else
                {
                    // buttonList.Add(new sliderButton(sliderNames[info[1]]));
                    myGameBoard.addmyButtons(tempSlider);
                }

            }


            Debug.Log(info[0] + " " + info[1]);

            if (info[0] == "f")
            {
                // buttonList.Add(new pressButton(pressNames[info[1]]));
                myGameBoard.addButtontoGameboard(tempPress);
            }

            else if (info[0] == "t" || info[0] == "s")
            {
                // buttonList.Add(new switchButton(switchNames[info[1]]));
                myGameBoard.addButtontoGameboard(tempSwitch);
            }

            else
            {
                // buttonList.Add(new sliderButton(sliderNames[info[1]]));
                myGameBoard.addButtontoGameboard(tempSlider);
            }


        }
    }

    public void serverReceiver()
    {
        myStreamReader = new StreamReader(myNetworkStream);
        string line;
        while (true)
        {
            
            if ((line = myStreamReader.ReadLine()) != null)
            {
                Debug.Log("RECEIVED " + line);
                lock (myLock)
                {
                    data.Enqueue(line);
                }
            }


        }
    }

}




public abstract class ButtonBase
{
    public string m_name { get; set; } // access state variable
    public string m_state { get; set; } //
    //public
    //constructor
    public ButtonBase(string name) { m_name = name; m_state = "0"; }
    public abstract string getNewCommand();
    public abstract string getAssociatedCommand();


}



class switchButton : ButtonBase
{

    static Dictionary<string, string> anchorCommands = new Dictionary<string, string>
    {
        { "1", "Drop the Anchor" },
        { "0", "Raise the Anchor" }
    };

    static Dictionary<string, string> cockpitCommands = new Dictionary<string, string>
    {
        { "1", "Open the Cockpit" },
        { "0", "Close the Cockpit" }
    };

    static Dictionary<string, string> sailsCommands = new Dictionary<string, string>
    {
        { "1", "Hoist the Sails" },
        { "0", "Close the Sails" }
    };

    static Dictionary<string, string> cockswainCommands = new Dictionary<string, string>
    {
        { "1", "Engage the Cockswain" },
        { "0", "Disengage the Cockswain" }
    };

    static Dictionary<string, string> barnaclesCommands = new Dictionary<string, string>
    {
        { "1", "Engage the Barnacles" },
        { "0", "Disengage the Baranacles" }
    };

    static Dictionary<string, string> enemy_shipCommands = new Dictionary<string, string>
    {
        { "0", "Attack enemy ship" },
        { "1", "Run away from enemy ship" }
    };



    static Dictionary<string, Dictionary<string, string>> switchCommands = new Dictionary<string, Dictionary<string, string>>
    {
        { "Anchor", anchorCommands },
        { "Cockpit", cockpitCommands },
        { "Sails", sailsCommands },
        { "Cockswain", cockswainCommands },
        { "Barnacles", barnaclesCommands },
        { "Enemy Ship", enemy_shipCommands }
    };

    public switchButton(string name) : base(name) { }

    //public
    public override string getAssociatedCommand()
    {
        return switchCommands[m_name][m_state];
    }

    public override string getNewCommand()
    {
        if (m_state == "0") { return switchCommands[m_name]["1"]; }
        else { return switchCommands[m_name]["0"]; }
    }
}

class pressButton : ButtonBase
{
    Dictionary<string, string> pressCommands = new Dictionary<string, string>
    {
        { "Arghh", "Engage Argh" },
        { "Booty", "Collect the Booty" },
        { "Cannon", "Fire the Cannon" },
        { "Leak", "Plug the Leak"  },
        { "Plank", "Walk the Plank" },
        { "Poop Deck", "Stop the Poop Deck" }
    };

    //public

    public pressButton(string name) : base(name) { }


    public override string getNewCommand()
    {
        return pressCommands[m_name];
    }

    public override string getAssociatedCommand()
    {
        return pressCommands[m_name];
    }
}

class sliderButton : ButtonBase
{
    static Dictionary<string, string> directionCommands = new Dictionary<string, string>
    {
        { "0", "Set Direction to Baby Min" },
        { "1", "Set Direction to Min" },
        { "2", "Set Direction to Max" },
        { "3", "Set Direction to Super Max" }
    };

    static Dictionary<string, string> speedCommands = new Dictionary<string, string>
    {
        { "0", "Set Speed to to Baby Min" },
        { "1", "Set Speed to Min" },
        { "2", "Set Speed to Max" },
        { "3", "Set Speed to Super Max" }
    };

    static Dictionary<string, string> parrotCommands = new Dictionary<string, string>
    {
        { "0", "Set talking Parrot to Baby Min" },
        { "1", "Set talking Parrot to Min" },
        { "2", "Set talking Parrot to Max" },
        { "3", "Set talking Parrot to Super Max" }
    };

    static Dictionary<string, string> heave_hoCommands = new Dictionary<string, string>
    {
        { "0", "Set Heave Ho to Baby Min" },
        { "1", "Set Heave Ho to Min" },
        { "2", "Set Heave Ho to Max" },
        { "3", "Set Heave Ho to Super Max" }
    };

    static Dictionary<string, string> timbersCommands = new Dictionary<string, string>
    {
        { "0", "Shiver me timbers to Baby Min" },
        { "1", "Shiver me timbers to Min" },
        { "2", "Shiver me timbers to Max" },
        { "3", "Shiver me timbers to Super Max" }
    };

    static Dictionary<string, string> swab_deckCommands = new Dictionary<string, string>
    {
        { "0", "Swab the deck to Baby Min" },
        { "1", "Swab the deck to Min" },
        { "2", "Swab the deck to Max" },
        { "3", "Swab the deck to Super Max" }
    };

    static Dictionary<string, Dictionary<string, string>> sliderCommands = new Dictionary<string, Dictionary<string, string>>
    {
        { "Direction", directionCommands },
        { "Speed", speedCommands },
        { "Parrot", parrotCommands },
        { "Heave Ho", heave_hoCommands },
        { "Timbers", timbersCommands },
        { "Swab Deck", swab_deckCommands }
    };

    private System.Random rnd;

    public sliderButton(string name) : base(name)
    {
        rnd = new System.Random();
    }

    //public

    public override string getAssociatedCommand()
    {
        return sliderCommands[m_name][m_state];
    }

    public override string getNewCommand()
    {
        List<string> possibleNewStates = new List<string> { "0", "1", "2", "3" };
        possibleNewStates.Remove(m_state);
        string newState = possibleNewStates[rnd.Next(3)];
        return sliderCommands[m_name][newState];
    }



}

public class Gameboard
{
    int startPostion;
    List<ButtonBase> totalButtons;
    List<ButtonBase> myButtons;
    List<ButtonBase> theirButtons;
    Dictionary<string, ButtonBase> totalButtonDict;
    string command;
    System.Random rnd;
    Gridboard myGrid;

    public Gameboard()
    {
        totalButtonDict = new Dictionary<string, ButtonBase>();
        totalButtons = new List<ButtonBase>();
        command = "";
        myButtons = new List<ButtonBase>();
        theirButtons = new List<ButtonBase>();
        rnd = new System.Random();
        startPostion = 5;
    }

    public void addmyButtons(ButtonBase b)
    {
        myButtons.Add(b);
        Debug.Log("adding to my buttons");
        Debug.Log(b.m_name);
        Debug.Log(myButtons.Count.ToString());

    }

    public void addtheirButtons()
    {
        Debug.Log("total buttons");
        for (int i = 0; i < totalButtons.Count; i++)
        {
            Debug.Log(totalButtons[i].m_name);
        }

        Debug.Log("mybuttons");

        for (int i = 0; i < myButtons.Count; i++)
        {
            Debug.Log(myButtons[i].m_name);
        }

        for (int i = 0; i < totalButtons.Count; i++)
        {
            if (!myButtons.Contains(totalButtons[i]))
            {
                theirButtons.Add(totalButtons[i]);
            }
        }
        Debug.Log(theirButtons.Count);
    }

    public void addButtontoGameboard(ButtonBase b)
    {
        totalButtons.Add(b);
        totalButtonDict.Add(b.m_name, b);

    }

    public string initializeCommand()
    {
        int randNum = rnd.Next(10);
        if (randNum == 0)
        {
            command = generateCommand(myButtons[rnd.Next(myButtons.Count)]);
        }
        else
        {
            command = generateCommand(theirButtons[rnd.Next(theirButtons.Count)]);
        }

        return command;
    }

    public string updateGameBoard(string name, string state)
    {
        ButtonBase button = totalButtonDict[name];
        button.m_state = state;
        if (checkCommandSatisfied(button)) // generate new command for the button
        {
            Debug.Log("command satisfied!");
            Boat.x_change++;
            buttonClient.myStreamWriter.WriteLine("s&s:1");
            
            CountDown.timeLeft = 0.0f;
            int randNum = rnd.Next(6);
            if (randNum == 0)
            {
                command = generateCommand(myButtons[rnd.Next(myButtons.Count)]);
            }
            else
            {
                command = generateCommand(theirButtons[rnd.Next(theirButtons.Count)]);
            }
        }
        Debug.Log("oh");
        return command;

    }

    public string lateCommand()
    {
        int randNum = rnd.Next(6);
        string newCommand = command;
        while (newCommand == command)
        {
            if (randNum == 0)
            {
                newCommand = generateCommand(myButtons[rnd.Next(myButtons.Count)]);
            }
            else
            {
                newCommand = generateCommand(theirButtons[rnd.Next(theirButtons.Count)]);
            }
        }

        command = newCommand;

        return command;
    }

    bool checkCommandSatisfied(ButtonBase b)
    {
        // needs to cast into correct button
        if (b is sliderButton)
        {
            sliderButton temp = (sliderButton)b;
            return (command == temp.getAssociatedCommand());
        }
        else if (b is switchButton)
        {
            switchButton temp = (switchButton)b;
            return (command == temp.getAssociatedCommand());
        }
        else
        {
            pressButton temp = (pressButton)b;
            return (command == temp.getAssociatedCommand());
        }

    }



    string generateCommand(ButtonBase b)
    {
        if (b is sliderButton)
        {
            sliderButton temp = (sliderButton)b;
            return temp.getNewCommand();
        }
        else if (b is switchButton)
        {
            switchButton temp = (switchButton)b;
            return temp.getNewCommand();
        }
        else
        {
            pressButton temp = (pressButton)b;
            return temp.getNewCommand();
        }
    }

}

public class Gridboard
{
    string gridnumber;
    List<GridButton> myButtons;
    Dictionary<string, GridButton> gridButtonDict;

    public Gridboard()
    {
        myButtons = new List<GridButton>();
        gridButtonDict = new Dictionary<string, GridButton>();
    }

    public void updateGridNum(string gNum)
    {
        gridnumber = gNum;
    }

    public void addButton(GridButton but)
    {
        myButtons.Add(but);
        gridButtonDict.Add(but.name, but);
    }


    public void switchToGrid()
    {
        GameObject.Find("ConnectedToggle").transform.SetParent(null, false);
        GameObject.DontDestroyOnLoad(GameObject.Find("ConnectedToggle"));


        if (gridnumber == "0")
        {
            Debug.Log("Loading GridBoard2");
            SceneManager.LoadScene("GameBoard2");
        }
        else
        {
            Debug.Log("Loading GridBoard1");
            SceneManager.LoadScene("Gameboard1");
        }
    }

    public void populateGrid()
    {
        for (int i = 0; i < myButtons.Count; i++)
        {
            myButtons[i].instantiateButton();
        }
    }

}

public class GridButton
{
    string type;
    public string name;
    string state;
    Button myButton;
    int position;
    GameObject physButton;
    Vector3 boardPosition;
    string path;
    string script;
    string gridNumber;
    Vector3 rotation;
    Dictionary<string, string> pressDict;
    Dictionary<string, string> switchDict;
    Dictionary<string, string> sliderDict;

    //UI Button variables
    GameObject myCanvas;
    Vector3 UIposition;
    string UIpath;

    static Dictionary<string, string> pathMaps = new Dictionary<string, string> {
        {"f", "prefabs/Buttons/Flat" },
        {"h", "prefabs/Buttons/Horizontal" },
        {"v", "prefabs/Buttons/Vertical" },
        {"s", "prefabs/Buttons/Switch" },
        {"t", "prefabs/Buttons/Toggle" }

    };

    static Dictionary<string, string> UIpathMaps = new Dictionary<string, string>
    {
        {"f", "prefabs/UI/UIFlat" },
        {"h", "prefabs/UI/UIHorizontal" },
        {"v", "prefabs/UI/UIVertical" },
        {"s", "prefabs/UI/UISwitchOn" },
        {"t", "prefabs/UI/UIToggle" }

    };

    static Dictionary<string, string> UIscripts = new Dictionary<string, string>
    {
        {"f", "Flat Button" },
        {"h", "Slider All" },
        {"v", "Slider All" },
        {"s", "Switch" },
        {"t", "Toggle Button" }

    };

    public GridButton(string t, string n, int pos, string gNum, Dictionary<string, string> sliDict, Dictionary<string, string> swiDict, Dictionary<string, string> preDict)
    {
        type = t;
        state = "0";
        position = pos;
        gridNumber = gNum; //0-3
        path = pathMaps[t];
        UIpath = UIpathMaps[t];
        Debug.Log(path);
        
        Debug.Log(type);
        pressDict = preDict;
        sliderDict = sliDict;
        switchDict = swiDict;
        script = UIscripts[t];

        
        if (type == "v" || type == "h")
        {
            name = sliderDict[n];
        }
        else if (type == "f")
        {
            name = pressDict[n];
        }
        else
        {
            name = switchDict[n];
        }

        Debug.Log(name);
        if (gridNumber == "1")
        {
            switch (pos)
            {
                case 0:
                    boardPosition = new Vector3(299.2f, 167.5f, 158.9f);
                    rotation = new Vector3(0, 180f, 270f);

                    UIposition = new Vector3(0, 43.7f, 0);

                    break;
                case 1:

                    if (type == "s")
                    {
                        boardPosition = new Vector3(268.4f, 83f, 149.1f);
                        rotation = new Vector3(-96.25f, 180f, 0f);

                        UIposition = new Vector3(-198f, -423.4f, 0);
                    }
                    else if (type == "t")
                    {
                        boardPosition = new Vector3(266.36f, 81.426f, 158.9f);
                        rotation = new Vector3(90, 0, 0);

                        UIposition = new Vector3(-193f, -460f, 0);
                    }
                    else
                    {
                        boardPosition = new Vector3(271.42f, 81, 158.9f);
                        rotation = new Vector3(-161.4f, 0, 180);

                        UIposition = new Vector3(-193f, -487, 0);
                    }
                    break;
                case 2:
                    if (type == "s")
                    {
                        boardPosition = new Vector3(334.7f, 110.1f, 149.1f);
                        rotation = new Vector3(-90f, 180f, 0f);

                        UIposition = new Vector3(208.25f, -252.7f, 0);
                    }
                    else
                    {
                        boardPosition = new Vector3(338, 107.3f, 158.9f);
                        rotation = new Vector3(90, 0, 0);

                        UIposition = new Vector3(208f, -321, 0);
                    }
                    break;
                case 3:
                    boardPosition = new Vector3(336.4f, 61.5f, 158.9f);
                    rotation = new Vector3(90f, 0f, 0f);

                    UIposition = new Vector3(203f, -596.4f, 0);
                    break;
                default:
                    Debug.Log("Wrong Position");
                    break;
            }

        }

    }

    public void instantiateButton()
    {

        myCanvas = GameObject.Find("UICanvas");

        GameObject temp = UnityEngine.Object.Instantiate(Resources.Load(path), boardPosition, Quaternion.identity) as GameObject;
        temp.transform.eulerAngles = rotation;

        GameObject UItemp = UnityEngine.Object.Instantiate(Resources.Load(UIpath), UIposition, Quaternion.identity) as GameObject;
        UItemp.transform.SetParent(myCanvas.transform, false);

        // Assign physical button to proper button UI
        if (type == "s")
        {
            Switch switchScript = UItemp.transform.GetComponent<Switch>(); // this gets the switch script in switchon
            switchScript.toggle = temp.transform.GetChild(0).gameObject; // this assigns the toggle in switchon to switch part
            switchScript.name = name;

            switchScript = UItemp.transform.GetChild(1).GetComponent<Switch>(); // gets the switch script in switchoff
            switchScript.toggle = temp.transform.GetChild(0).gameObject;
            switchScript.name = name;

        } else if (type == "t") {

            ToggleButton toggleScript = UItemp.transform.GetComponent<ToggleButton>(); // gets script for toggle buttons
            toggleScript.toggle = temp.transform.GetChild(0).gameObject; // assigns toggle to UItoggle
            toggleScript.name = name;

        } else if (type == "f") {

            FlatButton toggleScript = UItemp.transform.GetComponent<FlatButton>(); // gets script for flat buttons
            toggleScript.toggle = temp.transform.GetChild(0).gameObject; // assigns toggle to UIFlat
            toggleScript.name = name;

        } else if (type == "v" || type == "h") {

            SliderAll sliderScript = UItemp.transform.GetComponent<SliderAll>(); // gets script for slider
            sliderScript.toggle = temp.transform.GetChild(0).gameObject;
            sliderScript.name = name;

        }

        // Correct position and scale of physical buttons
        if (position == 1 && type == "s")
        {
            temp.transform.localScale = new Vector3(200, 200, 200);
        }
        else if (position == 1 && type == "v")
        {
            temp.transform.localScale = new Vector3(130, 130, 130);
        }
        else if (position == 2 && type == "s")
        {
            temp.transform.localScale = new Vector3(160, 150, 150);
            //UItemp.transform.localScale = new Vector3(.8738f, .7222f, 1);
            //UItemp.transform.GetChild(1).gameObject.transform.localScale = new Vector3(.8738f, .7222f, 1);
            UItemp.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 72);
            UItemp.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 90);

            UItemp.transform.GetChild(1).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 72);
            UItemp.transform.GetChild(1).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 90);

        }

        Debug.Log(temp.name);
        temp.transform.Find("Text").GetComponent<TextMesh>().text = name;
    }


    // Create Button Function that initializes a button object for this button based on button type

    //Create onclick listener whenever the button is clicked

}

// Gridboard
// should get grid number
// get a list of buttons for this player
// initialize 
// load grid
// make sure that this scipt isn't destroyed on new scene load
// make button invisible and untouchable or move it off the screen