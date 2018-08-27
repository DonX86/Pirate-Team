from twisted.internet.protocol import Protocol, Factory
from twisted.internet import reactor
import string, sys
from collections import defaultdict
from twisted.protocols.basic import LineReceiver
from twisted.internet.error import CannotListenError
from twisted.internet.interfaces import IReactorTCP
from random import randint

# TCP Connection Information
portNumber = 6789
ipAddress = ''
numQueues = 2

# Initialization Variables
gridAmount = 2
switchAmount = 2
sliderAmount = 2
switchPressAmount = 2
pressAmount = 1 
switchVSliderAmount = 2
numOptions = 3

# Array Raw and Combinations
switchOptions = ['t', 's']
sliderOptions = ['h', 'v']
horizontalSlider = 'h'
verticalSlider = 'v'
pressOptions = 'f'
totalOptions = [switchOptions, sliderOptions, pressOptions]
switchVSlider = [switchOptions, verticalSlider]
switchPress = [switchOptions, pressOptions]

# Available Names
switchNames = ['a', 'b', 'c', 'd', 'e', 'f']
sliderNames = ['a', 'b', 'c', 'd', 'e', 'f']
pressNames = ['a', 'b', 'c', 'd', 'e', 'f']

# Name Combinations
optionNames = [switchNames, sliderNames, pressNames]
switchVSliderNames = [switchNames, sliderNames]
switchPressNames = [switchNames, pressNames]

"""
### Grid Selections ###

# Initialization Notation #
# "type:name?type:name..."


# Two Grid Options

# Grid 0
# Button 0 - Switch or Press
# Button 1 - Horizontal Slider Only
# Button 2 - Anything
# Button 3 - Switch or Vertical Slider

# Grid 1
# Button 0 - Horizontal Slider Only
# Button 1 - Vertical Slider or Switch
# Button 2 - Switch Only
# Button 3 - Press Only

# Switch Names


### Button Options ###

# Switch # - 0
# 0 - Toggle - 't'
# 1 - Switch - 's'

# Slider # - 1
# 0 - Horizontal - 'h'
# 1 - Vertical - 'v'

# Press # - 2 
# 0 - Flat - 'f'
"""


class Server(LineReceiver):

    def __init__(self, factory):

        self.factory = factory
        self.factory.players.append(self)
        self.delimiter = '\n'
        self.state = 'startGame'
        self.readyStart = 0
        self.numPlayer = -1

    def randomNumber(self, length):
        return randint(0, length - 1)

    def randomNameChar(self, names):
        nameChar = names[self.randomNumber(len(names))]
        print('char: ' + nameChar)
        names.remove(nameChar)
        print("list is now: " + str(names))
        return nameChar

    def randMultOption(self, amount, list, names):
           
        randNum = self.randomNumber(amount)
        buttonOption = list[randNum]
        buttonChar = buttonOption[self.randomNumber(len(buttonOption))]
        nameChar = self.randomNameChar(names[randNum])
    
        return [buttonChar, nameChar]

    def initializeGrid(self):
        # Randomize Grid (Either one or zero)
        self.gridNumber = "1"
        print ("Grid Layout: " + str(self.gridNumber) + " For Player " + str(self.numPlayer))
        self.initializationString = (str(self.numPlayer) + '&' + str(self.gridNumber) + '%')
        # Randomize Buttons Depending on Grid
        # Grid 1 Layout
        if self.gridNumber == 0:
            # Initialize Button 0 - Switch or Press + Name
            nameChar = self.randomNameChar(pressNames)
            self.initializationString += pressOptions + ':' + nameChar + '?'

            print(self.initializationString)
            # Initialize Button 1 - Horizontal Slider + Name

            nameChar = self.randomNameChar(switchNames)
            self.initializationString += 't' + ':' + nameChar + '?'
            print(self.initializationString)

            # Initialize Button 2 - Anything + Name
            
            buttonChar, nameChar = self.randMultOption(numOptions, totalOptions, optionNames)
            self.initializationString += ( buttonChar + ':' + nameChar + '?')
            print (self.initializationString)

            # Initialize Button 3 - Switch or Vertical Slider

            buttonChar, nameChar = self.randMultOption(switchVSliderAmount, switchVSlider, switchVSliderNames )
            self.initializationString += (buttonChar + ':' + nameChar)
            print("Final String for Grid 0: " + self.initializationString + " For Player " + str(self.numPlayer))
            

        # Grid 2 Layout
        else:

            # Button 0 - Horizontal Slider Only

            nameChar = self.randomNameChar(sliderNames)
            self.initializationString += (horizontalSlider + ':' + nameChar + '?')
            
            print(self.initializationString)
            # Button 1 - Vertical Slider or Switch
            
            buttonChar, nameChar = self.randMultOption(switchVSliderAmount, switchVSlider, switchVSliderNames )
            self.initializationString += (buttonChar + ':' + nameChar + '?')
            
            print(self.initializationString)
            # Button 2 - Switch Only

            buttonChar = switchOptions[self.randomNumber(switchAmount)]
            nameChar = self.randomNameChar(switchNames)
            self.initializationString += ( buttonChar + ':' + nameChar + '?')
            print(self.initializationString)

            # Button 3 - Press Only
            nameChar = self.randomNameChar(pressNames)
            self.initializationString += (pressOptions + ':' + nameChar)
            print(self.initializationString)

        print("Switch: " + str(switchNames))
        print("Slider: " + str(sliderNames))
        print("Press: " + str(pressNames))
        print(self.initializationString)
        self.sendLine(self.initializationString)


    def connectionMade(self):

        print("Current Players List: " + str(self.factory.players))
        if self.factory.connections == 2:
            for player in self.factory.players:
                player.sendLine("Ready")
        # Initialization of Grid/Buttons


    def connectionLost(self, reason):

        if self.factory.connections > 0:
            self.factory.connections -= 1
            self.factory.players.remove(self)
            print("The connection for a player was lost " + str(self.factory.connections) + " connection(s) remain.")
            for player in self.factory.players:
                player.sendLine("Not Ready")

    def initPlayerNum(self):
        initPlayer = 0
        for player in self.factory.players:
            player.sendLine(str(initPlayer))
            player.numPlayer = initPlayer
            initPlayer += 1

    def shareGrid(self):
        print ("I am Player " + str(self.numPlayer))
        self.initializeGrid()
        print("Initialized Player " + str(self.numPlayer))
        for player in self.factory.players:
            if player is not self:
                print("initializing player " + str(player.numPlayer) + " With Player " + str(self.numPlayer))
                player.sendLine(self.initializationString)

        self.factory.numSent += 1
        self.state = "Game"
        if self.factory.numSent == self.factory.connections:
            for player in self.factory.players:
                player.sendLine("END")


    def lineReceived(self, line):

        print(line)
        if self.state is 'startGame':
            for player in self.factory.players:
                if player is not self:
                    player.sendLine(line)
            readyCheck = 0
            if line is '1':
                self.readyStart = 1
            else:
                self.readyStart = 0
            for player in self.factory.players:
                if player.readyStart is 1:
                    readyCheck += 1
            if readyCheck == self.factory.connections and self.factory.connections > 1:  # change to > 1 after test
                print("Players Ready!")
                for player in self.factory.players:
                    player.sendLine("Players Ready")
                self.initPlayerNum()
                for player in self.factory.players:
                    player.shareGrid()

        elif self.state is 'Game':
            print("server in Game state")

            print("The line is " + line)
            for player in self.factory.players:
                if player is not self:
                    player.sendLine(line)
            """
            if parsed_line[0] == "c":

                print ("sending line 1 over: " + parsed_line[1])
                for player in self.factory.players:
                    if player is not self:
                        player.clearLineBuffer()
                        player.transport.write("0")

            else: # parsed_line[1] == " "
                print("index 0 is not 'c'")
            """




class ServerFactory(Factory):

    def __init__(self):

        print("Initiliazing ServerFactory...")
        print("Attemping to create TCP Endpoint for Server with:\n"
        + "Port: " + str(portNumber) + "\nIP Address: " + ipAddress)

        self.players = []
        self.connections = 0
        self.numSent = 0
        print("Number of Connections: " + str(self.connections))

    def buildProtocol(self, addr):

        if self.connections < 2:

            self.connections += 1
            print("Number of Connections: " + str(self.connections))
            return Server(self)

        else:
            print("2 Players Max! Not allowing anymore connections")




try:

    serverFactory = ServerFactory()
    reactor.listenTCP(portNumber, serverFactory, numQueues, ipAddress)

    print("Successfully created Socket...")

except CannotListenError:
    print("Error creating socket")

reactor.run()