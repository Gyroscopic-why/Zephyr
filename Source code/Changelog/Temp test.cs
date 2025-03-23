using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

// Token: 0x02000002 RID: 2
internal class Program
{
	// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	private static void Main(string[] args)
	{
		Console.OutputEncoding = Encoding.Unicode;
		Console.Title = "Zephyr engine Eta";
		string a = "";
		Program.Move move = null;
		while (a != "exit")
		{
			Program.gCheckMate = 0;
			int depth = Program.GetDepth();
			bool boardDisplayType = Program.GetBoardDisplayType();
			Program.GetEncodedBoard(false);
			Program.whiteTurn = Program.GetTurn();
			Console.Clear();
			Program.DisplayBoard(Program.mainBoard, boardDisplayType, 64, 64);
			int num = Program.AdvEvaluate(Program.mainBoard, true, true);
			Console.Write(string.Format("\n\t\t\t\tCurrent eval: {0}\n\n\n\t", num));
			Console.Write("White king in check: " + Program.IsKingInCheck(Program.mainBoard, Program.wkPos, true).ToString() + ", wkPos: " + Program.wkPos.ToString());
			Console.Write("\n\tBlack king in check: " + Program.IsKingInCheck(Program.mainBoard, Program.bkPos, false).ToString() + ", bkPos: " + Program.bkPos.ToString());
			Console.Write("\n\n\tContinue?  (press ENTER): ");
			a = Console.ReadLine();
			int minValue = int.MinValue;
			int maxValue = int.MaxValue;
			while (a == "")
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				bool flag = Program.gCheckMate < 3;
				if (flag)
				{
					Program.AlphaBetaSearch(Program.mainBoard, depth, minValue, maxValue, Program.whiteTurn, out move);
					Program.ApplyMove(Program.mainBoard, move);
					Program.whiteTurn = !Program.whiteTurn;
					Console.Clear();
				}
				else
				{
					Console.Clear();
					Console.Write("\t\tCHECKMATE  :D");
				}
				stopwatch.Stop();
				bool flag2 = move != null;
				if (flag2)
				{
					Program.DisplayBoard(Program.mainBoard, boardDisplayType, move.From, move.To);
				}
				else
				{
					Program.DisplayBoard(Program.mainBoard, boardDisplayType, 64, 64);
				}
				num = Program.AdvEvaluate(Program.mainBoard, true, true);
				Console.Write(string.Format("\n\t\t\t\tCurrent eval: {0}\n\n\n\t", num));
				Console.Write(string.Concat(new string[]
				{
					"\n\tAlpha-beta skipped: ",
					Program.gSkippedPositions.ToString(),
					", time elapsed: ",
					stopwatch.ElapsedMilliseconds.ToString(),
					" ms\n\t"
				}));
				Console.Write("White king in check: " + Program.IsKingInCheck(Program.mainBoard, Program.wkPos, true).ToString() + ", wkPos: " + Program.wkPos.ToString());
				Console.Write("\n\tBlack king in check: " + Program.IsKingInCheck(Program.mainBoard, Program.bkPos, false).ToString() + ", bkPos: " + Program.bkPos.ToString());
				Console.Write("\n\n\tContinue?  (press ENTER): ");
				a = Console.ReadLine().Trim().ToLower();
			}
			Program.EncodeBoard(Program.mainBoard);
			bool flag3 = a != "exit";
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
		bool flag = false;
		bool flag2 = false;
		while (!flag)
		{
			string text = "";
			Console.Write("\n\tEnter the board (64 characters): (SP: 0 / rnbqkbnrpppppppp32PPPPPPPPRNBQKBNR)\n\t");
			while (text.Length < 64 && !flag && !flag2)
			{
				string text2 = Console.ReadLine().Replace(" ", "");
				text += text2;
				bool flag3 = text2 == "0";
				if (flag3)
				{
					text = "rnbqkbnrpppppppp32PPPPPPPPRNBQKBNR";
				}
				bool flag4 = text.Length <= 64 && text.Length > 0;
				if (flag4)
				{
					bool flag5 = Program.TryParseClassicBoard(ref text);
					if (flag5)
					{
						flag = true;
					}
					else
					{
						bool flag6 = text[text.Length - 1] == '#';
						if (flag6)
						{
							Console.Clear();
							Console.Write("\t[!]  - Error while parsing board: to many characters\n");
							flag2 = true;
						}
						else
						{
							bool flag7 = text[text.Length - 1] == '!';
							if (flag7)
							{
								Console.Clear();
								Console.Write("\t[!]  - Error while parsing board: unknown character\n");
								flag2 = true;
							}
						}
					}
				}
				Console.Write("\t");
			}
			bool flag8 = text.Length == 64;
			if (flag8)
			{
				if (!_type)
				{
					bool flag9 = Program.TryParseClassicBoard(ref text);
					if (flag9)
					{
						flag = true;
					}
					else
					{
						Console.Clear();
						Console.Write("\t[!]  - Error while parsing board: unknown character\n");
					}
				}
			}
		}
		return flag;
	}

	// Token: 0x06000003 RID: 3 RVA: 0x000024E8 File Offset: 0x000006E8
	private static bool TryParseOptimisedBoard(string _encoded, byte[] _board)
	{
		byte b = 0;
		while (b < 32)
		{
			byte b2 = Program.ConvertBoardToBytes(_encoded[(int)b], true);
			bool flag = b2 == 8;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				_board[(int)b] = b2;
				bool flag2 = _board[(int)b] == 6;
				if (flag2)
				{
					Program.wkPos = b;
				}
				bool flag3 = _board[(int)b] == 14;
				if (flag3)
				{
					Program.bkPos = b;
				}
				b2 = Program.ConvertBoardToBytes(_encoded[(int)b], false);
				bool flag4 = b2 == 8;
				if (!flag4)
				{
					byte b3 = b;
					_board[(int)b3] = _board[(int)b3] + b2;
					bool flag5 = _board[(int)b] == 6;
					if (flag5)
					{
						Program.wkPos = b;
					}
					bool flag6 = _board[(int)b] == 14;
					if (flag6)
					{
						Program.bkPos = b;
					}
					b += 1;
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
		byte b = 0;
		byte b2 = 0;
		while ((int)b2 < _encoded.Length)
		{
			bool flag = b2 + b < 64;
			if (flag)
			{
				Program.mainBoard[(int)(b2 + b)] = Program.ConvertBoardToBytes(_encoded[(int)b2], true);
				bool flag2 = Program.mainBoard[(int)(b2 + b)] == 8;
				if (flag2)
				{
					string text = "";
					bool flag3 = (int)b2 < _encoded.Length - 1;
					if (flag3)
					{
						text += _encoded[(int)b2].ToString();
						text += _encoded[(int)(b2 + 1)].ToString();
					}
					byte b3;
					bool flag4 = byte.TryParse(text, out b3);
					if (flag4)
					{
						for (byte b4 = b2; b4 < b3 + b2; b4 += 1)
						{
							bool flag5 = b2 + b < 64;
							if (flag5)
							{
								Program.mainBoard[(int)(b2 + b)] = 0;
								b += 1;
							}
							else
							{
								b4 += b3;
								_encoded += "#";
							}
						}
						b -= 2;
						b2 += 1;
					}
					else
					{
						bool flag6 = byte.TryParse(_encoded[(int)b2].ToString(), out b3);
						if (!flag6)
						{
							_encoded += "!";
							return false;
						}
						for (int i = (int)b2; i < (int)(b3 + b2); i++)
						{
							bool flag7 = b2 + b < 64;
							if (flag7)
							{
								Program.mainBoard[(int)(b2 + b)] = 0;
								b += 1;
							}
							else
							{
								i += (int)b3;
								_encoded += "#";
							}
						}
						b -= 1;
					}
				}
				bool flag8 = Program.mainBoard[(int)(b2 + b)] == 6;
				if (flag8)
				{
					Program.wkPos = b2 + b;
				}
				bool flag9 = Program.mainBoard[(int)(b2 + b)] == 14;
				if (flag9)
				{
					Program.bkPos = b2 + b;
				}
			}
			else
			{
				b2 += 128;
				_encoded += "#";
			}
			b2 += 1;
		}
		bool flag10 = _encoded.Length + (int)b != 64;
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
		Console.Write("\t\t\t\tParsed board (in bytes): " + _board.Length.ToString() + "\n\n\n\t\t\t\t  ");
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

	// Token: 0x06000008 RID: 8 RVA: 0x00003030 File Offset: 0x00001230
	private static void EncodeBoard(byte[] _board)
	{
		string text = "";
		string text2 = "";
		byte b = 0;
		bool flag = _board.Length == 64;
		if (flag)
		{
			for (byte b2 = 0; b2 < 64; b2 += 1)
			{
				text += Program.ConvertBoardToChars(_board[(int)b2], true).ToString();
				bool flag2 = text[(int)b2] != '+';
				if (flag2)
				{
					bool flag3 = b > 0;
					if (flag3)
					{
						text2 += b.ToString();
						b = 0;
					}
					text2 += text[(int)b2].ToString();
				}
				else
				{
					b += 1;
				}
			}
			bool flag4 = b > 0;
			if (flag4)
			{
				text2 += b.ToString();
			}
		}
		text = text.Replace("++++++++", "+++++++/");
		Program.PrintEncodedBoard(text, text2);
	}

	// Token: 0x06000009 RID: 9 RVA: 0x0000311E File Offset: 0x0000131E
	private static void PrintEncodedBoard(string _encodedBoard, string _boardFen)
	{
		Console.Write("\n\tEncoded board code: " + _encodedBoard);
		Console.Write("\n\tCustom fen board code: " + _boardFen + "\n\n\n");
	}

	// Token: 0x0600000A RID: 10 RVA: 0x00003148 File Offset: 0x00001348
	private static int AdvEvaluate(byte[] _board, bool _isWhite, bool _writeInfo = false)
	{
		int[] array = new int[]
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
		int[] array2 = new int[]
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
		int[] array3 = new int[]
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
		int[] array4 = new int[]
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
		int[] array5 = new int[]
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
		int[] array6 = new int[]
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
		int[] array7 = new int[]
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
		int[] array8 = new int[]
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
		int[] array9 = new int[]
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
		int[] array10 = new int[]
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
		int[] array11 = new int[]
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
		int[] array12 = new int[]
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
		int val = 0;
		int val2 = 0;
		int num = Program.CalculateMaterial(_board, ref val2, ref val);
		int num2 = 0;
		int num3 = 0;
		bool flag = Math.Max(val2, val) > 1700;
		if (flag)
		{
			for (byte b = 0; b < 64; b += 1)
			{
				byte b2 = _board[(int)b];
				bool flag2 = b2 == 0;
				if (!flag2)
				{
					switch (b2)
					{
					case 1:
						num2 += array[(int)b];
						break;
					case 2:
						num2 += array2[(int)b];
						break;
					case 3:
						num2 += array3[(int)b];
						break;
					case 4:
						num2 += array4[(int)b];
						break;
					case 5:
						num2 += array5[(int)b];
						break;
					case 6:
						num2 += array6[(int)b];
						Program.wkPos = b;
						break;
					case 9:
						num2 -= array[(int)(63 - b)];
						break;
					case 10:
						num2 -= array2[(int)(63 - b)];
						break;
					case 11:
						num2 -= array3[(int)(63 - b)];
						break;
					case 12:
						num2 -= array4[(int)(63 - b)];
						break;
					case 13:
						num2 -= array5[(int)(63 - b)];
						break;
					case 14:
						num2 -= array6[(int)(63 - b)];
						Program.bkPos = b;
						break;
					}
					num2 = num2;
				}
			}
		}
		else
		{
			for (byte b3 = 0; b3 < 64; b3 += 1)
			{
				byte b4 = _board[(int)b3];
				bool flag3 = b4 == 0;
				if (!flag3)
				{
					switch (b4)
					{
					case 1:
						num2 += array7[(int)b3];
						break;
					case 2:
						num2 += array8[(int)b3];
						break;
					case 3:
						num2 += array9[(int)b3];
						break;
					case 4:
						num2 += array10[(int)b3];
						break;
					case 5:
						num2 += array11[(int)b3];
						break;
					case 6:
						num2 += array12[(int)b3];
						Program.wkPos = b3;
						break;
					case 9:
						num2 -= array7[(int)(63 - b3)];
						break;
					case 10:
						num2 -= array8[(int)(63 - b3)];
						break;
					case 11:
						num2 -= array9[(int)(63 - b3)];
						break;
					case 12:
						num2 -= array10[(int)(63 - b3)];
						break;
					case 13:
						num2 -= array11[(int)(63 - b3)];
						break;
					case 14:
						num2 -= array12[(int)(63 - b3)];
						Program.bkPos = b3;
						break;
					}
					num2 = num2;
				}
			}
		}
		if (_isWhite)
		{
			bool flag4 = Program.IsKingInCheck(_board, 64, false);
			if (flag4)
			{
				num3 += 150;
			}
			else
			{
				bool flag5 = Program.IsKingInCheck(_board, 64, true);
				if (flag5)
				{
					num3 -= 150;
				}
			}
		}
		int num4 = Program.CalculateKingSafety(_board);
		int num5 = Program.CalculateOpenFiles(_board, _isWhite);
		int num6 = Program.CalculateCenterControl(_board);
		int num7 = Program.CalculatePawnStructure(_board);
		int result = num + num2 + num5 + num6 + num7 + num3;
		if (_writeInfo)
		{
			Console.Write(string.Concat(new string[]
			{
				"Material: ",
				num.ToString(),
				" + positional: ",
				num2.ToString(),
				" + open file: ",
				num5.ToString()
			}));
			Console.Write(string.Concat(new string[]
			{
				"\n\t\t\t+ king safety: ",
				num4.ToString(),
				" + enemy in check: ",
				num3.ToString(),
				" +\n\t\t\t"
			}));
			Console.Write("center control: " + num6.ToString() + " + pawn structure: " + num7.ToString());
		}
		return result;
	}

	// Token: 0x0600000B RID: 11 RVA: 0x00003628 File Offset: 0x00001828
	private static int CalculateOpenFiles(byte[] _board, bool _isWhite)
	{
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			bool flag = false;
			for (int j = 0; j < 8; j++)
			{
				byte b = _board[j * 8 + i];
				bool flag2 = (_isWhite && b == 1) || (!_isWhite && b == 9);
				if (flag2)
				{
					flag = true;
					break;
				}
			}
			bool flag3 = !flag;
			if (flag3)
			{
				for (int k = 0; k < 8; k++)
				{
					byte b2 = _board[k * 8 + i];
					byte b3 = b2;
					byte b4 = b3;
					if (b4 <= 5)
					{
						if (b4 != 4)
						{
							if (b4 == 5)
							{
								num += (_isWhite ? 25 : -25);
								flag = true;
							}
						}
						else
						{
							num += (_isWhite ? 30 : -30);
							flag = true;
						}
					}
					else if (b4 != 12)
					{
						if (b4 == 13)
						{
							num += (_isWhite ? -25 : 25);
							flag = true;
						}
					}
					else
					{
						num += (_isWhite ? -30 : 30);
						flag = true;
					}
				}
				bool flag4 = !flag;
				if (flag4)
				{
					num += (_isWhite ? -5 : 5);
				}
			}
		}
		return num;
	}

	// Token: 0x0600000C RID: 12 RVA: 0x00003750 File Offset: 0x00001950
	private static int CalculateMaterial(byte[] _board, ref int _whiteLargePiecesVal, ref int _blackLargePiecesVal)
	{
		int num = 0;
		for (int i = 0; i < 64; i++)
		{
			switch (_board[i])
			{
			case 1:
				num += 101;
				break;
			case 2:
				num += 300;
				_whiteLargePiecesVal += 300;
				break;
			case 3:
				num += 321;
				_whiteLargePiecesVal += 321;
				break;
			case 4:
				num += 500;
				_whiteLargePiecesVal += 500;
				break;
			case 5:
				num += 915;
				_whiteLargePiecesVal += 915;
				break;
			case 6:
				num += 10000;
				break;
			case 9:
				num -= 101;
				break;
			case 10:
				num -= 300;
				_blackLargePiecesVal -= 300;
				break;
			case 11:
				num -= 321;
				_blackLargePiecesVal -= 321;
				break;
			case 12:
				num -= 500;
				_blackLargePiecesVal -= 500;
				break;
			case 13:
				num -= 915;
				_blackLargePiecesVal -= 915;
				break;
			case 14:
				num -= 10000;
				break;
			}
		}
		return num;
	}

	// Token: 0x0600000D RID: 13 RVA: 0x0000389C File Offset: 0x00001A9C
	private static int CalculateCenterControl(byte[] _board)
	{
		int num = 0;
		byte b = _board[27];
		bool flag = _board[35] == 1;
		if (flag)
		{
			bool flag2 = _board[42] == 1;
			if (flag2)
			{
				num += 15;
			}
			bool flag3 = _board[44] == 3;
			if (flag3)
			{
				num += 5;
			}
			bool flag4 = _board[45] == 2;
			if (flag4)
			{
				num += 15;
			}
			else
			{
				bool flag5 = _board[52] == 2;
				if (flag5)
				{
					num += 10;
				}
			}
			num += 10;
		}
		bool flag6 = _board[36] == 1;
		if (flag6)
		{
			bool flag7 = _board[45] == 1;
			if (flag7)
			{
				num += 5;
			}
			bool flag8 = _board[42] == 2;
			if (flag8)
			{
				num += 5;
			}
			else
			{
				bool flag9 = _board[52] == 2;
				if (flag9)
				{
					num += 3;
				}
			}
			num += 5;
		}
		bool flag10 = _board[27] == 9;
		if (flag10)
		{
			bool flag11 = _board[18] == 9;
			if (flag11)
			{
				num -= 15;
			}
			bool flag12 = _board[20] == 11;
			if (flag12)
			{
				num -= 5;
			}
			bool flag13 = _board[21] == 10;
			if (flag13)
			{
				num -= 15;
			}
			else
			{
				bool flag14 = _board[12] == 10;
				if (flag14)
				{
					num -= 10;
				}
			}
			num -= 10;
		}
		bool flag15 = _board[36] == 9;
		if (flag15)
		{
			bool flag16 = _board[21] == 9;
			if (flag16)
			{
				num -= 5;
			}
			bool flag17 = _board[18] == 10;
			if (flag17)
			{
				num -= 5;
			}
			else
			{
				bool flag18 = _board[11] == 10;
				if (flag18)
				{
					num -= 3;
				}
			}
			num -= 5;
		}
		return num;
	}

	// Token: 0x0600000E RID: 14 RVA: 0x00003A08 File Offset: 0x00001C08
	private static int CalculateKingSafety(byte[] _board)
	{
		int num = 0;
		num -= (int)Program.CountAttacks(_board, (int)Program.wkPos, true);
		num += (int)Program.CountAttacks(_board, (int)Program.bkPos, false);
		return num * 40;
	}

	// Token: 0x0600000F RID: 15 RVA: 0x00003A40 File Offset: 0x00001C40
	private static int CalculatePawnStructure(byte[] _board)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				int num3 = (int)_board[i * 8 + j];
				bool flag = num3 == 1;
				if (flag)
				{
					bool flag2 = Program.IsIsolatedPawn(_board, i, j, true);
					if (flag2)
					{
						num -= 15;
					}
				}
				else
				{
					bool flag3 = num3 == 9;
					if (flag3)
					{
						bool flag4 = Program.IsIsolatedPawn(_board, i, j, false);
						if (flag4)
						{
							num2 -= 15;
						}
					}
				}
			}
		}
		return num - num2;
	}

	// Token: 0x06000010 RID: 16 RVA: 0x00003AD4 File Offset: 0x00001CD4
	private static byte CountAttacks(byte[] _board, int _ourPos, bool _weWhite)
	{
		byte b = 0;
		for (byte b2 = 0; b2 < 64; b2 += 1)
		{
			byte b3 = _board[(int)b2];
			bool flag = (_weWhite && b3 > 8) || (!_weWhite && b3 < 8);
			if (flag)
			{
				bool flag2 = Program.CanAttack(_board, b3, b2 % 8, b2 / 8, (byte)(_ourPos % 8), (byte)(_ourPos / 8));
				if (flag2)
				{
					b += 1;
				}
			}
		}
		return b;
	}

	// Token: 0x06000011 RID: 17 RVA: 0x00003B44 File Offset: 0x00001D44
	private static bool IsIsolatedPawn(byte[] _board, int _x, int _y, bool isWhite)
	{
		bool flag = _y > 0 && _y < 7;
		bool result;
		if (flag)
		{
			bool flag2;
			bool flag3;
			if (isWhite)
			{
				flag2 = (_board[_y * 8 + 9 + _x] == 1);
				flag3 = (_board[_y * 8 + 7 + _x] == 1);
			}
			else
			{
				flag2 = (_board[_y * 8 - 9 + _x] == 9);
				flag3 = (_board[_y * 8 - 7 + _x] == 9);
			}
			result = (!flag2 && !flag3);
		}
		else
		{
			result = true;
		}
		return result;
	}

	// Token: 0x06000012 RID: 18 RVA: 0x00003BBC File Offset: 0x00001DBC
	private static bool CanAttack(byte[] _board, byte _enemy, byte _fromX, byte _fromY, byte _toX, byte _toY)
	{
		int num = Math.Abs((int)(_toX - _fromX));
		int num2 = Math.Abs((int)(_toY - _fromY));
		switch (_enemy)
		{
		case 1:
		case 9:
			return num == 1 && num2 == 1;
		case 2:
		case 10:
			return (num == 2 && num2 == 1) || (num == 1 && num2 == 2);
		case 3:
		case 11:
		{
			bool flag = num == num2;
			return flag && !Program.CheckForDiagonalBlocking(_board, _fromX, _fromY, _toX, _toY);
		}
		case 4:
		case 12:
		{
			bool flag2 = num == 0 || num2 == 0;
			return flag2 && !Program.CheckForVerticalBlocking(_board, 8 * _fromY + _fromX, 8 * _toY + _toX) && !Program.CheckForHorizontalBlocking(_board, 8 * _fromY + _fromX, 8 * _toY + _toX);
		}
		case 5:
		case 13:
		{
			bool flag3 = num == 0 || num2 == 0 || num == num2;
			return flag3 && (!Program.CheckForVerticalBlocking(_board, 8 * _fromY + _fromX, 8 * _toY + _toX) && !Program.CheckForDiagonalBlocking(_board, _fromX, _fromY, _toX, _toY)) && !Program.CheckForHorizontalBlocking(_board, 8 * _fromY + _fromX, 8 * _toY + _toX);
		}
		}
		return false;
	}

	// Token: 0x06000013 RID: 19 RVA: 0x00003D28 File Offset: 0x00001F28
	private static bool CheckForDiagonalBlocking(byte[] _board, byte _fromX, byte _fromY, byte _toX, byte _toY)
	{
		for (byte b = Math.Min(_fromY, _toY); b < Math.Max(_fromY, _toY); b += 1)
		{
			for (byte b2 = Math.Min(_fromX, _toX); b2 < Math.Max(_fromX, _toX); b2 += 1)
			{
				bool flag = _board[(int)(b * 8 + b2)] > 0;
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000014 RID: 20 RVA: 0x00003D94 File Offset: 0x00001F94
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
			for (byte b = Math.Min(_from, _to) + 8; b < Math.Max(_from, _to); b += 8)
			{
				bool flag2 = _board[(int)b] > 0;
				if (flag2)
				{
					return true;
				}
			}
			result = false;
		}
		return result;
	}

	// Token: 0x06000015 RID: 21 RVA: 0x00003DF0 File Offset: 0x00001FF0
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
			for (byte b = Math.Min(_from, _to) + 1; b < Math.Max(_from, _to); b += 1)
			{
				bool flag2 = _board[(int)b] > 0;
				if (flag2)
				{
					return true;
				}
			}
			result = false;
		}
		return result;
	}

	// Token: 0x06000016 RID: 22 RVA: 0x00003E48 File Offset: 0x00002048
	private static List<Program.Move> GenerateAllMoves(byte[] _thisboard, bool _isWhiteTurn)
	{
		List<Program.Move> list = new List<Program.Move>();
		for (byte b = 0; b < 64; b += 1)
		{
			byte b2 = _thisboard[(int)b];
			bool flag = (_isWhiteTurn && b2 < 8) || (!_isWhiteTurn && b2 > 8);
			if (flag)
			{
				switch (b2)
				{
				case 1:
				case 9:
					list.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GeneratePawnMoves(_thisboard, b, _isWhiteTurn), _isWhiteTurn));
					break;
				case 2:
				case 10:
					list.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GenerateKnightMoves(_thisboard, b, b2, _isWhiteTurn), _isWhiteTurn));
					break;
				case 3:
				case 11:
					list.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GenerateBishopMoves(_thisboard, b, b2, _isWhiteTurn), _isWhiteTurn));
					break;
				case 4:
				case 12:
					list.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GenerateRookMoves(_thisboard, b, b2, _isWhiteTurn), _isWhiteTurn));
					break;
				case 5:
				case 13:
					list.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GenerateQueenMoves(_thisboard, b, b2, _isWhiteTurn), _isWhiteTurn));
					break;
				case 6:
				case 14:
					list.AddRange(Program.IsMoveLegalNoCheckCriteria(_thisboard, Program.GenerateKingMoves(_thisboard, b, b2, _isWhiteTurn), _isWhiteTurn));
					break;
				}
			}
		}
		return list;
	}

	// Token: 0x06000017 RID: 23 RVA: 0x00003F84 File Offset: 0x00002184
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

	// Token: 0x06000018 RID: 24 RVA: 0x00003FD8 File Offset: 0x000021D8
	private static byte[] SimulateMove(byte[] _oldBoard, Program.Move move)
	{
		byte[] array = (byte[])_oldBoard.Clone();
		array[(int)move.To] = move.Piece;
		array[(int)move.From] = 0;
		return array;
	}

	// Token: 0x06000019 RID: 25 RVA: 0x00004010 File Offset: 0x00002210
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
			List<Program.Move> list = Program.GenerateAllMoves(_board, _isWhiteTurn);
			int num = 0;
			foreach (Program.Move move in list)
			{
				num += Program.PositionsAmountTest(Program.SimulateMove(_board, move), _depth - 1, !_isWhiteTurn);
			}
			result = num;
		}
		return result;
	}

	// Token: 0x0600001A RID: 26 RVA: 0x00004090 File Offset: 0x00002290
	private static bool IsKingInCheck(byte[] _board, byte _kingPos, bool _kingColor)
	{
		bool flag = _kingPos == 64;
		if (flag)
		{
			if (_kingColor)
			{
				for (byte b = 0; b < 64; b += 1)
				{
					bool flag2 = _board[(int)b] == 6;
					if (flag2)
					{
						_kingPos = b;
						b += 64;
					}
				}
			}
			else
			{
				for (byte b2 = 0; b2 < 64; b2 += 1)
				{
					bool flag3 = _board[(int)b2] == 14;
					if (flag3)
					{
						_kingPos = b2;
						b2 += 64;
					}
				}
			}
		}
		return Program.CountAttacks(_board, (int)_kingPos, _kingColor) > 0;
	}

	// Token: 0x0600001B RID: 27 RVA: 0x00004134 File Offset: 0x00002334
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
			List<Program.Move> list = Program.GenerateAllMoves(_board, maximizingPlayer);
			if (maximizingPlayer)
			{
				int num = -999999;
				foreach (Program.Move move in list)
				{
					byte[] board = Program.SimulateMove(_board, move);
					Program.Move move2;
					int num2 = Program.AlphaBetaSearch(board, depth - 1, alpha, beta, false, out move2);
					bool flag2 = num2 > num;
					if (flag2)
					{
						num = num2;
						bestMove = move;
					}
					alpha = Math.Max(alpha, num2);
					bool flag3 = beta <= alpha;
					if (flag3)
					{
						Program.gSkippedPositions++;
						break;
					}
				}
				result = num;
			}
			else
			{
				int num3 = 999999;
				foreach (Program.Move move3 in list)
				{
					byte[] board2 = Program.SimulateMove(_board, move3);
					Program.Move move4;
					int num4 = Program.AlphaBetaSearch(board2, depth - 1, alpha, beta, true, out move4);
					bool flag4 = num4 < num3;
					if (flag4)
					{
						num3 = num4;
						bestMove = move3;
					}
					beta = Math.Min(beta, num4);
					bool flag5 = beta <= alpha;
					if (flag5)
					{
						Program.gSkippedPositions++;
						break;
					}
				}
				result = num3;
			}
		}
		return result;
	}

	// Token: 0x0600001C RID: 28 RVA: 0x000042B8 File Offset: 0x000024B8
	private static List<Program.Move> GeneratePawnMoves(byte[] _board, byte _position, bool _isWhite)
	{
		List<Program.Move> list = new List<Program.Move>();
		int num = _isWhite ? -8 : 8;
		byte b = _isWhite ? 6 : 1;
		byte b2 = _isWhite ? 0 : 7;
		byte b3 = _position % 8;
		int num2 = (int)_position + num;
		bool flag = num2 >= 0 && num2 < 64 && _board[num2] == 0;
		if (flag)
		{
			bool flag2 = num2 / 8 == (int)b2;
			if (flag2)
			{
				list.Insert(0, new Program.Move
				{
					From = _position,
					To = (byte)num2,
					Piece = (_isWhite ? 2 : 10),
					CapturedPiece = 0
				});
				list.Insert(0, new Program.Move
				{
					From = _position,
					To = (byte)num2,
					Piece = (_isWhite ? 3 : 11),
					CapturedPiece = 0
				});
				list.Insert(0, new Program.Move
				{
					From = _position,
					To = (byte)num2,
					Piece = (_isWhite ? 4 : 12),
					CapturedPiece = 0
				});
				list.Insert(0, new Program.Move
				{
					From = _position,
					To = (byte)num2,
					Piece = (_isWhite ? 5 : 13),
					CapturedPiece = 0
				});
			}
			else
			{
				list.Add(new Program.Move
				{
					From = _position,
					To = (byte)num2,
					Piece = (_isWhite ? 1 : 9),
					CapturedPiece = 0
				});
			}
			bool flag3 = _position / 8 == b && _board[num2 + num] == 0;
			if (flag3)
			{
				list.Add(new Program.Move
				{
					From = _position,
					To = (byte)(num2 + num),
					Piece = _board[(int)_position],
					CapturedPiece = 0
				});
			}
		}
		bool flag4 = b3 > 0;
		if (flag4)
		{
			byte b4 = _isWhite ? (_position - 9) : (_position + 7);
			bool flag5 = b4 >= 0 && b4 < 64;
			if (flag5)
			{
				byte b5 = _board[(int)b4];
				bool flag6 = b5 != 0 && ((_isWhite && b5 > 8) || (!_isWhite && b5 < 8));
				if (flag6)
				{
					bool flag7 = b4 / 8 == b2;
					if (flag7)
					{
						list.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)num2,
							Piece = (_isWhite ? 2 : 10),
							CapturedPiece = 0
						});
						list.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)num2,
							Piece = (_isWhite ? 3 : 11),
							CapturedPiece = 0
						});
						list.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)num2,
							Piece = (_isWhite ? 4 : 12),
							CapturedPiece = 0
						});
						list.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)num2,
							Piece = (_isWhite ? 5 : 13),
							CapturedPiece = 0
						});
					}
					else
					{
						list.Insert(0, new Program.Move
						{
							From = _position,
							To = b4,
							Piece = (_isWhite ? 1 : 9),
							CapturedPiece = b5
						});
					}
				}
			}
		}
		bool flag8 = b3 < 7;
		if (flag8)
		{
			byte b6 = _isWhite ? (_position - 7) : (_position + 9);
			bool flag9 = b6 >= 0 && b6 < 64;
			if (flag9)
			{
				byte b7 = _board[(int)b6];
				bool flag10 = b7 != 0 && ((_isWhite && b7 > 8) || (!_isWhite && b7 < 8));
				if (flag10)
				{
					bool flag11 = b6 / 8 == b2;
					if (flag11)
					{
						list.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)num2,
							Piece = (_isWhite ? 2 : 10),
							CapturedPiece = 0
						});
						list.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)num2,
							Piece = (_isWhite ? 3 : 11),
							CapturedPiece = 0
						});
						list.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)num2,
							Piece = (_isWhite ? 4 : 12),
							CapturedPiece = 0
						});
						list.Insert(0, new Program.Move
						{
							From = _position,
							To = (byte)num2,
							Piece = (_isWhite ? 5 : 13),
							CapturedPiece = 0
						});
					}
					else
					{
						list.Insert(0, new Program.Move
						{
							From = _position,
							To = b6,
							Piece = (_isWhite ? 1 : 9),
							CapturedPiece = b7
						});
					}
				}
			}
		}
		return list;
	}

	// Token: 0x0600001D RID: 29 RVA: 0x0000479C File Offset: 0x0000299C
	private static List<Program.Move> GenerateKnightMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
	{
		List<Program.Move> list = new List<Program.Move>();
		int[] array = new int[]
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
		byte b = _position % 8;
		for (int i = 0; i < 8; i++)
		{
			bool flag = (i < 2 && b > 1) || (i > 1 && i < 4 && b > 0) || (i > 3 && i < 6 && b < 7) || (i > 5 && b < 6);
			if (flag)
			{
				int num = (int)_position + array[i];
				bool flag2 = num >= 0 && num < 64;
				if (flag2)
				{
					byte b2 = _board[num];
					bool flag3 = b2 == 0;
					if (flag3)
					{
						list.Add(new Program.Move
						{
							From = _position,
							To = (byte)num,
							Piece = _piece,
							CapturedPiece = 0
						});
					}
					else
					{
						bool flag4 = _isWhite && b2 > 8;
						if (flag4)
						{
							list.Insert(0, new Program.Move
							{
								From = _position,
								To = (byte)num,
								Piece = _piece,
								CapturedPiece = b2
							});
						}
						else
						{
							bool flag5 = !_isWhite && b2 < 8;
							if (flag5)
							{
								list.Insert(0, new Program.Move
								{
									From = _position,
									To = (byte)num,
									Piece = _piece,
									CapturedPiece = b2
								});
							}
						}
					}
				}
			}
		}
		return list;
	}

	// Token: 0x0600001E RID: 30 RVA: 0x00004910 File Offset: 0x00002B10
	private static List<Program.Move> GenerateBishopMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
	{
		List<Program.Move> list = new List<Program.Move>();
		int[] array = new int[]
		{
			-9,
			7,
			-7,
			9
		};
		for (int i = 0; i < 4; i++)
		{
			byte b = _position % 8;
			bool flag = (i < 2 && b > 0) || (i > 1 && b < 7);
			if (flag)
			{
				int num = (int)_position + array[i];
				while (num >= 0 && num < 64)
				{
					byte b2 = _board[num];
					bool flag2 = b2 == 0;
					if (!flag2)
					{
						bool flag3 = (_isWhite && b2 > 8) || (!_isWhite && b2 < 8);
						if (flag3)
						{
							list.Insert(0, new Program.Move
							{
								From = _position,
								To = (byte)num,
								Piece = _piece,
								CapturedPiece = b2
							});
						}
						break;
					}
					list.Add(new Program.Move
					{
						From = _position,
						To = (byte)num,
						Piece = _piece,
						CapturedPiece = 0
					});
					b = (byte)(num % 8);
					bool flag4 = (i < 2 && b > 0) || (i > 1 && b < 7);
					if (flag4)
					{
						num += array[i];
					}
					else
					{
						num = 64;
					}
				}
			}
		}
		return list;
	}

	// Token: 0x0600001F RID: 31 RVA: 0x00004A68 File Offset: 0x00002C68
	private static List<Program.Move> GenerateRookMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
	{
		List<Program.Move> list = new List<Program.Move>();
		int[] array = new int[]
		{
			-1,
			-8,
			8,
			1
		};
		byte b = _position % 8;
		for (int i = 0; i < 4; i++)
		{
			bool flag = (i < 1 && b > 0) || i == 1 || i == 2 || (i > 2 && b < 7);
			if (flag)
			{
				int num = (int)_position + array[i];
				while (num >= 0 && num < 64)
				{
					byte b2 = _board[num];
					bool flag2 = b2 == 0;
					if (!flag2)
					{
						bool flag3 = (_isWhite && b2 > 8) || (!_isWhite && b2 < 8);
						if (flag3)
						{
							list.Insert(0, new Program.Move
							{
								From = _position,
								To = (byte)num,
								Piece = _piece,
								CapturedPiece = b2
							});
						}
						break;
					}
					list.Add(new Program.Move
					{
						From = _position,
						To = (byte)num,
						Piece = _piece,
						CapturedPiece = 0
					});
					b = (byte)(num % 8);
					bool flag4 = (i < 1 && b > 0) || i == 1 || i == 2 || (i > 2 && b < 7);
					if (flag4)
					{
						num += array[i];
					}
					else
					{
						num = 64;
					}
				}
			}
		}
		return list;
	}

	// Token: 0x06000020 RID: 32 RVA: 0x00004BD0 File Offset: 0x00002DD0
	private static List<Program.Move> GenerateQueenMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
	{
		List<Program.Move> list = new List<Program.Move>();
		list = Program.GenerateBishopMoves(_board, _position, _piece, _isWhite);
		list.AddRange(Program.GenerateRookMoves(_board, _position, _piece, _isWhite));
		return list;
	}

	// Token: 0x06000021 RID: 33 RVA: 0x00004C04 File Offset: 0x00002E04
	private static List<Program.Move> GenerateKingMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
	{
		List<Program.Move> list = new List<Program.Move>();
		int[] array = new int[]
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
		byte b = _position % 8;
		for (int i = 0; i < 8; i++)
		{
			bool flag = (i < 3 && b > 0) || (i > 4 && i < 8 && b < 7) || i == 3 || i == 4;
			if (flag)
			{
				int num = (int)_position + array[i];
				bool flag2 = num >= 0 && num < 64;
				if (flag2)
				{
					byte b2 = _board[num];
					bool flag3 = b2 == 0;
					if (flag3)
					{
						list.Add(new Program.Move
						{
							From = _position,
							To = (byte)num,
							Piece = _piece,
							CapturedPiece = b2
						});
					}
					else
					{
						bool flag4 = _isWhite && b2 > 8;
						if (flag4)
						{
							list.Insert(0, new Program.Move
							{
								From = _position,
								To = (byte)num,
								Piece = _piece,
								CapturedPiece = b2
							});
						}
						else
						{
							bool flag5 = !_isWhite && b2 < 8;
							if (flag5)
							{
								list.Insert(0, new Program.Move
								{
									From = _position,
									To = (byte)num,
									Piece = _piece,
									CapturedPiece = b2
								});
							}
						}
					}
				}
			}
		}
		return list;
	}

	// Token: 0x06000022 RID: 34 RVA: 0x00004D6C File Offset: 0x00002F6C
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

	// Token: 0x06000023 RID: 35 RVA: 0x00004F08 File Offset: 0x00003108
	private static bool GetTurn()
	{
		Console.Write("\n\tWhose turn? (W / Y / YES / 1  = player white turn): ");
		string a = Console.ReadLine().Trim().ToLower();
		return a == "w" || a == "y" || a == "yes" || a == "1";
	}

	// Token: 0x06000024 RID: 36 RVA: 0x00004F7C File Offset: 0x0000317C
	private static int GetDepth()
	{
		int num = -1;
		Console.Write("\tEnter the depth for the algorithm search (only from 1 to 6): ");
		while (num < 1 || num > 6)
		{
			string s = Console.ReadLine();
			Console.Clear();
			bool flag = !int.TryParse(s, out num);
			if (flag)
			{
				Console.Write("\tInvalid input, please try again: ");
			}
			else
			{
				bool flag2 = num < 1 || num > 6;
				if (flag2)
				{
					Console.Write("\tOut of bounds, please enter a valid number from the interval: ");
				}
			}
		}
		Console.Write("\n");
		return num;
	}

	// Token: 0x06000025 RID: 37 RVA: 0x00005008 File Offset: 0x00003208
	private static bool GetBoardDisplayType()
	{
		Console.Write("\n\tDisplay chess unicode pieces? (Yes/Y/1/Да = display unicode, else = numbers): ");
		string a = Console.ReadLine().ToLower().Replace(" ", "");
		return a == "yes" || a == "y" || a == "1" || a == "да";
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
		// (get) Token: 0x06000028 RID: 40 RVA: 0x0000509A File Offset: 0x0000329A
		// (set) Token: 0x06000029 RID: 41 RVA: 0x000050A2 File Offset: 0x000032A2
		public byte From { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600002A RID: 42 RVA: 0x000050AB File Offset: 0x000032AB
		// (set) Token: 0x0600002B RID: 43 RVA: 0x000050B3 File Offset: 0x000032B3
		public byte To { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600002C RID: 44 RVA: 0x000050BC File Offset: 0x000032BC
		// (set) Token: 0x0600002D RID: 45 RVA: 0x000050C4 File Offset: 0x000032C4
		public byte Piece { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600002E RID: 46 RVA: 0x000050CD File Offset: 0x000032CD
		// (set) Token: 0x0600002F RID: 47 RVA: 0x000050D5 File Offset: 0x000032D5
		public byte CapturedPiece { get; set; }
	}
}
