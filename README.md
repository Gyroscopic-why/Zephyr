# Zephyr
This is my attempt at making a chess engine :D

I will be competing with Zephyr against my friend with another engine [Albel-coder](https://github.com/Albel-coder)


## I will be using C# to make a simple chess engine in the console, planning to grow it later

It will be a small program in the beginning, only able to do basic chess engine stuff

But I am planning to grow it into a strong chess engine/bot in the future


# Current small plans for the near future for this project:
- (+) Make a board parsing function in the console
- (+) Make an Evaluation function
- (+) Implement a function that calculates all legitimate moves
- (+) Implement a system for parsing user moves
- (+) Implement an optimised search algorith to calculate the positions
- (+) Make the engine make moves on it's own
- (+) Make the engine see CheckMates up to 3 full moves as of now
- UI will currently be only on english, however I will add russian UI later
- Build a Unity interface for better visualisation and experience
- Make the engine find and remember forced mate sequences
- Use multithread processing to increase the engines power and time efficiency


# History and changes:
### Alpha: 
- Added temporary placeholder code
- first board parsing prototypes


### Beta:
- Constants refactoring
- Added an ok parsing system
- Preparation for an optimised storing of the board
  (32 bytes array instead of 64 bytes in the classic version)


### Gamma:
- Slight code refactoring
- Added a working evaluation function:
     - Counts material
     - Counts isolated pawns
     - Counts center control
     - (+-) Counts king safety (Not final)

 
### Delta:
- Added a move calculation function
- Added an alpha beta search function, seems to work :D
- Added a start player color parser
- Added a more clean info
- Added a board encode function (the opposite of parsing)
- Now the engine can play a move (and for both player colors!)
- A bit of UI cleaning
- Added a temporary placeholder for the mate recognition and saving its sequence
  (   Doesn't work ):   )


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
- Fixed the applying on the dynamic king position
- Fixed the inabillity of a player to move if he is in check
- Fixed the ability to move your pieces that are pinned to the king


### Zeta:
- Advanced board evaluation (Now the engine is not only more smart, but also more agressive):
     - Better material score value calculation
     - Now the engine considers a piece position as an extra factor
     - Piece position tables chage to a version 2 when we reach endgame
       (+ Endgame detection)
     - Consideration of open files:
     -      Recognising open files
     -      Bonus for taking advantage of the open files
     -      Punishment for not using the open file
     -  Bonus for simplified center control calculation   (still)
     -  Bonus for simplified pawn structure calculation   (still)
     -  Bonus for very simplified king safety calculation (still)
- Pawns can now promote into any piece (not only the queen)
- Fixed a lot of small bugs
- Removed old stuff


### Zeta+:
- Added a better checkmate detection
- Added advanced info output for the evaluation function
- Fixed bishop and queens teleportation through the left and right boarders
- Fixed Check xrays from major pieces
- Fixed inverse king safety calculation
- Fixed Evaluation problems
- Tweaked the position tables
- Fixed crashing when you didn't input any symbols for the board in the parsing stage
- Optimised the board parsing function
- Fixed wrong error output in the board parsing stage


### Zeta++:
- Added a pseudo-graphical interface
- Fixed program crashes
- Added a better info display
- Improved optimisation


### Eta (Entire generation)
- Fixed a wrong discartion error of legal moves
- Massive optimization compared to the Zeta generation
- Added the ability to make your own moves (play against the computer)
- Added finding forced moves up to 6 moves forward
- Changed position tables in the  AdvEvaluate() function
- Changed user interface 
- Added field markup output for more visual and convenient calculations for moves
- Changed the colors of the players when the board is drawn for a more understandable difference in large figures of different players
- Added the exact output of the number of moves when a forced chemate is detected
- Added correctly working game stopper when a checkmate is reached
- Added pseudo-dynamic depth for the searching
- Added technical output of information: the best move at the moment of each search depth
- Added the saving of the amount of evaluated boards
- Dispplaying the amount of evaluated boards (for each move)
- Fixed errors with the ouput of time spent on the move,
   as well as errors related to the output of the number of branches missed
   (thanks to Alpha Beta optimization)
- Fixed many minor errors related to the info output
- Board parsing errors were fixed (again)
- The position evaluation function has been greatly improved
- Changed some old mechanics in position evaluation functions
- Upon winning, the player will now fully try to avoid forced draw (pat)
- Improved the calculation of checks
- Included the enemy king's check on our king as an illegal move
- Improved the move generation function
- Code refactoring

  
# Project milestones :D

## 12.3.2025 - renaimed the project from _Gymat_ to _Zephyr_, 
**Reason, quote:**

_"Zephyr means a gentle breeze. The name reflects the lightness and elegance with which the engine solves complex problems, making it seem effortless and graceful."_


## 18.3.2025 - First big improvement in the engine' s strength
Happened with the release of the version Zeta

Probably because of the new AdvEvaluate() function

Most significant upgrade was: Position tables



## 20.3.2025 - First checkmate from the engine
First full recorded game where the engine from to start position with no advantage against the enemy (itself)

made a sequence of moves that led to the checkmate of the enemy (itself in black color)

**Input:**
- Depth = 4
- Board position: 0 
  (Classic start position)
- White turn: true



## 7.4.2025 - First found forced mate in 6
After playing some test positions, the Eta13 version successfully detected the hidden mate in 6 

(And successfully executed it as well)



## 8.4.4045 - First stable version reaches a minimum of 1500-1600 Elo (Eta13.1, Eta13.2 and V4)!
First time the Zephyr engine actually beat some strong opponents (1000-1600 Elo bots from [Chess.com](https://chess.com))

It still hadn't lost to any of them, however due to the hard communication in using it as of now

I have only played it a few times against 1000-1600 Elo opponents. 

Worst case scenario - the game ends in a draw, so Zephyr's Elo is probably a bit higher than 1600


# More comming soon :)
