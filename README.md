# Zephyr
This is my attempt at making a chess engine :D

I will be competing with Zephyr against my friend with another engine [Albel-coder](https://github.com/Albel-coder)


## I will be using C# to make a simple chess engine in the console, planning to grow it later

It will be a small program in the beginning, only able to do basic chess engine stuff

But I am planning to grow it into a strong chess engine/bot in the future


# Current small plans for the near future for this project:
- UI will currently be only on english, however I will add russian UI later
- (+) Make a board parsing function in the console
- (+) Make an Evaluation function
- (+) Implement a function that calculates all legitimate moves
- Make the engine find and remember forced mate sequences
- (+-) Implement an optimised search algorith to calculate the positions
- (+) Make the engine make moves on it's own
- Use multithread processing to increase the engines power and time efficiency
- Make the engine see CheckMates up to 3 full moves as of now


# History and changes:
### Alpha: 
- Added temporary placeholder code
- parsing works.. yeah just works

### Beta:
- Constants refactoring
- Added a very good parsing system
- Added a preparation for an optimised storing of the board
  (32 bytes array instead of 64 bytes in the classic version)

### Gamma:
- Slight code refactoring
- Added a working evaluation function:
     - Counts material
     - Counts isolated pawns
     - Counts center control
     - (+-) Counts king safety (This needs to be reworked)
 
### Delta:
- Added a move calculation function
- Added an alpha beta search function, seems to work :D
- Added a start player color parser
- Added a more clean info
- Added a board encode function (the opposite of parsing)
- Now the engine can play a move (and for both player colors!)
- A bit of UI cleaning
- Added a temporary placeholder for the mate recognition and saving its sequence
  (Yeah it doesn't work)

### Delta++:
- Fixed the crash when a player can't move for some reason
- Massive UI cleaning (although not final)
- Added a fen and custom fen board parsing
- Added the board encoding in a custom fen
- Fixed the king check function (although there are still very specific cases that don't work)
- Added an auto new game option
- Small optimisation

### Epsilon:
- Added a check detection
- Fixed a lot of movement bug with clipping through pieces
- Move generation optimisation
- Board parsing bug fixed
- Board Fen encoding bug fixed
- Now pawns that reach the final promotion rank transform into queens instead of sitting there
- Added a position benchmark test (only in the code as a function)
- Added a verification of the generated moves = if they still leave the king in a check they are disqualified as being valid
- Fixed bug where if the king was in check he could play other moves (now the player doesnt move at all lol, but still better)

### Epsilon+:
- Fixed incorrect error display in the board parsing function
- Fixed the app freeze in the board parsing function
- Fixed the app crash after typed some very specific input in the board parsing function
- Fixed the apying on the dynamic king position
- Fixed the inabillity of a player to move if he is in check
- Fixed the ability to move your pieces that are pinned to the king

# Info :D
### 12.3.2025 - renaimed the project from _Gymat_ to _Zephyr_, 

**Reason, quote:**

_"Zephyr means a gentle breeze. The name reflects the lightness and elegance with which the engine solves complex problems, making it seem effortless and graceful."_


