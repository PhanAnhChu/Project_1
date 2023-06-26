using System;
using System.Collections.Generic;
using BLL;
using Persistence;

namespace ConsoleApp
{
    public class Program
    {
        public static void Main()
        {
            bool running = true;
            int screen = 0;
            CashierBLL cbll = new();
            DateTime now = DateTime.Now;
            DateTime start1 = new(now.Year, now.Month, now.Day, 8, 0, 0);
            DateTime start2 = new(now.Year, now.Month, now.Day, 15, 0, 0);
            DateTime end = new(now.Year, now.Month, now.Day, 22, 0, 0);
            List<Cashier> shift1 = new();
            List<Cashier> shift2 = new();

            while (running)
            {
                switch (screen)
                {
                    case 0:
                        MainMenu();
                        screen = ReadCmd();
                        break;

                    case 1:
                        ChooseShiftScreen();
                        int cmd = ReadCmd(end: 2);

                        if (cmd == -1) {
                            screen = 0;
                            break;
                        }

                        while (true) {
                            string[]? strs = OpenLoginScreen(cmd);
                            if (strs != null)
                            {
                                Cashier? cashier = cbll.GetCashierByLogin(strs[0], strs[1]);
                                if (cashier != null)
                                {
                                    now = DateTime.Now;
                                    if (cmd == 1)
                                    {
                                        if (now.AddMinutes(15) >= start1 && now < start2)
                                        {
                                            shift1.Add(cashier);
                                            screen = 0;
                                            Alert("Login successfully. Hello " + cashier.Name, 4, 27);
                                        }
                                    }
                                    else if (now.AddMinutes(15) >= start2 && now < end)
                                    {
                                        shift2.Add(cashier);
                                        screen = 0;
                                        Alert("Login successfully. Hello " + cashier.Name, 4, 27);
                                    }
                                    else
                                        Alert("Invalid login time! Please try again later.", 4, 27, ConsoleColor.Red);

                                    break;
                                }
                                else if (Alert("ACCOUNT NOT FOUND", 4, 27, ConsoleColor.Red) == ConsoleKey.Escape)
                                    break;
                            }
                            else break;
                        }

                        break;
                    
                    case 2:
                        running = false;
                        break;

                    case 3:
                        running = false;
                        break;
                    
                    case 4:
                        running = false;
                        break;
                }
            }
        }


        static void MainMenu()
        {
            Console.Clear();
            // Console.WriteLine("Current cashier in shift:");
            // for (int i = 0; i < 1; ++i)
            Console.Write("\n\n\n");
            Console.WriteLine("                  ██████  █████  ███████ ██   ██ ██ ███████ ██████      ███    ███ ███████ ███    ██ ██    ██");
            Console.WriteLine("                 ██      ██   ██ ██      ██   ██ ██ ██      ██   ██     ████  ████ ██      ████   ██ ██    ██");
            Console.WriteLine("                 ██      ███████ ███████ ███████ ██ █████   ██████      ██ ████ ██ █████   ██ ██  ██ ██    ██");
            Console.WriteLine("                 ██      ██   ██      ██ ██   ██ ██ ██      ██   ██     ██  ██  ██ ██      ██  ██ ██ ██    ██");
            Console.WriteLine("                  ██████ ██   ██ ███████ ██   ██ ██ ███████ ██   ██     ██      ██ ███████ ██   ████  ██████\n\n\n");

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


        static int ReadCmd(int start = 1, int end = 4)
        {
            Console.CursorVisible = false;

            ConsoleKeyInfo c;
            
            while (true) {
                c = Console.ReadKey(true);

                if (char.IsDigit(c.KeyChar)) {
                    int i = c.KeyChar - 48;
                    if (i < start || i > end)
                        continue;
                    
                    return i;
                }
                else if (c.Key == ConsoleKey.Escape)
                    return -1;
            }
        }


        static void ChooseShiftScreen()
        {
            Alert("Choose your shift:\n", interrupt: false, clrscr: true);

            PrintButton("Shift 1", 20, 10);
            PrintButton("Shift 2", 20, 10);

            Console.WriteLine("\n\n\n    --- Enter 'esc' button to go back ---");
        }


        static string[]? OpenLoginScreen(int shift = 1)
        {
            Console.CursorVisible = true;
            Alert($"You are logging to shift {shift}\n    You can only login from {((shift == 1) ? "7:45 AM to 2:59 PM" : "2:45 PM to 9:59 PM")}", 4, 1, interrupt: false, clrscr: true);
            Console.ResetColor();

            Console.Write("\n\n\n");
            Console.WriteLine("                                ██       ██████   ██████  ██ ███    ██");
            Console.WriteLine("                                ██      ██    ██ ██       ██ ████   ██");
            Console.WriteLine("                                ██      ██    ██ ██   ███ ██ ██ ██  ██");
            Console.WriteLine("                                ██      ██    ██ ██    ██ ██ ██  ██ ██");
            Console.WriteLine("                                ███████  ██████   ██████  ██ ██   ████\n\n\n");
            Console.WriteLine("                        ╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("                        ║ Username:                                         ║");
            Console.WriteLine("                        ╚═══════════════════════════════════════════════════╝\n");
            Console.WriteLine("                        ╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("                        ║ Password:                                         ║");
            Console.WriteLine("                        ╚═══════════════════════════════════════════════════╝\n\n\n");
            Console.WriteLine("                                --- Enter 'esc' button to go back ---");


            Console.SetCursorPosition(36, 15);
            string? username = GetStrCharByChar();
            if (username == null)
                return null;

            Console.SetCursorPosition(36, 19);
            string? password = GetStrCharByChar();
            if (password == null)
                return null;

            return new string[2] {username, password};
        }


        static string? GetStrCharByChar()
        {
            Stack<char> chars = new();
            ConsoleKeyInfo key;
            while (true)
            {
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (chars.Count > 0)
                    {
                        chars.Pop();
                        Console.Write(" ");
                        --Console.CursorLeft;
                    }
                    else
                        ++Console.CursorLeft;
                }
                else if (key.Key == ConsoleKey.Enter) {
                    char[] result = chars.ToArray();
                    Array.Reverse(result);
                    return new string(result);
                }
                else if (key.Key == ConsoleKey.Escape) return null;

                else chars.Push(key.KeyChar);
            }
        }


        static ConsoleKey? Alert(string alert, int left = 3, int top = 3, ConsoleColor color = ConsoleColor.Green, bool interrupt = true, bool clrscr = false)
        {
            if (clrscr) Console.Clear();
            Console.SetCursorPosition(left, top);
            Console.ForegroundColor = color;
            Console.WriteLine(alert);
            Console.ResetColor();

            if (interrupt)
            {
                Console.CursorLeft += left;
                Console.WriteLine("Press any key to continue...");
                return Console.ReadKey(true).Key;
            }

            return null;
        }
    }
}