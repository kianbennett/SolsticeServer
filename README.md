# Secret of the Solstice Server Emulator

## Features

- [x] Logging on to server with username
- [x] Character create/delete
- [x] Loading into map
- [x] Displaying character in map
- [x] Character movement
- [x] Emotes
- [x] Friends list
- [x] Basic server commands (!h)
- [x] NPCs
- [x] Moving between maps (sort of)
- [ ] Parties
- [ ] Combat
- [ ] Quests
- [ ] Display equipped items
- [ ] Guilds
- [ ] Monster spawns

## Live Servers

| Location	| IP				| Port		|
| --------- | ----------------- | --------- |
| EUW		| 35.178.43.164		| 1818		|

## Quickstart

- Download the client from [here](https://www.fileplanet.com/183005/180000/fileinfo/Secret-of-the-Solstice-Client)
- Copy the executable and batch file from the `client-patches` folder into the game folder
- Usually one would connect with the launcher giving a username and password, and a session token would be returned, which would be passed onto the master server. Until account creation is finished, a temporary solution is to pass just the username with -t and the server will pull characters from the database using this.
	- tl;dr edit the batch file to `-t your_username`, passwords will come later
- If self-hosting, run both `MasterServer.exe` and `SolsticeServer.exe` from the `build` folder
- Launch the client with `start.bat`

To connect to a live server change the `-i` parameter to the server ip. The server port is 1818 by default, use `-pt` to set a custom port

## Packet Structure

See the notes text files for information on packet structure, more detailed documentation will come soon.