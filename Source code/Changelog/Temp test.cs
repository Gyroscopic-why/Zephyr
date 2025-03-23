using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

// Token: 0x02000002 RID: 2
internal class Program
{
	// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	private static void Main()
	{
		Console.OutputEncoding = Encoding.Unicode;
		Console.Title = "Zephyr engine Eta";
		string continueGame = "";
		Program.Move makeBestMove = null;
		while (continueGame != "exit")
		{
			Program.gCheckMate = 0;
			int depth = Program.GetDepth();
			bool boardDisplayType = Program.GetBoardDisplayType();
			Program.GetEncodedBoard(false);
			Program.whiteTurn = Program.GetTurn();
			Console.Clear();
			Program.DisplayBoard(Program.mainBoard, boardDisplayType, 64, 64);
			int eval = Program.AdvEvaluate(Program.mainBoard, true, true);
			Console.Write(string.Format("\n\t\t\t\tCurrent eval: {0}\n\n\n\t", eval));
			Console.Write("White king in check: " + Program.IsKingInCheck(Program.mainBoard, Program.wkPos, true).ToString() + ", wkPos: " + Program.wkPos.ToString());
			Console.Write("\n\tBlack king in check: " + Program.IsKingInCheck(Program.mainBoard, Program.bkPos, false).ToString() + ", bkPos: " + Program.bkPos.ToString());
			Console.Write("\n\n\tContinue?  (press ENTER): ");
			continueGame = Console.ReadLine();
			int alpha = int.MinValue;
			int beta = int.MaxValue;
			while (continueGame == "")
			{
				Stopwatch timeCounter = Stopwatch.StartNew();
				bool flag = Program.gCheckMate < 3;
				if (flag)
				{
					Program.AlphaBetaSearch(Program.mainBoard, depth, alpha, beta, Program.whiteTurn, out makeBestMove);
					Program.ApplyMove(Program.mainBoard, makeBestMove);
					Program.whiteTurn = !Program.whiteTurn;
					Console.Clear();
				}
				else
				{
					Console.Clear();
					Console.Write("\t\tCHECKMATE  :D");
				}
				timeCounter.Stop();
				bool flag2 = makeBestMove != null;
				if (flag2)
				{
					Program.DisplayBoard(Program.mainBoard, boardDisplayType, makeBestMove.From, makeBestMove.To);
				}
				else
				{
					Program.DisplayBoard(Program.mainBoard, boardDisplayType, 64, 64);
				}
				eval = Program.AdvEvaluate(Program.mainBoard, true, true);
				Console.Write(string.Format("\n\t\t\t\tCurrent eval: {0}\n\n\n\t", eval));
				Console.Write(string.Concat(new string[]
				{
					"\n\tAlpha-beta skipped: ",
					Program.gSkippedPositions.ToString(),
					", time elapsed: ",
					timeCounter.ElapsedMilliseconds.ToString(),
					" ms\n\t"
				}));
				Console.Write("White king in check: " + Program.IsKingInCheck(Program.mainBoard, Program.wkPos, true).ToString() + ", wkPos: " + Program.wkPos.ToString());
				Console.Write("\n\tBlack king in check: " + Program.IsKingInCheck(Program.mainBoard, Program.bkPos, false).ToString() + ", bkPos: " + Program.bkPos.ToString());
				Console.Write("\n\n\tContinue?  (press ENTER): ");
				continueGame = Console.ReadLine().Trim().ToLower();
			}
			Program.EncodeBoard(Program.mainBoard);
			bool flag3 = continueGame != "exit";
			if (flag3)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("\tNew game settings: ");
				Console.ForegroundColor = ConsoleColor.White;
			}
		}
		Console.ReadKey();
	}

	// Token: 0x06000002 RID: 2 RVA: 0x00002378 File Offset: 0x00000578
	private static bool GetEncodedBoard(bool _type)
	{
		bool _validEncoding = false;
		bool _reset = false;
		while (!_validEncoding)
		{
			string _encodedBoard = "";
			Console.Write("\n\tEnter the board (64 characters): (SP: 0 / rnbqkbnrpppppppp32PPPPPPPPRNBQKBNR)\n\t");
			while (_encodedBoard.Length < 64 && !_validEncoding && !_reset)
			{
				string _userInput = Console.ReadLine().Replace(" ", "");
				_encodedBoard += _userInput;
				bool flag = _userInput == "0";
				if (flag)
				{
					_encodedBoard = "rnbqkbnrpppppppp32PPPPPPPPRNBQKBNR";
				}
				bool flag2 = _encodedBoard.Length <= 64 && _encodedBoard.Length > 0;
				if (flag2)
				{
					bool flag3 = Program.TryParseClassicBoard(ref _encodedBoard);
					if (flag3)
					{
						_validEncoding = true;
					}
					else
					{
						bool flag4 = _encodedBoard[_encodedBoard.Length - 1] == '#';
						if (flag4)
						{
							Console.Clear();
							Console.Write("\t[!]  - Error while parsing board: to many characters\n");
							_reset = true;
						}
						else
						{
							bool flag5 = _encodedBoard[_encodedBoard.Length - 1] == '!';
							if (flag5)
							{
								Console.Clear();
								Console.Write("\t[!]  - Error while parsing board: unknown character\n");
								_reset = true;
							}
						}
					}
				}
				Console.Write("\t");
			}
			bool flag6 = _encodedBoard.Length == 64;
			if (flag6)
			{
				if (!_type)
				{
					bool flag7 = Program.TryParseClassicBoard(ref _encodedBoard);
					if (flag7)
					{
						_validEncoding = true;
					}
					else
					{
						Console.Clear();
						Console.Write("\t[!]  - Error while parsing board: unknown character\n");
					}
				}
			}
		}
		return _validEncoding;
	}

	// Token: 0x06000003 RID: 3 RVA: 0x000024E8 File Offset: 0x000006E8
	private static bool TryParseOptimisedBoard(string _encoded, byte[] _board)
	{
		byte i = 0;
		while (i < 32)
		{
			byte _tempBuffer = Program.ConvertBoardToBytes(_encoded[(int)i], true);
			bool flag = _tempBuffer == 8;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				_board[(int)i] = _tempBuffer;
				bool flag2 = _board[(int)i] == 6;
				if (flag2)
				{
					Program.wkPos = i;
				}
				bool flag3 = _board[(int)i] == 14;
				if (flag3)
				{
					Program.bkPos = i;
				}
				_tempBuffer = Program.ConvertBoardToBytes(_encoded[(int)i], false);
				bool flag4 = _tempBuffer == 8;
				if (!flag4)
				{
					byte b = i;
					_board[(int)b] = _board[(int)b] + _tempBuffer;
					bool flag5 = _board[(int)i] == 6;
					if (flag5)
					{
						Program.wkPos = i;
					}
					bool flag6 = _board[(int)i] == 14;
					if (flag6)
					{
						Program.bkPos = i;
					}
					i += 1;
					continue;
				}
				result = false;
			}
			return result;
		}
		return true;
	}

	// Token: 0x06000004 RID: 4 RVA: 0x000025B4 File Offset: 0x000007B4
	private static bool TryParseClassicBoard(ref string _encoded)
	{
		byte _fenOffset = 0;
		byte i = 0;
		while ((int)i < _encoded.Length)
		{
			bool flag = i + _fenOffset < 64;
			if (flag)
			{
				Program.mainBoard[(int)(i + _fenOffset)] = Program.ConvertBoardToBytes(_encoded[(int)i], true);
				bool flag2 = Program.mainBoard[(int)(i + _fenOffset)] == 8;
				if (flag2)
				{
					string _temp = "";
					bool flag3 = (int)i < _encoded.Length - 1;
					if (flag3)
					{
						_temp += _encoded[(int)i].ToString();
						_temp += _encoded[(int)(i + 1)].ToString();
					}
					byte _freeSpaceForFen;
					bool flag4 = byte.TryParse(_temp, out _freeSpaceForFen);
					if (flag4)
					{
						for (byte j = i; j < _freeSpaceForFen + i; j += 1)
						{
							bool flag5 = i + _fenOffset < 64;
							if (flag5)
							{
								Program.mainBoard[(int)(i + _fenOffset)] = 0;
								_fenOffset += 1;
							}
							else
							{
								j += _freeSpaceForFen;
								_encoded += "#";
							}
						}
						_fenOffset -= 2;
						i += 1;
					}
					else
					{
						bool flag6 = byte.TryParse(_encoded[(int)i].ToString(), out _freeSpaceForFen);
						if (!flag6)
						{
							_encoded += "!";
							return false;
						}
						for (int k = (int)i; k < (int)(_freeSpaceForFen + i); k++)
						{
							bool flag7 = i + _fenOffset < 64;
							if (flag7)
							{
								Program.mainBoard[(int)(i + _fenOffset)] = 0;
								_fenOffset += 1;
							}
							else
							{
								k += (int)_freeSpaceForFen;
								_encoded += "#";
							}
						}
						_fenOffset -= 1;
					}
				}
				bool flag8 = Program.mainBoard[(int)(i + _fenOffset)] == 6;
				if (flag8)
				{
					Program.wkPos = i + _fenOffset;
				}
				bool flag9 = Program.mainBoard[(int)(i + _fenOffset)] == 14;
				if (flag9)
				{
					Program.bkPos = i + _fenOffset;
				}
			}
			else
			{
				i += 128;
				_encoded += "#";
			}
			i += 1;
		}
		bool flag10 = _encoded.Length + (int)_fenOffset != 64;
		return !flag10;
	}

	// Token: 0x06000005 RID: 5 RVA: 0x000027F0 File Offset: 0x000009F0
	private static byte ConvertBoardToBytes(char _encodedPiece, bool _parsingPos)
	{
		byte result;
		if (_parsingPos)
		{
			if (_encodedPiece <= 'B')
			{
				switch (_encodedPiece)
				{
				case '+':
				case '-':
				case '/':
					break;
				case ',':
				case '.':
					goto IL_119;
				default:
					if (_encodedPiece != '=')
					{
						if (_encodedPiece != 'B')
						{
							goto IL_119;
						}
						return 3;
					}
					break;
				}
			}
			else if (_encodedPiece <= '_')
			{
				switch (_encodedPiece)
				{
				case 'K':
					return 6;
				case 'L':
				case 'M':
				case 'O':
					goto IL_119;
				case 'N':
					return 2;
				case 'P':
					return 1;
				case 'Q':
					return 5;
				case 'R':
					return 4;
				default:
					if (_encodedPiece != '_')
					{
						goto IL_119;
					}
					break;
				}
			}
			else
			{
				if (_encodedPiece == 'b')
				{
					return 11;
				}
				switch (_encodedPiece)
				{
				case 'k':
					return 14;
				case 'l':
				case 'm':
				case 'o':
					goto IL_119;
				case 'n':
					return 10;
				case 'p':
					return 9;
				case 'q':
					return 13;
				case 'r':
					return 12;
				default:
					goto IL_119;
				}
			}
			return 0;
			IL_119:
			result = 8;
		}
		else
		{
			if (_encodedPiece <= 'B')
			{
				switch (_encodedPiece)
				{
				case '+':
				case '-':
				case '/':
				case '0':
					break;
				case ',':
				case '.':
					goto IL_231;
				default:
					if (_encodedPiece != '=')
					{
						if (_encodedPiece != 'B')
						{
							goto IL_231;
						}
						return 48;
					}
					break;
				}
			}
			else if (_encodedPiece <= '_')
			{
				switch (_encodedPiece)
				{
				case 'K':
					return 96;
				case 'L':
				case 'M':
				case 'O':
					goto IL_231;
				case 'N':
					return 32;
				case 'P':
					return 16;
				case 'Q':
					return 80;
				case 'R':
					return 64;
				default:
					if (_encodedPiece != '_')
					{
						goto IL_231;
					}
					break;
				}
			}
			else
			{
				if (_encodedPiece == 'b')
				{
					return 176;
				}
				switch (_encodedPiece)
				{
				case 'k':
					return 224;
				case 'l':
				case 'm':
				case 'o':
					goto IL_231;
				case 'n':
					return 160;
				case 'p':
					return 144;
				case 'q':
					return 208;
				case 'r':
					return 192;
				default:
					goto IL_231;
				}
			}
			return 0;
			IL_231:
			result = 8;
		}
		return result;
	}

	// Token: 0x06000006 RID: 6 RVA: 0x00002A34 File Offset: 0x00000C34
	private static char ConvertBoardToChars(byte _decodedPiece, bool _parsingPos)
	{
		char result;
		if (_parsingPos)
		{
			switch (_decodedPiece)
			{
			case 1:
				return 'P';
			case 2:
				return 'N';
			case 3:
				return 'B';
			case 4:
				return 'R';
			case 5:
				return 'Q';
			case 6:
				return 'K';
			case 9:
				return 'p';
			case 10:
				return 'n';
			case 11:
				return 'b';
			case 12:
				return 'r';
			case 13:
				return 'q';
			case 14:
				return 'k';
			}
			result = '+';
		}
		else
		{
			if (_decodedPiece <= 80)
			{
				if (_decodedPiece <= 32)
				{
					if (_decodedPiece != 0)
					{
						if (_decodedPiece == 16)
						{
							return 'P';
						}
						if (_decodedPiece == 32)
						{
							return 'N';
						}
					}
				}
				else
				{
					if (_decodedPiece == 48)
					{
						return 'B';
					}
					if (_decodedPiece == 64)
					{
						return 'R';
					}
					if (_decodedPiece == 80)
					{
						return 'Q';
					}
				}
			}
			else if (_decodedPiece <= 160)
			{
				if (_decodedPiece == 96)
				{
					return 'K';
				}
				if (_decodedPiece == 144)
				{
					return 'p';
				}
				if (_decodedPiece == 160)
				{
					return 'n';
				}
			}
			else if (_decodedPiece <= 192)
			{
				if (_decodedPiece == 176)
				{
					return 'b';
				}
				if (_decodedPiece == 192)
				{
					return 'r';
				}
			}
			else
			{
				if (_decodedPiece == 208)
				{
					return 'q';
				}
				if (_decodedPiece == 224)
				{
					return 'k';
				}
			}
			result = '+';
		}
		return result;
	}

	// Token: 0x06000007 RID: 7 RVA: 0x00002BEC File Offset: 0x00000DEC
	private static void DisplayBoard(byte[] _board, bool _displayType, byte _moveFrom = 64, byte _moveTo = 64)
	{
		bool flag = Program.gPieceGotEaten;
		if (flag)
		{
			Program.gPieceGotEaten = false;
		}
		else
		{
			Console.Write("\n\n\n");
		}
		Console.Write("\n\n\n");
		bool flag2 = !_displayType;
		if (flag2)
		{
			Console.Write("\n\n\n");
		}
		Console.Write("\t\t\t\tParsed board (in bytes): " + _board.Length.ToString() + "\n\n\n\t\t\t  ");
		bool flag3 = _board.Length == 64;
		if (flag3)
		{
			bool flag4 = !_displayType;
			if (flag4)
			{
				for (int i = 0; i < 64; i++)
				{
					bool flag5 = _board[i] == 0;
					if (flag5)
					{
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						bool flag6 = _board[i] == 1 || _board[i] == 9;
						if (flag6)
						{
							Console.ForegroundColor = ConsoleColor.White;
						}
						else
						{
							bool flag7 = _board[i] == 6 || _board[i] == 14;
							if (flag7)
							{
								Console.ForegroundColor = ConsoleColor.Red;
							}
							else
							{
								bool flag8 = _board[i] == 5 || _board[i] == 13;
								if (flag8)
								{
									Console.ForegroundColor = ConsoleColor.DarkMagenta;
								}
								else
								{
									bool flag9 = _board[i] < 8;
									if (flag9)
									{
										Console.ForegroundColor = ConsoleColor.DarkGreen;
									}
									else
									{
										Console.ForegroundColor = ConsoleColor.DarkBlue;
									}
								}
							}
						}
					}
					bool flag10 = i == (int)_moveFrom;
					if (flag10)
					{
						Console.ForegroundColor = ConsoleColor.DarkRed;
					}
					else
					{
						bool flag11 = i == (int)_moveTo;
						if (flag11)
						{
							Console.ForegroundColor = ConsoleColor.Cyan;
						}
					}
					bool flag12 = _board[i] > 9;
					if (flag12)
					{
						Console.Write(" " + _board[i].ToString() + "  ");
					}
					else
					{
						Console.Write(" 0" + _board[i].ToString() + "  ");
					}
					bool flag13 = i % 8 == 7;
					if (flag13)
					{
						Console.Write("\n\n\t\t\t  ");
					}
				}
			}
			else
			{
				Console.Write("\t  ");
				for (int j = 0; j < 64; j++)
				{
					bool flag14 = _board[j] == 0;
					if (flag14)
					{
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						bool flag15 = _board[j] == 1 || _board[j] == 9;
						if (flag15)
						{
							Console.ForegroundColor = ConsoleColor.White;
						}
						else
						{
							bool flag16 = _board[j] == 6 || _board[j] == 14;
							if (flag16)
							{
								Console.ForegroundColor = ConsoleColor.Red;
							}
							else
							{
								bool flag17 = _board[j] == 5 || _board[j] == 13;
								if (flag17)
								{
									Console.ForegroundColor = ConsoleColor.DarkMagenta;
								}
								else
								{
									bool flag18 = _board[j] < 8;
									if (flag18)
									{
										Console.ForegroundColor = ConsoleColor.DarkGreen;
									}
									else
									{
										Console.ForegroundColor = ConsoleColor.DarkBlue;
									}
								}
							}
						}
					}
					bool flag19 = (j + j / 8) % 2 == 0;
					if (flag19)
					{
						Console.BackgroundColor = ConsoleColor.Gray;
					}
					else
					{
						Console.BackgroundColor = ConsoleColor.Black;
					}
					bool flag20 = j == (int)_moveFrom;
					if (flag20)
					{
						Console.BackgroundColor = ConsoleColor.DarkRed;
					}
					else
					{
						bool flag21 = j == (int)_moveTo;
						if (flag21)
						{
							Console.BackgroundColor = ConsoleColor.Cyan;
						}
					}
					switch (_board[j])
					{
					case 0:
						Console.Write(" . ");
						break;
					case 1:
						Console.Write(" ♟ ");
						break;
					case 2:
						Console.Write(" ♞ ");
						break;
					case 3:
						Console.Write(" ♝ ");
						break;
					case 4:
						Console.Write(" ♜ ");
						break;
					case 5:
						Console.Write(" ♛ ");
						break;
					case 6:
						Console.Write(" ♚ ");
						break;
					case 9:
						Console.Write(" ♙ ");
						break;
					case 10:
						Console.Write(" ♘ ");
						break;
					case 11:
						Console.Write(" ♗ ");
						break;
					case 12:
						Console.Write(" ♖ ");
						break;
					case 13:
						Console.Write(" ♕ ");
						break;
					case 14:
						Console.Write(" ♔ ");
						break;
					}
					bool flag22 = j % 8 == 7 && j < 63;
					if (flag22)
					{
						Console.BackgroundColor = ConsoleColor.Black;
						Console.Write("\n\t\t\t\t  ");
					}
				}
			}
			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.Write("\n\n\t\t\t");
		}
		else
		{
			for (int k = 0; k < 32; k++)
			{
			}
		}
	}

	// Token: 0x06000008 RID: 8 RVA: 0x0000303C File Offset: 0x0000123C
	private static void EncodeBoard(byte[] _board)
	{
		string _fullEncoded = "";
		string _fen = "";
		byte _emptySquares = 0;
		bool flag = _board.Length == 64;
		if (flag)
		{
			for (byte i = 0; i < 64; i += 1)
			{
				_fullEncoded += Program.ConvertBoardToChars(_board[(int)i], true).ToString();
				bool flag2 = _fullEncoded[(int)i] != '+';
				if (flag2)
				{
					bool flag3 = _emptySquares > 0;
					if (flag3)
					{
						_fen += _emptySquares.ToString();
						_emptySquares = 0;
					}
					_fen += _fullEncoded[(int)i].ToString();
				}
				else
				{
					_emptySquares += 1;
				}
			}
			bool flag4 = _emptySquares > 0;
			if (flag4)
			{
				_fen += _emptySquares.ToString();
			}
		}
		_fullEncoded = _fullEncoded.Replace("++++++++", "+++++++/");
		Program.PrintEncodedBoard(_fullEncoded, _fen);
	}

	// Token: 0x06000009 RID: 9 RVA: 0x0000312A File Offset: 0x0000132A
	private static void PrintEncodedBoard(string _encodedBoard, string _boardFen)
	{
		Console.Write("\n\tEncoded board code: " + _encodedBoard);
		Console.Write("\n\tCustom fen board code: " + _boardFen + "\n\n\n");
	}

	// Token: 0x0600000A RID: 10 RVA: 0x00003154 File Offset: 0x00001354
	private static int AdvEvaluate(byte[] _board, bool _isWhite, bool _writeInfo = false)
	{
		int[] STARTpawnTable = new int[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			80,
			80,
			80,
			70,
			70,
			80,
			80,
			80,
			20,
			20,
			30,
			30,
			30,
			30,
			20,
			20,
			10,
			10,
			20,
			30,
			30,
			20,
			10,
			10,
			0,
			0,
			5,
			30,
			30,
			5,
			0,
			0,
			0,
			5,
			0,
			5,
			5,
			0,
			5,
			0,
			10,
			10,
			5,
			-20,
			-20,
			5,
			10,
			10,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};
		int[] STARTknightTable = new int[]
		{
			-50,
			-40,
			-30,
			-30,
			-30,
			-30,
			-40,
			-50,
			-41,
			-20,
			0,
			5,
			5,
			0,
			-20,
			-41,
			-30,
			0,
			20,
			25,
			25,
			20,
			0,
			-30,
			-30,
			5,
			25,
			35,
			35,
			25,
			5,
			-30,
			-30,
			0,
			25,
			35,
			35,
			25,
			0,
			-30,
			-30,
			5,
			20,
			25,
			25,
			20,
			5,
			-30,
			-51,
			-20,
			0,
			5,
			5,
			0,
			-20,
			-51,
			-50,
			-50,
			-30,
			-30,
			-30,
			-30,
			-50,
			-50
		};
		int[] STARTbishopTable = new int[]
		{
			-25,
			-10,
			-10,
			-10,
			-10,
			-10,
			-10,
			-25,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-10,
			-10,
			0,
			10,
			15,
			15,
			10,
			0,
			-10,
			-10,
			5,
			15,
			20,
			20,
			15,
			5,
			-10,
			-10,
			0,
			15,
			20,
			20,
			15,
			0,
			-10,
			-10,
			5,
			10,
			15,
			15,
			10,
			5,
			-10,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-10,
			-25,
			-10,
			-10,
			-10,
			-10,
			-10,
			-10,
			-25
		};
		int[] STARTrookTable = new int[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			10,
			20,
			20,
			20,
			20,
			20,
			20,
			10,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-10,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-10,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-10,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-10,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-10,
			0,
			0,
			0,
			10,
			10,
			0,
			0,
			0
		};
		int[] STARTqueenTable = new int[]
		{
			-20,
			-10,
			-10,
			-5,
			-5,
			-10,
			-10,
			-20,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-10,
			-10,
			0,
			10,
			10,
			10,
			10,
			0,
			-10,
			-5,
			0,
			10,
			15,
			15,
			10,
			0,
			-5,
			0,
			0,
			10,
			15,
			15,
			10,
			0,
			-5,
			-10,
			5,
			10,
			10,
			10,
			10,
			0,
			-10,
			-10,
			0,
			5,
			0,
			0,
			0,
			0,
			-10,
			-20,
			-10,
			-10,
			-5,
			-5,
			-10,
			-10,
			-20
		};
		int[] STARTkingTable = new int[]
		{
			-30,
			-40,
			-40,
			-50,
			-50,
			-40,
			-40,
			-30,
			-30,
			-40,
			-40,
			-50,
			-50,
			-40,
			-40,
			-30,
			-30,
			-40,
			-40,
			-50,
			-50,
			-40,
			-40,
			-30,
			-30,
			-40,
			-40,
			-50,
			-50,
			-40,
			-40,
			-30,
			-20,
			-30,
			-30,
			-40,
			-40,
			-30,
			-30,
			-20,
			-10,
			-20,
			-20,
			-20,
			-20,
			-20,
			-20,
			-10,
			20,
			20,
			0,
			0,
			0,
			0,
			20,
			20,
			20,
			30,
			10,
			0,
			0,
			10,
			30,
			20
		};
		int[] ENDpawnTable = new int[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			70,
			60,
			55,
			50,
			50,
			55,
			60,
			70,
			20,
			20,
			30,
			30,
			30,
			30,
			20,
			20,
			10,
			10,
			20,
			20,
			20,
			20,
			10,
			10,
			-5,
			5,
			0,
			0,
			0,
			0,
			5,
			-5,
			-20,
			-15,
			-15,
			-20,
			-20,
			-15,
			-15,
			-20,
			-30,
			-20,
			-20,
			-40,
			-40,
			-20,
			-20,
			-30,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};
		int[] ENDknightTable = new int[]
		{
			-50,
			-40,
			-30,
			-30,
			-30,
			-30,
			-40,
			-50,
			-41,
			-20,
			0,
			5,
			5,
			0,
			-20,
			-41,
			-30,
			0,
			20,
			25,
			25,
			20,
			0,
			-30,
			-30,
			5,
			25,
			35,
			35,
			25,
			5,
			-30,
			-30,
			0,
			25,
			35,
			35,
			25,
			0,
			-30,
			-30,
			5,
			20,
			25,
			25,
			20,
			5,
			-30,
			-51,
			-20,
			0,
			5,
			5,
			0,
			-20,
			-51,
			-50,
			-50,
			-30,
			-30,
			-30,
			-30,
			-50,
			-50
		};
		int[] ENDbishopTable = new int[]
		{
			-40,
			-10,
			-10,
			-10,
			-10,
			-10,
			-10,
			-40,
			-20,
			0,
			0,
			0,
			0,
			0,
			0,
			-20,
			-15,
			0,
			10,
			15,
			15,
			10,
			0,
			-15,
			-10,
			5,
			15,
			20,
			20,
			15,
			5,
			-10,
			-10,
			0,
			15,
			20,
			20,
			15,
			0,
			-10,
			-15,
			5,
			10,
			15,
			15,
			10,
			5,
			-15,
			-20,
			0,
			0,
			0,
			0,
			0,
			0,
			-20,
			-40,
			-10,
			-10,
			-10,
			-10,
			-10,
			-10,
			-40
		};
		int[] ENDrookTable = new int[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			10,
			20,
			20,
			20,
			20,
			20,
			20,
			10,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-5,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-5,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-5,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-5,
			-5,
			0,
			0,
			0,
			0,
			0,
			0,
			-5,
			0,
			0,
			0,
			10,
			10,
			0,
			0,
			0
		};
		int[] ENDqueenTable = new int[]
		{
			-20,
			-10,
			-10,
			-5,
			-5,
			-10,
			-10,
			-20,
			-10,
			0,
			0,
			0,
			0,
			0,
			0,
			-10,
			-10,
			0,
			10,
			10,
			10,
			10,
			0,
			-10,
			-5,
			0,
			10,
			15,
			15,
			10,
			0,
			-5,
			0,
			0,
			10,
			15,
			15,
			10,
			0,
			-5,
			-10,
			5,
			10,
			10,
			10,
			10,
			0,
			-10,
			-10,
			0,
			5,
			0,
			0,
			0,
			0,
			-10,
			-20,
			-10,
			-10,
			-5,
			-5,
			-10,
			-10,
			-20
		};
		int[] ENDkingTable = new int[]
		{
			-50,
			-30,
			-10,
			-5,
			-5,
			-10,
			-30,
			-50,
			-30,
			-10,
			-3,
			-2,
			-2,
			-3,
			-10,
			-30,
			-10,
			-3,
			-2,
			-1,
			-1,
			-2,
			-3,
			-10,
			-5,
			-2,
			-1,
			0,
			0,
			-1,
			-2,
			-5,
			-5,
			-2,
			-1,
			0,
			0,
			-1,
			-2,
			-5,
			-10,
			-3,
			-2,
			-1,
			-1,
			-2,
			-3,
			-10,
			-30,
			-10,
			-3,
			-2,
			-2,
			-3,
			-10,
			-30,
			-50,
			-30,
			-10,
			-5,
			-5,
			-10,
			-30,
			-50
		};
		int _blackMajorPieces = 0;
		int _whiteMajorPieces = 0;
		int _materialVal = Program.CalculateMaterial(_board, ref _whiteMajorPieces, ref _blackMajorPieces);
		int _positionalVal = 0;
		int _enemyInCheckVal = 0;
		bool flag = Math.Max(_whiteMajorPieces, _blackMajorPieces) > 1700;
		if (flag)
		{
			for (byte i = 0; i < 64; i += 1)
			{
				byte _piece = _board[(int)i];
				bool flag2 = _piece == 0;
				if (!flag2)
				{
					switch (_piece)
					{
					case 1:
						_positionalVal += STARTpawnTable[(int)i];
						break;
					case 2:
						_positionalVal += STARTknightTable[(int)i];
						break;
					case 3:
						_positionalVal += STARTbishopTable[(int)i];
						break;
					case 4:
						_positionalVal += STARTrookTable[(int)i];
						break;
					case 5:
						_positionalVal += STARTqueenTable[(int)i];
						break;
					case 6:
						_positionalVal += STARTkingTable[(int)i];
						Program.wkPos = i;
						break;
					case 9:
						_positionalVal -= STARTpawnTable[(int)(63 - i)];
						break;
					case 10:
						_positionalVal -= STARTknightTable[(int)(63 - i)];
						break;
					case 11:
						_positionalVal -= STARTbishopTable[(int)(63 - i)];
						break;
					case 12:
						_positionalVal -= STARTrookTable[(int)(63 - i)];
						break;
					case 13:
						_positionalVal -= STARTqueenTable[(int)(63 - i)];
						break;
					case 14:
						_positionalVal -= STARTkingTable[(int)(63 - i)];
						Program.bkPos = i;
						break;
					}
					_positionalVal = _positionalVal;
				}
			}
		}
		else
		{
			for (byte j = 0; j < 64; j += 1)
			{
				byte _piece2 = _board[(int)j];
				bool flag3 = _piece2 == 0;
				if (!flag3)
				{
					switch (_piece2)
					{
					case 1:
						_positionalVal += ENDpawnTable[(int)j];
						break;
					case 2:
						_positionalVal += ENDknightTable[(int)j];
						break;
					case 3:
						_positionalVal += ENDbishopTable[(int)j];
						break;
					case 4:
						_positionalVal += ENDrookTable[(int)j];
						break;
					case 5:
						_positionalVal += ENDqueenTable[(int)j];
						break;
					case 6:
						_positionalVal += ENDkingTable[(int)j];
						Program.wkPos = j;
						break;
					case 9:
						_positionalVal -= ENDpawnTable[(int)(63 - j)];
						break;
					case 10:
						_positionalVal -= ENDknightTable[(int)(63 - j)];
						break;
					case 11:
						_positionalVal -= ENDbishopTable[(int)(63 - j)];
						break;
					case 12:
						_positionalVal -= ENDrookTable[(int)(63 - j)];
						break;
					case 13:
						_positionalVal -= ENDqueenTable[(int)(63 - j)];
						break;
					case 14:
						_positionalVal -= ENDkingTable[(int)(63 - j)];
						Program.bkPos = j;
						break;
					}
					_positionalVal = _positionalVal;
				}
			}
		}
		if (_isWhite)
		{
			bool flag4 = Program.IsKingInCheck(_board, 64, false);
			if (flag4)
			{
				_enemyInCheckVal += 150;
			}
			else
			{
				bool flag5 = Program.IsKingInCheck(_board, 64, true);
				if (flag5)
				{
					_enemyInCheckVal -= 150;
				}
			}
		}
		int _kingSafetyVal = Program.CalculateKingSafety(_board);
		int _openFileVal = Program.CalculateOpenFiles(_board, _isWhite);
		int _centerControlVal = Program.CalculateCenterControl(_board);
		int _pawnStructureVal = Program.CalculatePawnStructure(_board);
		int _evaluation = _materialVal + _positionalVal + _openFileVal + _centerControlVal + _pawnStructureVal + _enemyInCheckVal;
		if (_writeInfo)
		{
			Console.Write(string.Concat(new string[]
			{
				"Material: ",
				_materialVal.ToString(),
				" + positional: ",
				_positionalVal.ToString(),
				" + open file: ",
				_openFileVal.ToString()
			}));
			Console.Write(string.Concat(new string[]
			{
				"\n\t\t\t+ king safety: ",
				_kingSafetyVal.ToString(),
				" + enemy in check: ",
				_enemyInCheckVal.ToString(),
				" +\n\t\t\t"
			}));
			Console.Write("center control: " + _centerControlVal.ToString() + " + pawn structure: " + _pawnStructureVal.ToString());
		}
		return _evaluation;
	}

	// Token: 0x0600000B RID: 11 RVA: 0x00003634 File Offset: 0x00001834
	private static int CalculateOpenFiles(byte[] _board, bool _isWhite)
	{
		int _totalBonus = 0;
		for (int x = 0; x < 8; x++)
		{
			bool _hasPawn = false;
			for (int y = 0; y < 8; y++)
			{
				byte _piece = _board[y * 8 + x];
				bool flag = (_isWhite && _piece == 1) || (!_isWhite && _piece == 9);
				if (flag)
				{
					_hasPawn = true;
					break;
				}
			}
			bool flag2 = !_hasPawn;
			if (flag2)
			{
				for (int y2 = 0; y2 < 8; y2++)
				{
					byte _piece2 = _board[y2 * 8 + x];
					byte b = _piece2;
					byte b2 = b;
					if (b2 <= 5)
					{
						if (b2 != 4)
						{
							if (b2 == 5)
							{
								_totalBonus += (_isWhite ? 25 : -25);
								_hasPawn = true;
							}
						}
						else
						{
							_totalBonus += (_isWhite ? 30 : -30);
							_hasPawn = true;
						}
					}
					else if (b2 != 12)
					{
						if (b2 == 13)
						{
							_totalBonus += (_isWhite ? -25 : 25);
							_hasPawn = true;
						}
					}
					else
					{
						_totalBonus += (_isWhite ? -30 : 30);
						_hasPawn = true;
					}
				}
				bool flag3 = !_hasPawn;
				if (flag3)
				{
					_totalBonus += (_isWhite ? -5 : 5);
				}
			}
		}
		return _totalBonus;
	}

	// Token: 0x0600000C RID: 12 RVA: 0x0000375C File Offset: 0x0000195C
	private static int CalculateMaterial(byte[] _board, ref int _whiteLargePiecesVal, ref int _blackLargePiecesVal)
	{
		int _materialEval = 0;
		for (int i = 0; i < 64; i++)
		{
			switch (_board[i])
			{
			case 1:
				_materialEval += 101;
				break;
			case 2:
				_materialEval += 300;
				_whiteLargePiecesVal += 300;
				break;
			case 3:
				_materialEval += 321;
				_whiteLargePiecesVal += 321;
				break;
			case 4:
				_materialEval += 500;
				_whiteLargePiecesVal += 500;
				break;
			case 5:
				_materialEval += 915;
				_whiteLargePiecesVal += 915;
				break;
			case 6:
				_materialEval += 10000;
				break;
			case 9:
				_materialEval -= 101;
				break;
			case 10:
				_materialEval -= 300;
				_blackLargePiecesVal -= 300;
				break;
			case 11:
				_materialEval -= 321;
				_blackLargePiecesVal -= 321;
				break;
			case 12:
				_materialEval -= 500;
				_blackLargePiecesVal -= 500;
				break;
			case 13:
				_materialEval -= 915;
				_blackLargePiecesVal -= 915;
				break;
			case 14:
				_materialEval -= 10000;
				break;
			}
		}
		return _materialEval;
	}

	// Token: 0x0600000D RID: 13 RVA: 0x000038A8 File Offset: 0x00001AA8
	private static int CalculateCenterControl(byte[] _board)
	{
		int _controlScore = 0;
		byte _piece = _board[27];
		bool flag = _board[35] == 1;
		if (flag)
		{
			bool flag2 = _board[42] == 1;
			if (flag2)
			{
				_controlScore += 15;
			}
			bool flag3 = _board[44] == 3;
			if (flag3)
			{
				_controlScore += 5;
			}
			bool flag4 = _board[45] == 2;
			if (flag4)
			{
				_controlScore += 15;
			}
			else
			{
				bool flag5 = _board[52] == 2;
				if (flag5)
				{
					_controlScore += 10;
				}
			}
			_controlScore += 10;
		}
		bool flag6 = _board[36] == 1;
		if (flag6)
		{
			bool flag7 = _board[45] == 1;
			if (flag7)
			{
				_controlScore += 5;
			}
			bool flag8 = _board[42] == 2;
			if (flag8)
			{
				_controlScore += 5;
			}
			else
			{
				bool flag9 = _board[52] == 2;
				if (flag9)
				{
					_controlScore += 3;
				}
			}
			_controlScore += 5;
		}
		bool flag10 = _board[27] == 9;
		if (flag10)
		{
			bool flag11 = _board[18] == 9;
			if (flag11)
			{
				_controlScore -= 15;
			}
			bool flag12 = _board[20] == 11;
			if (flag12)
			{
				_controlScore -= 5;
			}
			bool flag13 = _board[21] == 10;
			if (flag13)
			{
				_controlScore -= 15;
			}
			else
			{
				bool flag14 = _board[12] == 10;
				if (flag14)
				{
					_controlScore -= 10;
				}
			}
			_controlScore -= 10;
		}
		bool flag15 = _board[36] == 9;
		if (flag15)
		{
			bool flag16 = _board[21] == 9;
			if (flag16)
			{
				_controlScore -= 5;
			}
			bool flag17 = _board[18] == 10;
			if (flag17)
			{
				_controlScore -= 5;
			}
			else
			{
				bool flag18 = _board[11] == 10;
				if (flag18)
				{
					_controlScore -= 3;
				}
			}
			_controlScore -= 5;
		}
		return _controlScore;
	}

	// Token: 0x0600000E RID: 14 RVA: 0x00003A14 File Offset: 0x00001C14
	private static int CalculateKingSafety(byte[] _board)
	{
		int _kingSafety = 0;
		_kingSafety -= (int)Program.CountAttacks(_board, (int)Program.wkPos, true);
		_kingSafety += (int)Program.CountAttacks(_board, (int)Program.bkPos, false);
		return _kingSafety * 40;
	}

	// Token: 0x0600000F RID: 15 RVA: 0x00003A4C File Offset: 0x00001C4C
	private static int CalculatePawnStructure(byte[] _board)
	{
		int whitePawnStructure = 0;
		int blackPawnStructure = 0;
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				int piece = (int)_board[i * 8 + j];
				bool flag = piece == 1;
				if (flag)
				{
					bool flag2 = Program.IsIsolatedPawn(_board, i, j, true);
					if (flag2)
					{
						whitePawnStructure -= 15;
					}
				}
				else
				{
					bool flag3 = piece == 9;
					if (flag3)
					{
						bool flag4 = Program.IsIsolatedPawn(_board, i, j, false);
						if (flag4)
						{
							blackPawnStructure -= 15;
						}
					}
				}
			}
		}
		return whitePawnStructure - blackPawnStructure;
	}

	// Token: 0x06000010 RID: 16 RVA: 0x00003AE0 File Offset: 0x00001CE0
	private static byte CountAttacks(byte[] _board, int _ourPos, bool _weWhite)
	{
		byte _attacks = 0;
		for (byte i = 0; i < 64; i += 1)
		{
			byte _enemy = _board[(int)i];
			bool flag = (_weWhite && _enemy > 8) || (!_weWhite && _enemy < 8);
			if (flag)
			{
				bool flag2 = Program.CanAttack(_board, _enemy, i % 8, i / 8, (byte)(_ourPos % 8), (byte)(_ourPos / 8));
				if (flag2)
				{
					_attacks += 1;
				}
			}
		}
		return _attacks;
	}

	// Token: 0x06000011 RID: 17 RVA: 0x00003B50 File Offset: 0x00001D50
	private static bool IsIsolatedPawn(byte[] _board, int _x, int _y, bool isWhite)
	{
		bool flag = _y > 0 && _y < 7;
		bool result;
		if (flag)
		{
			bool hasLeftPawn;
			bool hasRightPawn;
			if (isWhite)
			{
				hasLeftPawn = (_board[_y * 8 + 9 + _x] == 1);
				hasRightPawn = (_board[_y * 8 + 7 + _x] == 1);
			}
			else
			{
				hasLeftPawn = (_board[_y * 8 - 9 + _x] == 9);
				hasRightPawn = (_board[_y * 8 - 7 + _x] == 9);
			}
			result = (!hasLeftPawn && !hasRightPawn);
		}
		else
		{
			result = true;
		}
		return result;
	}

	// Token: 0x06000012 RID: 18 RVA: 0x00003BC8 File Offset: 0x00001DC8
	private static bool CanAttack(byte[] _board, byte _enemy, byte _fromX, byte _fromY, byte _toX, byte _toY)
	{
		int _distX = Math.Abs((int)(_toX - _fromX));
		int _distY = Math.Abs((int)(_toY - _fromY));
		switch (_enemy)
		{
		case 1:
		case 9:
			return _distX == 1 && _distY == 1;
		case 2:
		case 10:
			return (_distX == 2 && _distY == 1) || (_distX == 1 && _distY == 2);
		case 3:
		case 11:
		{
			bool flag = _distX == _distY;
			return flag && !Program.CheckForDiagonalBlocking(_board, _fromX, _fromY, _toX, _toY);
		}
		case 4:
		case 12:
		{
			bool flag2 = _distX == 0 || _distY == 0;
			return flag2 && !Program.CheckForVerticalBlocking(_board, 8 * _fromY + _fromX, 8 * _toY + _toX) && !Program.CheckForHorizontalBlocking(_board, 8 * _fromY + _fromX, 8 * _toY + _toX);
		}
		case 5:
		case 13:
		{
			bool flag3 = _distX == 0 || _distY == 0 || _distX == _distY;
			return flag3 && (!Program.CheckForVerticalBlocking(_board, 8 * _fromY + _fromX, 8 * _toY + _toX) && !Program.CheckForDiagonalBlocking(_board, _fromX, _fromY, _toX, _toY)) && !Program.CheckForHorizontalBlocking(_board, 8 * _fromY + _fromX, 8 * _toY + _toX);
		}
		}
		return false;
	}

	// Token: 0x06000013 RID: 19 RVA: 0x00003D34 File Offset: 0x00001F34
	private static bool CheckForDiagonalBlocking(byte[] _board, byte _fromX, byte _fromY, byte _toX, byte _toY)
	{
		for (byte y = Math.Min(_fromY, _toY); y < Math.Max(_fromY, _toY); y += 1)
		{
			for (byte x = Math.Min(_fromX, _toX); x < Math.Max(_fromX, _toX); x += 1)
			{
				bool flag = _board[(int)(y * 8 + x)] > 0;
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000014 RID: 20 RVA: 0x00003DA0 File Offset: 0x00001FA0
	private static bool CheckForVerticalBlocking(byte[] _board, byte _from, byte _to)
	{
		bool flag = Math.Abs((int)(_from - _to)) % 8 != 0;
		bool result;
		if (flag)
		{
			result = false;
		}
		else
		{
			for (byte position = Math.Min(_from, _to) + 8; position < Math.Max(_from, _to); position += 8)
			{
				bool flag2 = _board[(int)position] > 0;
				if (flag2)
				{
					return true;
				}
			}
			result = false;
		}
		return result;
	}

	// Token: 0x06000015 RID: 21 RVA: 0x00003DFC File Offset: 0x00001FFC
	private static bool CheckForHorizontalBlocking(byte[] _board, byte _from, byte _to)
	{
		bool flag = Math.Abs((int)(_from - _to)) > 7;
		bool result;
		if (flag)
		{
			result = false;
		}
		else
		{
			for (byte position = Math.Min(_from, _to) + 1; position < Math.Max(_from, _to); position += 1)
			{
				bool flag2 = _board[(int)position] > 0;
				if (flag2)
				{
					return true;
				}
			}
			result = false;
		}
		return result;
	}

	// Token: 0x06000016 RID: 22 RVA: 0x00003E54 File Offset: 0x00002054
	private static List<Program.Move> GenerateAllMoves(byte[] _thisboard, bool _isWhiteTurn)
	{
		List<Program.Move> _moves = new List<Program.Move>();
		for (byte i = 0; i < 64; i += 1)
		{
			byte _piece = _thisboard[(int)i];
			bool flag = (_isWhiteTurn && _piece < 8) || (!_isWhiteTurn && _piece > 8);
			if (flag)
			{
				switch (_piece)
				{
				case 1:
				case 9:
					_moves.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GeneratePawnMoves(_thisboard, i, _isWhiteTurn), _isWhiteTurn));
					break;
				case 2:
				case 10:
					_moves.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GenerateKnightMoves(_thisboard, i, _piece, _isWhiteTurn), _isWhiteTurn));
					break;
				case 3:
				case 11:
					_moves.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GenerateBishopMoves(_thisboard, i, _piece, _isWhiteTurn), _isWhiteTurn));
					break;
				case 4:
				case 12:
					_moves.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GenerateRookMoves(_thisboard, i, _piece, _isWhiteTurn), _isWhiteTurn));
					break;
				case 5:
				case 13:
					_moves.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GenerateQueenMoves(_thisboard, i, _piece, _isWhiteTurn), _isWhiteTurn));
					break;
				case 6:
				case 14:
					_moves.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GenerateKingMoves(_thisboard, i, _piece, _isWhiteTurn), _isWhiteTurn));
					break;
				}
			}
		}
		return _moves;
	}

	// Token: 0x06000017 RID: 23 RVA: 0x00003F90 File Offset: 0x00002190
	private static List<Program.Move> IsMoveLegalNoCheckCriteria(byte[] _board, List<Program.Move> _moves, bool _isKingWhite)
	{
		for (int i = 0; i < _moves.Count; i++)
		{
			bool flag = Program.IsKingInCheck(Program.SimulateMove(_board, _moves[i]), 64, _isKingWhite);
			if (flag)
			{
				_moves.RemoveAt(i);
				i--;
			}
		}
		return _moves;
	}

	// Token: 0x06000018 RID: 24 RVA: 0x00003FE4 File Offset: 0x000021E4
	private static byte[] SimulateMove(byte[] _oldBoard, Program.Move move)
	{
		byte[] _newBoard = (byte[])_oldBoard.Clone();
		_newBoard[(int)move.To] = move.Piece;
		_newBoard[(int)move.From] = 0;
		return _newBoard;
	}

	// Token: 0x06000019 RID: 25 RVA: 0x0000401C File Offset: 0x0000221C
	private static int PositionsAmountTest(byte[] _board, int _depth, bool _isWhiteTurn)
	{
		bool flag = _depth == 0;
		int result;
		if (flag)
		{
			result = 1;
		}
		else
		{
			List<Program.Move> _moves = Program.GenerateAllMoves(_board, _isWhiteTurn);
			int _amountOfPositions = 0;
			foreach (Program.Move _move in _moves)
			{
				_amountOfPositions += Program.PositionsAmountTest(Program.SimulateMove(_board, _move), _depth - 1, !_isWhiteTurn);
			}
			result = _amountOfPositions;
		}
		return result;
	}

	// Token: 0x0600001A RID: 26 RVA: 0x0000409C File Offset: 0x0000229C
	private static bool IsKingInCheck(byte[] _board, byte _kingPos, bool _kingColor)
	{
		bool flag = _kingPos == 64;
		if (flag)
		{
			if (_kingColor)
			{
				for (byte i = 0; i < 64; i += 1)
				{
					bool flag2 = _board[(int)i] == 6;
					if (flag2)
					{
						_kingPos = i;
						i += 64;
					}
				}
			}
			else
			{
				for (byte j = 0; j < 64; j += 1)
				{
					bool flag3 = _board[(int)j] == 14;
					if (flag3)
					{
						_kingPos = j;
						j += 64;
					}
				}
			}
		}
		return Program.CountAttacks(_board, (int)_kingPos, _kingColor) > 0;
	}

	// Token: 0x0600001B RID: 27 RVA: 0x00004140 File Offset: 0x00002340
	private static int AlphaBetaSearch(byte[] _board, int depth, int alpha, int beta, bool maximizingPlayer, out Program.Move bestMove)
	{
		bestMove = null;
		bool flag = depth == 0;
		int result;
		if (flag)
		{
			result = Program.AdvEvaluate(_board, false, false);
		}
		else
		{
			List<Program.Move> moves = Program.GenerateAllMoves(_board, maximizingPlayer);
			if (maximizingPlayer)
			{
				int maxEval = -999999;
				foreach (Program.Move move in moves)
				{
					byte[] newBoard = Program.SimulateMove(_board, move);
					int eval = Program.AlphaBetaEvalSearch(newBoard, depth - 1, alpha, beta, false);
					bool flag2 = eval > maxEval;
					if (flag2)
					{
						maxEval = eval;
						bestMove = move;
					}
					alpha = Math.Max(alpha, eval);
					bool flag3 = beta <= alpha;
					if (flag3)
					{
						Program.gSkippedPositions++;
						break;
					}
				}
				result = maxEval;
			}
			else
			{
				int minEval = 999999;
				foreach (Program.Move move2 in moves)
				{
					byte[] newBoard2 = Program.SimulateMove(_board, move2);
					int eval2 = Program.AlphaBetaEvalSearch(newBoard2, depth - 1, alpha, beta, true);
					bool flag4 = eval2 < minEval;
					if (flag4)
					{
						minEval = eval2;
						bestMove = move2;
					}
					beta = Math.Min(beta, eval2);
					bool flag5 = beta <= alpha;
					if (flag5)
					{
						Program.gSkippedPositions++;
						break;
					}
				}
				result = minEval;
			}
		}
		return result;
	}

	// Token: 0x0600001C RID: 28 RVA: 0x000042C0 File Offset: 0x000024C0
	private static int AlphaBetaEvalSearch(byte[] _board, int depth, int alpha, int beta, bool maximizingPlayer)
	{
		bool flag = depth == 0;
		int result;
		if (flag)
		{
			result = Program.AdvEvaluate(_board, false, false);
		}
		else
		{
			List<Program.Move> moves = Program.GenerateAllMoves(_board, maximizingPlayer);
			if (maximizingPlayer)
			{
				int maxEval = -999999;
				foreach (Program.Move move in moves)
				{
					byte[] newBoard = Program.SimulateMove(_board, move);
					int eval = Program.AlphaBetaEvalSearch(newBoard, depth - 1, alpha, beta, false);
					bool flag2 = eval > maxEval;
					if (flag2)
					{
						maxEval = eval;
					}
					alpha = Math.Max(alpha, eval);
					bool flag3 = beta <= alpha;
					if (flag3)
					{
						Program.gSkippedPositions++;
						break;
					}
				}
				result = maxEval;
			}
			else
			{
				int minEval = 999999;
				foreach (Program.Move move2 in moves)
				{
					byte[] newBoard2 = Program.SimulateMove(_board, move2);
					int eval2 = Program.AlphaBetaEvalSearch(newBoard2, depth - 1, alpha, beta, true);
					bool flag4 = eval2 < minEval;
					if (flag4)
					{
						minEval = eval2;
					}
					beta = Math.Min(beta, eval2);
					bool flag5 = beta <= alpha;
					if (flag5)
					{
						Program.gSkippedPositions++;
						break;
					}
				}
				result = minEval;
			}
		}
		return result;
	}

	// Token: 0x0600001D RID: 29 RVA: 0x00004434 File Offset: 0x00002634
	private static List<Program.Move> GeneratePawnMoves(byte[] _board, byte _position, bool _isWhite)
	{
		List<Program.Move> _generatedMoves = new List<Program.Move>();
		int _direction = _isWhite ? -8 : 8;
		byte _startRow = _isWhite ? 6 : 1;
		byte _promotionRow = _isWhite ? 0 : 7;
		byte _xPos = _position % 8;
		int _newPosition = (int)_position + _direction;
		bool flag = _newPosition >= 0 && _newPosition < 64 && _board[_newPosition] == 0;
		if (flag)
		{
			bool flag2 = _newPosition / 8 == (int)_promotionRow;
			if (flag2)
			{
				_generatedMoves.Insert(0, new Program.Move
				{
					From = _position,
					To = (byte)_newPosition,
					Piece = (_isWhite ? 2 : 10),
					CapturedPiece = 0
				});
				_generatedMoves.Insert(0, new Program.Move
				{
					From = _position,
					To = (byte)_newPosition,
					Piece = (_isWhite ? 3 : 11),
					CapturedPiece = 0
				});
				_generatedMoves.Insert(0, new Program.Move
				{
					From = _position,
					To = (byte)_newPosition,
					Piece = (_isWhite ? 4 : 12),
					CapturedPiece = 0
				});
				_generatedMoves.Insert(0, new Program.Move
				{
					From = _position,
					To = (byte)_newPosition,
					Piece = (_isWhite ? 5 : 13),
					CapturedPiece = 0
				});
			}
			else
			{
				_generatedMoves.Add(new Program.Move
				{
					From = _position,
					To = (byte)_newPosition,
					Piece = (_isWhite ? 1 : 9),
					CapturedPiece = 0
				});
			}
			bool flag3 = _position / 8 == _startRow && _board[_newPosition + _direction] == 0;
			if (flag3)
			{
				_generatedMoves.Add(new Program.Move
				{
					From = _position,
					To = (byte)(_newPosition + _direction),
					Piece = _board[(int)_position],
					CapturedPiece = 0
				});
			}
		}
		bool flag4 = _xPos > 0;
		if (flag4)
		{
			byte _attackPosition = _isWhite ? (_position - 9) : (_position + 7);
			bool flag5 = _attackPosition >= 0 && _attackPosition < 64;
			if (flag5)
			{
				byte _target = _board[(int)_attackPosition];
				bool flag6 = _target != 0 && ((_isWhite && _target > 8) || (!_isWhite && _target < 8));
				if (flag6)
				{
					bool flag7 = _attackPosition / 8 == _promotionRow;
					if (flag7)
					{
						_generatedMoves.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)_newPosition,
							Piece = (_isWhite ? 2 : 10),
							CapturedPiece = 0
						});
						_generatedMoves.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)_newPosition,
							Piece = (_isWhite ? 3 : 11),
							CapturedPiece = 0
						});
						_generatedMoves.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)_newPosition,
							Piece = (_isWhite ? 4 : 12),
							CapturedPiece = 0
						});
						_generatedMoves.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)_newPosition,
							Piece = (_isWhite ? 5 : 13),
							CapturedPiece = 0
						});
					}
					else
					{
						_generatedMoves.Insert(0, new Program.Move
						{
							From = _position,
							To = _attackPosition,
							Piece = (_isWhite ? 1 : 9),
							CapturedPiece = _target
						});
					}
				}
			}
		}
		bool flag8 = _xPos < 7;
		if (flag8)
		{
			byte _attackPosition2 = _isWhite ? (_position - 7) : (_position + 9);
			bool flag9 = _attackPosition2 >= 0 && _attackPosition2 < 64;
			if (flag9)
			{
				byte _target2 = _board[(int)_attackPosition2];
				bool flag10 = _target2 != 0 && ((_isWhite && _target2 > 8) || (!_isWhite && _target2 < 8));
				if (flag10)
				{
					bool flag11 = _attackPosition2 / 8 == _promotionRow;
					if (flag11)
					{
						_generatedMoves.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)_newPosition,
							Piece = (_isWhite ? 2 : 10),
							CapturedPiece = 0
						});
						_generatedMoves.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)_newPosition,
							Piece = (_isWhite ? 3 : 11),
							CapturedPiece = 0
						});
						_generatedMoves.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)_newPosition,
							Piece = (_isWhite ? 4 : 12),
							CapturedPiece = 0
						});
						_generatedMoves.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)_newPosition,
							Piece = (_isWhite ? 5 : 13),
							CapturedPiece = 0
						});
					}
					else
					{
						_generatedMoves.Insert(0, new Program.Move
						{
							From = _position,
							To = _attackPosition2,
							Piece = (_isWhite ? 1 : 9),
							CapturedPiece = _target2
						});
					}
				}
			}
		}
		return _generatedMoves;
	}

	// Token: 0x0600001E RID: 30 RVA: 0x00004918 File Offset: 0x00002B18
	private static List<Program.Move> GenerateKnightMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
	{
		List<Program.Move> _moves = new List<Program.Move>();
		int[] _knightOffsets = new int[]
		{
			-10,
			6,
			-17,
			15,
			-15,
			17,
			-6,
			10
		};
		byte _xPos = _position % 8;
		for (int i = 0; i < 8; i++)
		{
			bool flag = (i < 2 && _xPos > 1) || (i > 1 && i < 4 && _xPos > 0) || (i > 3 && i < 6 && _xPos < 7) || (i > 5 && _xPos < 6);
			if (flag)
			{
				int _newPosition = (int)_position + _knightOffsets[i];
				bool flag2 = _newPosition >= 0 && _newPosition < 64;
				if (flag2)
				{
					byte _target = _board[_newPosition];
					bool flag3 = _target == 0;
					if (flag3)
					{
						_moves.Add(new Program.Move
						{
							From = _position,
							To = (byte)_newPosition,
							Piece = _piece,
							CapturedPiece = 0
						});
					}
					else
					{
						bool flag4 = _isWhite && _target > 8;
						if (flag4)
						{
							_moves.Insert(0, new Program.Move
							{
								From = _position,
								To = (byte)_newPosition,
								Piece = _piece,
								CapturedPiece = _target
							});
						}
						else
						{
							bool flag5 = !_isWhite && _target < 8;
							if (flag5)
							{
								_moves.Insert(0, new Program.Move
								{
									From = _position,
									To = (byte)_newPosition,
									Piece = _piece,
									CapturedPiece = _target
								});
							}
						}
					}
				}
			}
		}
		return _moves;
	}

	// Token: 0x0600001F RID: 31 RVA: 0x00004A8C File Offset: 0x00002C8C
	private static List<Program.Move> GenerateBishopMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
	{
		List<Program.Move> _generatedMoves = new List<Program.Move>();
		int[] _bishopOffsets = new int[]
		{
			-9,
			7,
			-7,
			9
		};
		for (int i = 0; i < 4; i++)
		{
			byte _xPos = _position % 8;
			bool flag = (i < 2 && _xPos > 0) || (i > 1 && _xPos < 7);
			if (flag)
			{
				int _newPosition = (int)_position + _bishopOffsets[i];
				while (_newPosition >= 0 && _newPosition < 64)
				{
					byte _target = _board[_newPosition];
					bool flag2 = _target == 0;
					if (!flag2)
					{
						bool flag3 = (_isWhite && _target > 8) || (!_isWhite && _target < 8);
						if (flag3)
						{
							_generatedMoves.Insert(0, new Program.Move
							{
								From = _position,
								To = (byte)_newPosition,
								Piece = _piece,
								CapturedPiece = _target
							});
						}
						break;
					}
					_generatedMoves.Add(new Program.Move
					{
						From = _position,
						To = (byte)_newPosition,
						Piece = _piece,
						CapturedPiece = 0
					});
					_xPos = (byte)(_newPosition % 8);
					bool flag4 = (i < 2 && _xPos > 0) || (i > 1 && _xPos < 7);
					if (flag4)
					{
						_newPosition += _bishopOffsets[i];
					}
					else
					{
						_newPosition = 64;
					}
				}
			}
		}
		return _generatedMoves;
	}

	// Token: 0x06000020 RID: 32 RVA: 0x00004BE4 File Offset: 0x00002DE4
	private static List<Program.Move> GenerateRookMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
	{
		List<Program.Move> _generatedMoves = new List<Program.Move>();
		int[] _rookOffsets = new int[]
		{
			-1,
			-8,
			8,
			1
		};
		byte _xPos = _position % 8;
		for (int i = 0; i < 4; i++)
		{
			bool flag = (i < 1 && _xPos > 0) || i == 1 || i == 2 || (i > 2 && _xPos < 7);
			if (flag)
			{
				int _newPosition = (int)_position + _rookOffsets[i];
				while (_newPosition >= 0 && _newPosition < 64)
				{
					byte _target = _board[_newPosition];
					bool flag2 = _target == 0;
					if (!flag2)
					{
						bool flag3 = (_isWhite && _target > 8) || (!_isWhite && _target < 8);
						if (flag3)
						{
							_generatedMoves.Insert(0, new Program.Move
							{
								From = _position,
								To = (byte)_newPosition,
								Piece = _piece,
								CapturedPiece = _target
							});
						}
						break;
					}
					_generatedMoves.Add(new Program.Move
					{
						From = _position,
						To = (byte)_newPosition,
						Piece = _piece,
						CapturedPiece = 0
					});
					_xPos = (byte)(_newPosition % 8);
					bool flag4 = (i < 1 && _xPos > 0) || i == 1 || i == 2 || (i > 2 && _xPos < 7);
					if (flag4)
					{
						_newPosition += _rookOffsets[i];
					}
					else
					{
						_newPosition = 64;
					}
				}
			}
		}
		return _generatedMoves;
	}

	// Token: 0x06000021 RID: 33 RVA: 0x00004D4C File Offset: 0x00002F4C
	private static List<Program.Move> GenerateQueenMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
	{
		List<Program.Move> _generatedMoves = new List<Program.Move>();
		_generatedMoves = Program.GenerateBishopMoves(_board, _position, _piece, _isWhite);
		_generatedMoves.AddRange(Program.GenerateRookMoves(_board, _position, _piece, _isWhite));
		return _generatedMoves;
	}

	// Token: 0x06000022 RID: 34 RVA: 0x00004D80 File Offset: 0x00002F80
	private static List<Program.Move> GenerateKingMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
	{
		List<Program.Move> _generatedMoves = new List<Program.Move>();
		int[] _kingOffsets = new int[]
		{
			-9,
			-1,
			7,
			-8,
			8,
			-7,
			1,
			9
		};
		byte _xPos = _position % 8;
		for (int i = 0; i < 8; i++)
		{
			bool flag = (i < 3 && _xPos > 0) || (i > 4 && i < 8 && _xPos < 7) || i == 3 || i == 4;
			if (flag)
			{
				int _newPosition = (int)_position + _kingOffsets[i];
				bool flag2 = _newPosition >= 0 && _newPosition < 64;
				if (flag2)
				{
					byte _target = _board[_newPosition];
					bool flag3 = _target == 0;
					if (flag3)
					{
						_generatedMoves.Add(new Program.Move
						{
							From = _position,
							To = (byte)_newPosition,
							Piece = _piece,
							CapturedPiece = _target
						});
					}
					else
					{
						bool flag4 = _isWhite && _target > 8;
						if (flag4)
						{
							_generatedMoves.Insert(0, new Program.Move
							{
								From = _position,
								To = (byte)_newPosition,
								Piece = _piece,
								CapturedPiece = _target
							});
						}
						else
						{
							bool flag5 = !_isWhite && _target < 8;
							if (flag5)
							{
								_generatedMoves.Insert(0, new Program.Move
								{
									From = _position,
									To = (byte)_newPosition,
									Piece = _piece,
									CapturedPiece = _target
								});
							}
						}
					}
				}
			}
		}
		return _generatedMoves;
	}

	// Token: 0x06000023 RID: 35 RVA: 0x00004EE8 File Offset: 0x000030E8
	private static void ApplyMove(byte[] _board, Program.Move move)
	{
		bool flag = move == null;
		if (flag)
		{
			Program.gCheckMate += 1;
		}
		else
		{
			bool flag2 = _board[(int)move.To] > 0;
			if (flag2)
			{
				Console.Write("\n\n\n\t\t\t   [!]  - ");
				switch (_board[(int)move.To])
				{
				case 1:
					Console.Write("White pawn");
					break;
				case 2:
					Console.Write("White knight");
					break;
				case 3:
					Console.Write("White bishop");
					break;
				case 4:
					Console.Write("White rook");
					break;
				case 5:
					Console.Write("White queen");
					break;
				case 6:
					Console.Write("Opps, seems like the white king was captured");
					break;
				case 9:
					Console.Write("Black pawn");
					break;
				case 10:
					Console.Write("Black knight");
					break;
				case 11:
					Console.Write("Black bishop");
					break;
				case 12:
					Console.Write("Black rook");
					break;
				case 13:
					Console.Write("Black queen");
					break;
				case 14:
					Console.Write("Opps, seems like the black king was captured");
					break;
				}
				Console.Write(" was taken");
				Program.gPieceGotEaten = true;
			}
			_board[(int)move.To] = move.Piece;
			_board[(int)move.From] = 0;
			bool flag3 = move.Piece == 6;
			if (flag3)
			{
				Program.wkPos = move.To;
			}
			else
			{
				bool flag4 = move.Piece == 14;
				if (flag4)
				{
					Program.bkPos = move.To;
				}
			}
		}
	}

	// Token: 0x06000024 RID: 36 RVA: 0x00005084 File Offset: 0x00003284
	private static bool GetTurn()
	{
		Console.Write("\n\tWhose turn? (W / Y / YES / 1  = player white turn): ");
		string _userInput = Console.ReadLine().Trim().ToLower();
		return _userInput == "w" || _userInput == "y" || _userInput == "yes" || _userInput == "1";
	}

	// Token: 0x06000025 RID: 37 RVA: 0x000050F8 File Offset: 0x000032F8
	private static int GetDepth()
	{
		int _depth = -1;
		Console.Write("\tEnter the depth for the algorithm search (only from 1 to 6): ");
		while (_depth < 1 || _depth > 6)
		{
			string _userInput = Console.ReadLine();
			Console.Clear();
			bool flag = !int.TryParse(_userInput, out _depth);
			if (flag)
			{
				Console.Write("\tInvalid input, please try again: ");
			}
			else
			{
				bool flag2 = _depth < 1 || _depth > 6;
				if (flag2)
				{
					Console.Write("\tOut of bounds, please enter a valid number from the interval: ");
				}
			}
		}
		Console.Write("\n");
		return _depth;
	}

	// Token: 0x06000026 RID: 38 RVA: 0x00005184 File Offset: 0x00003384
	private static bool GetBoardDisplayType()
	{
		Console.Write("\n\tDisplay chess unicode pieces? (Yes/Y/1/Да = display unicode, else = numbers): ");
		string _userInput = Console.ReadLine().ToLower().Replace(" ", "");
		return _userInput == "yes" || _userInput == "y" || _userInput == "1" || _userInput == "да";
	}

	// Token: 0x04000001 RID: 1
	private const byte emptySquare = 0;

	// Token: 0x04000002 RID: 2
	private const byte decodingError = 8;

	// Token: 0x04000003 RID: 3
	private const byte wp1 = 1;

	// Token: 0x04000004 RID: 4
	private const byte wn1 = 2;

	// Token: 0x04000005 RID: 5
	private const byte wb1 = 3;

	// Token: 0x04000006 RID: 6
	private const byte wr1 = 4;

	// Token: 0x04000007 RID: 7
	private const byte wq1 = 5;

	// Token: 0x04000008 RID: 8
	private const byte wk1 = 6;

	// Token: 0x04000009 RID: 9
	private const byte wp2 = 16;

	// Token: 0x0400000A RID: 10
	private const byte wn2 = 32;

	// Token: 0x0400000B RID: 11
	private const byte wb2 = 48;

	// Token: 0x0400000C RID: 12
	private const byte wr2 = 64;

	// Token: 0x0400000D RID: 13
	private const byte wq2 = 80;

	// Token: 0x0400000E RID: 14
	private const byte wk2 = 96;

	// Token: 0x0400000F RID: 15
	private const byte bp1 = 9;

	// Token: 0x04000010 RID: 16
	private const byte bn1 = 10;

	// Token: 0x04000011 RID: 17
	private const byte bb1 = 11;

	// Token: 0x04000012 RID: 18
	private const byte br1 = 12;

	// Token: 0x04000013 RID: 19
	private const byte bq1 = 13;

	// Token: 0x04000014 RID: 20
	private const byte bk1 = 14;

	// Token: 0x04000015 RID: 21
	private const byte bp2 = 144;

	// Token: 0x04000016 RID: 22
	private const byte bn2 = 160;

	// Token: 0x04000017 RID: 23
	private const byte bb2 = 176;

	// Token: 0x04000018 RID: 24
	private const byte br2 = 192;

	// Token: 0x04000019 RID: 25
	private const byte bq2 = 208;

	// Token: 0x0400001A RID: 26
	private const byte bk2 = 224;

	// Token: 0x0400001B RID: 27
	private const int pVal = 101;

	// Token: 0x0400001C RID: 28
	private const int nVal = 300;

	// Token: 0x0400001D RID: 29
	private const int bVal = 321;

	// Token: 0x0400001E RID: 30
	private const int rVal = 500;

	// Token: 0x0400001F RID: 31
	private const int qVal = 915;

	// Token: 0x04000020 RID: 32
	private const int kVal = 10000;

	// Token: 0x04000021 RID: 33
	private const int rookOpenFileBonus = 30;

	// Token: 0x04000022 RID: 34
	private const int queenOpenFileBonus = 25;

	// Token: 0x04000023 RID: 35
	private const int openFileNotUsedPunishment = -5;

	// Token: 0x04000024 RID: 36
	private const int centerControlPriority = 1;

	// Token: 0x04000025 RID: 37
	private const int enemyInCheckPriority = 150;

	// Token: 0x04000026 RID: 38
	private const int piecePositionPriority = 1;

	// Token: 0x04000027 RID: 39
	private const int kingSafetyPriority = 40;

	// Token: 0x04000028 RID: 40
	private const int pieceActivityPriority = 5;

	// Token: 0x04000029 RID: 41
	private const int pawnStructurePriority = 15;

	// Token: 0x0400002A RID: 42
	private static readonly byte[] mainBoard = new byte[64];

	// Token: 0x0400002B RID: 43
	private static byte wkPos;

	// Token: 0x0400002C RID: 44
	private static byte bkPos;

	// Token: 0x0400002D RID: 45
	private static bool whiteTurn;

	// Token: 0x0400002E RID: 46
	private static bool gPieceGotEaten = false;

	// Token: 0x0400002F RID: 47
	private static byte gCheckMate;

	// Token: 0x04000030 RID: 48
	private static int gSkippedPositions;

	// Token: 0x02000004 RID: 4
	private class Move
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000029 RID: 41 RVA: 0x00005216 File Offset: 0x00003416
		// (set) Token: 0x0600002A RID: 42 RVA: 0x0000521E File Offset: 0x0000341E
		public byte From { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600002B RID: 43 RVA: 0x00005227 File Offset: 0x00003427
		// (set) Token: 0x0600002C RID: 44 RVA: 0x0000522F File Offset: 0x0000342F
		public byte To { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600002D RID: 45 RVA: 0x00005238 File Offset: 0x00003438
		// (set) Token: 0x0600002E RID: 46 RVA: 0x00005240 File Offset: 0x00003440
		public byte Piece { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600002F RID: 47 RVA: 0x00005249 File Offset: 0x00003449
		// (set) Token: 0x06000030 RID: 48 RVA: 0x00005251 File Offset: 0x00003451
		public byte CapturedPiece { get; set; }
	}
}
