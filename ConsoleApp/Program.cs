using System;
using BLL;
using Persistence;

namespace ConsoleApp
{
    public class Program
    {
        public static void Main()
        {
            bool running = true;
            int step = 1;
            CashierBLL cbll = new();

            while (running)
            {
                switch (step)
                {
                    case 0:
                        running = false;
                        break;

                    case 1:
                        PrintMainMenu();
                        int cmd = ReadCmd();
                        if (cmd == 1)
                            step = 2;
                        else if (cmd == 4)
                            step = 0;

                        break;

                    case 2:
                        PrintLoginScreen();
                        string[]? strs = ReceiveLoginInput();
                        if (strs != null)
                        {
                            Cashier? user = cbll.GetCashierByLogin(strs[0], strs[1]);
                            if (user != null)
                            {
                                step = 3;
                                // Add code here
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\n\n\n\n ACCOUNT NOT FOUND");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("Enter any key to retry...");
                                Console.ResetColor();
                                Console.ReadKey();
                            }
                        }
                        else
                            step = 1;

                        break;
                }
            }
        }


        static void PrintMainMenu()
        {
            Console.Clear();
            Console.Write("\n\n\n");
            Console.WriteLine("                  ██████  █████  ███████ ██   ██ ██ ███████ ██████      ███    ███ ███████ ███    ██ ██    ██");
            Console.WriteLine("                 ██      ██   ██ ██      ██   ██ ██ ██      ██   ██     ████  ████ ██      ████   ██ ██    ██");
            Console.WriteLine("                 ██      ███████ ███████ ███████ ██ █████   ██████      ██ ████ ██ █████   ██ ██  ██ ██    ██");
            Console.WriteLine("                 ██      ██   ██      ██ ██   ██ ██ ██      ██   ██     ██  ██  ██ ██      ██  ██ ██ ██    ██");
            Console.WriteLine("                  ██████ ██   ██ ███████ ██   ██ ██ ███████ ██   ██     ██      ██ ███████ ██   ████  ██████\n\n\n\n\n");

            PrintButton("1. Login", 50, 38);
            PrintButton("2. Create Bill", 50, 38);
            PrintButton("3. View History Transaction", 50, 38);
            PrintButton("4. Report Shift", 50, 38);
        }


        static void PrintButton(string text, int B_length, int padleft = 0)
        {
            string str = new('═', B_length - 2);
            if (text.Length > B_length - 2)
                B_length = text.Length + 4;

            int PadLeft = (B_length + text.Length) / 2 - 1;
            int PadRight = B_length - 2 - PadLeft;

            string pad = new(' ', padleft);

            Console.WriteLine($"{pad}╔{str}╗");
            Console.WriteLine($"{pad}║{text.PadLeft(PadLeft)}{new String(' ', PadRight)}║");
            Console.WriteLine($"{pad}╚{str}╝");
        }


        static int ReadCmd()
        {
            Console.CursorVisible = false;
            char c = Console.ReadKey().KeyChar;
            if (char.IsDigit(c))
                return Convert.ToInt32(c) - 48;

            return -1;
        }

        static void PrintLoginScreen()
        {
            Console.Clear();
            Console.Write("\n\n\n");
            Console.WriteLine("                               ██       ██████   ██████  ██ ███     ██");
            Console.WriteLine("                               ██      ██    ██ ██       ██ ████    ██");
            Console.WriteLine("                               ██      ██    ██ ██   ███ ██ ██ ██   ██");
            Console.WriteLine("                               ██      ██    ██ ██   ███ ██ ██  ██  ██");
            Console.WriteLine("                               ██      ██    ██ ██    ██ ██ ██   ██ ██");
            Console.WriteLine("                               ███████  ██████   ██████  ██ ██    ████\n\n");
            Console.WriteLine("                        ╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("                        ║ Username:                                         ║");
            Console.WriteLine("                        ╚═══════════════════════════════════════════════════╝\n");
            Console.WriteLine("                        ╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("                        ║ Password:                                         ║");
            Console.WriteLine("                        ╚═══════════════════════════════════════════════════╝");
        }


        static string[]? ReceiveLoginInput()
        {
            Console.CursorVisible = true;
            string?[] info = new string[2];
            Console.SetCursorPosition(36, 12);

            info[0] = GetStrCharByChar();
            if (info[0] == null)
                return null;

            Console.SetCursorPosition(36, 16);
            info[1] = GetStrCharByChar();
            if (info[1] == null)
                return null;

            return info;
        }


        static string? GetStrCharByChar()
        {
            string str = "";
            ConsoleKeyInfo key;
            while (true)
            {
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (str.Length > 0)
                        str = str.Remove(str.Length - 1);
                    else
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                }
                else if (key.Key == ConsoleKey.Enter)
                    return str;

                if (key.Key == ConsoleKey.Escape)
                    return null;

                str += key.KeyChar;
            }
        }
    }
}