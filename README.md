# Zephyr
This is my attempt at making a chess engine :D

I will be competing with Zephyr against my friend with another engine [Albel-coder](https://github.com/Albel-coder)


## I will be using C# to make a simple chess engine in the console, planning to grow it later

It will be a small program in the beginning, only able to do basic chess engine stuff

But I am planniтg to grow it into a strong chess engine/bot in the future


# Current small plans for the near future for this project:
- UI will currently be only on english, however I will add russian UI later
- (+) Make a board parsing function in the console
- (+) Make an Evaluation function
- Implement an optimised search algorith to calculate the positions
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


# Info :D
### 12.3.2025 - renaimed the project from _Gymat_ to _Zephyr_, 

**Reason, quote:**

_"Zephyr means a gentle breeze. The name reflects the lightness and elegance with which the engine solves complex problems, making it seem effortless and graceful."_
