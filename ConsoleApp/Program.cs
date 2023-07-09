using System;
using System.Collections.Generic;
using BLL;
using Persistence;
using System.Linq;

namespace ConsoleApp
{
    public class Program
    {
        static DateTime timeLog = DateTime.Now;
        static TimeSpan now = timeLog.TimeOfDay;

        // Control the start/end time of shift 1 & 2
        static TimeSpan startTime1 = new(8, 0, 0);
        static TimeSpan startTime2 = new(15, 0, 0);
        static TimeSpan endTime = new(22, 0, 0);

        public static void Main()
        {
            bool running = true; // The Program will exit when this is set to 'false'
            int screen = 0; // Define the current screen
            CashierBLL cbll = new(); // Do things with Cashier
            ShiftBLL sbll = new(); // Do things with Shift
            List<Cashier> list1 = new(); // Store cashiers who login to shift 1
            List<Cashier> list2 = new(); // Same, but shift 2

            while (running)
            {
                switch (screen)
                {
                    case 0: // Main Menu
                        screen = MainMenu(); // Only return 1 -> 4
                        break;

                    case 1: // Choose Shift, then Login
                        int cmd = ChooseShiftScreen(); // Only return int.MinValue, 1 or 2

                        if (cmd == int.MinValue) { // User enter 'Esc' => return int.MinValue => Back to Main Menu
                            screen = 0;
                            break;
                        }

                        while (true) {
                            string[]? strs = OpenLoginScreen(cmd); // Return null if User enter 'Esc'
                            if (strs != null)
                            {
                                Cashier? cashier = cbll.GetCashierByLogin(strs[0], strs[1]);
                                if (cashier != null)
                                {
                                    if (IsLoginTime(cmd)) {
                                        if (cmd == 1)
                                            list1.Add(cashier);
                                        else
                                            list2.Add(cashier);

                                        screen = 0;
                                        Alert($"Login successfully. Hello {cashier.Name}.", 4, 27);
                                    }
                                    else
                                        Alert("Invalid login time! Please try again later.", 4, 27, ConsoleColor.Red);

                                    break;
                                }
                                else if (Alert("ACCOUNT NOT FOUND!", 4, 27, ConsoleColor.Red) == ConsoleKey.Escape)
                                    break;
                            }
                            else break; // User enter 'Esc' => break => Back to Main Menu
                        }

                        break;

                    case 2: // Create Bill
                        int shiftValue = GetCurrentShiftValue();

                        if (shiftValue == 1) {
                            if (list1.Count > 0) { // Check if anyone is in the system first
                                Cashier? cashier = ChooseCashierScreen(list1); // Only null if User enter 'Esc' => Back to Main Menu

                                if (cashier != null) {
                                    CreateBillScreen(cashier);
                                    Console.ReadKey();
                                    break;
                                }
                            }
                            else
                                Alert("No one has logged in the shift yet, please login.", 25, 14, ConsoleColor.Yellow, cls: true);
                        }
                        else if (shiftValue == 2) {
                            if (list1.Count > 0) // Anyone in list1 must be logged out first
                                Alert("Cashier from previous shift must log out first.", 25, 14, ConsoleColor.Yellow, cls: true);
                            else if (list2.Count == 0)
                                Alert("No one has logged in the shift yet, please login.", 25, 14, ConsoleColor.Yellow, cls: true);
                            else {
                                Cashier? cashier = ChooseCashierScreen(list2);

                                if (cashier != null) {
                                    CreateBillScreen(cashier);
                                    Console.ReadKey();
                                    break;
                                }
                            }
                        }
                        else
                            Alert("The shift is not started yet.", 30, 14, ConsoleColor.Yellow, cls: true);

                        screen = 0;
                        break;

                    case 3:
                        running = false;
                        break;

                    case 4:
                        shiftValue = GetCurrentShiftValue();
                        Cashier? reporter = null;

                        if (list1.Count > 0 && shiftValue != 1)
                            reporter = ChooseCashierScreen(list1);
                        else if (list2.Count > 0 && shiftValue != 2)
                            reporter = ChooseCashierScreen(list2);

                        if (reporter != null) {
                            while (true) {
                                float total = ShiftBLL.CheckTotalIncome(new() { StartTime = timeLog, EndTime = DateTime.Now });
                                Alert($"Expected Income: ${total}", interrupt: false, cls: true);
                                Console.WriteLine("    Actual Income: ");

                                if (float.TryParse(Console.ReadLine(), out float actual))
                                {
                                    Alert($"Reporter: {reporter.Name} - Id: {reporter.Id}\nAre you sure to report this ? (Y/N)", 4, 8, ConsoleColor.Yellow);
                                    string? choice = GetStrCharByChar();

                                    if (choice != null) {
                                        if (choice == "Y") {
                                            if (sbll.AddShift(timeLog, DateTime.Now, reporter.Id, total, actual)) {
                                                if (list1.Count > 0)
                                                    list1.Clear();
                                                else if (list2.Count > 0)
                                                    list2.Clear();

                                                timeLog = DateTime.Now;
                                                Alert("Report successfully!", 4, 13);
                                                break;
                                            }
                                            else
                                                Alert("Some error occurs. Please try again or contact the maintenance department.", 18, 14, ConsoleColor.Red, cls: true);
                                        }
                                        else if (choice == "N") break;
                                        else {
                                            Alert("Please enter 'Y' or 'N' !", 4, 13, ConsoleColor.Red);
                                            continue;
                                        }
                                    }
                                    else break;
                                }
                                else Alert("Invalid input format! Please try again.", 4, 13, ConsoleColor.Red);
                            }
                        }
                        else {
                            Alert("Invalid report time! Please try again later.", 28, 14, ConsoleColor.Red, cls: true);
                        }
    
                        screen = 0;
                        break;
                }
            }
        }

        public static int MainMenu()
        {
            Console.Clear();
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

            return ReadCmd();
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
                    return int.MinValue;
            }
        }

        public static ConsoleKey? Alert(string alert, int left = 4, int top = 4, ConsoleColor color = ConsoleColor.Green, bool interrupt = true, bool cls = false)
        {
            if (cls) Console.Clear();
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

        public static void PrintButton(string text, int B_length, int padleft = 0)
        {
            string str = new('═', B_length - 2);
            if (text.Length > B_length - 2)
                B_length = text.Length + 4;

            int PadLeft = (B_length + text.Length) / 2 - 1;
            int PadRight = B_length - 2 - PadLeft;

            string pad = new(' ', padleft);

            Console.WriteLine($"{pad}╔{str}╗");
            Console.WriteLine($"{pad}║{text.PadLeft(PadLeft)}{new string(' ', PadRight)}║");
            Console.WriteLine($"{pad}╚{str}╝");
        }

        public static int ChooseShiftScreen()
        {
            Alert("Choose your shift:\n", interrupt: false, cls: true);

            PrintButton("1. Shift 1", 25, 10);
            PrintButton("2. Shift 2", 25, 10);

            Console.WriteLine("\n\n\n    --- Enter 'esc' button to go back ---");

            return ReadCmd(end: 2);
        }

        public static string? GetStrCharByChar()
        {
            Console.CursorVisible = true;
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

        public static string[]? OpenLoginScreen(int shift = 1)
        {
            Console.CursorVisible = true;
            Alert($"You are logging to shift {shift}\n    You can only login from {((shift == 1) ? "7:45 AM to 2:59 PM" : "2:45 PM to 9:59 PM")}", 4, 1, interrupt: false, cls: true);
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
            Console.Write("                                --- Enter 'esc' button to go back ---");


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

        public static Cashier? ChooseCashierScreen(List<Cashier> cashiers) {
            while (true) {
                Alert("Enter your Cashier Id:", interrupt: false, cls: true);

                string str = $"          +----+{new string('-', 34)}+";

                // Print out the cashiers table
                Console.Write("\n\n");
                Console.WriteLine(str);
                Console.WriteLine($"          | Id |               {"Name", -19}|");
                Console.WriteLine(str);

                foreach (Cashier cashier in cashiers)
                    Console.WriteLine($"          | {cashier.Id, -3}| {cashier.Name, -33}|");

                Console.WriteLine(str);
                Console.Write("\n\n\n    --- Enter 'esc' button to go back ---");

                Console.SetCursorPosition(27, 4);
                string? cmd = GetStrCharByChar();

                if (cmd == null)
                    return null;
                else {
                    if (int.TryParse(cmd, out int id)) {
                        Cashier? cashier = cashiers.FirstOrDefault(c => c.Id == id);
                        if (cashier != null)
                            return cashier;
                    }
                    Alert("Invalid Id, Please try again.", 3, 20 + cashiers.Count, ConsoleColor.Red);
                }
            }
        }

        public static void CreateBillScreen(Cashier cashier) {
            Console.Clear();
            Console.Write("\n\n\n");
            Console.WriteLine("                                    ██████  ██ ██      ██");
            Console.WriteLine("                                    ██   ██ ██ ██      ██");
            Console.WriteLine("                                    ██████  ██ ██      ██");
            Console.WriteLine("                                    ██   ██ ██ ██      ██");
            Console.WriteLine("                                    ██████  ██ ███████ ███████\n\n\n");
            Console.WriteLine($"Cashier: {cashier.Name}\n");
        }

        public static bool IsLoginTime(int shift) {
            now = DateTime.Now.TimeOfDay;
            if (shift == 1)
                return now.Add(new(0, 15, 0)) >= startTime1 && now < startTime2;
            else
                return now.Add(new(0, 15, 0)) >= startTime2 && now < endTime;
        }
    
        public static int GetCurrentShiftValue() {
            now = DateTime.Now.TimeOfDay;
            
            if (now >= startTime1 && now < startTime2)
                return 1;
            else if (now >= startTime2 && now < endTime)
                return 2;
            
            return 0;
        }
    }
}