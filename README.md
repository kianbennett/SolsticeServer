# Secret of the Solstice Server Emulator

## Features

- [x] Logging on to server with username
- [x] Character create/delete
- [x] Loading into map
- [ ] Displaying character in map
- [ ] Character movement

## Quickstart

- Download the client from [here](https://www.fileplanet.com/183005/180000/fileinfo/Secret-of-the-Solstice-Client)
- Copy the executable and batch file from the [patches](patches) folder into the game folder
- Usually one would connect with the launcher giving a username and password, and a session token would be returned, which would be passed onto the master server. Until account creation is finished, a temporary solution is to pass just the username with -t and the server will pull characters from the database using this.
	- tl;dr edit the batch file to `-t your_username_here`, passwords will come later
- Run both `MasterServer.exe` and `SolsticeServer.exe` from the [build](build) folder
- Launch the client with `start.bat`

## Packet Structure

See the notes text files for information on packet structure, more detailed documentation will come soon.