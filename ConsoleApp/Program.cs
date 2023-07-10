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
            BillBLL bbll = new(); // Do things with Bill
            ShiftBLL sbll = new(); // Do things with Shift
            List<List<Cashier>> loginList = new() { new(), new() }; // Store cashiers who login to shift 1 & 2
            List<Order> orders = new();

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

                        if (IsLoginTime(cmd)) { // If currently is not valid time to login, alert user immediately
                            while (true) {
                                string[]? strs = OpenLoginScreen(cmd); // Return null if User enter 'Esc'
                                if (strs != null)
                                {
                                    Cashier? cashier = cbll.GetCashierByLogin(strs[0], strs[1]);
                                    if (cashier != null)
                                    {
                                        loginList[--cmd].Add(cashier);

                                        screen = 0;
                                        Alert($"Login successfully. Hello {cashier.Name}.", 4, 27);
                                        break;
                                    }
                                    else if (Alert("ACCOUNT NOT FOUND!", 4, 27, ConsoleColor.Red) == ConsoleKey.Escape)
                                        break;
                                }
                                else break;
                            }
                        }
                        else
                            Alert("Invalid login time! Please try again later.", 4, 18, ConsoleColor.Red);

                        break;

                    case 2: // Create Bill
                        int shiftValue = GetCurrentShiftValue();

                        if (shiftValue == 0) {
                            Alert("The shift is not started yet.", 30, 14, ConsoleColor.Yellow, cls: true);
                            screen = 0;
                            break;
                        }

                        List<Cashier> list = loginList[shiftValue - 1];

                        if (list.Count == 0)
                            Alert("No one has logged in the shift yet, please login.", 25, 14, ConsoleColor.Yellow, cls: true);
                        else {
                            if (shiftValue == 2 && loginList[0].Count > 0)
                                Alert("Cashiers from previous shift must log out first.", 25, 14, ConsoleColor.Yellow, cls: true);
                            else {
                                Cashier? cashier = ChooseCashierScreen(list);

                                while (cashier != null) {
                                    bool confirm = false;
                                    cmd = CreateBillScreen(cashier);
                                    switch (cmd) {
                                        case 1:

                                            break;
                                        
                                        case 2:
                                            break;
                            
                                        case 3:
                                            break;

                                        case 4:
                                            while (true) {
                                                ConsoleKey? key = Alert("Are you sure to create this bill ? (Y/N)", 4, 15 + orders.Count, ConsoleColor.Yellow);
                                                if (key == ConsoleKey.Y) {
                                                    confirm = true;
                                                    break;
                                                }
                                                else if (key == ConsoleKey.N || key == null) break;
                                                else {
                                                    Alert("Please enter 'Y' or 'N' !", 4, 18 + orders.Count, ConsoleColor.Red);
                                                    continue;
                                                }
                                            }
                                            break;
                                    }

                                    if (confirm) {
                                        if (bbll.AddBill(new() { Cashier_id = cashier.Id, Created_date = DateTime.Now }, orders)) {
                                            orders.Clear();
                                            Alert("Add Bill successfully!", 4, 18 + orders.Count);
                                        }
                                        else Alert("Some error occurs. Please try again or contact the maintenance department.", 18, 14, ConsoleColor.Red, cls: true);

                                        break;
                                    }
                                }
                            }
                        }

                        screen = 0;
                        break;

                    case 3:
                        running = false;
                        break;

                    case 4:
                        bool IsReportTime = false;
                        shiftValue = GetCurrentShiftValue();
                        list = new();
                        Cashier? reporter = null;

                        for (int i = 0; i < loginList.Count; ++i)
                            if (loginList[i].Count > 0 && shiftValue != i + 1) {
                                list = loginList[i];
                                reporter = ChooseCashierScreen(list); // Only return null if User enter 'Esc'
                                IsReportTime = true;
                                break;
                            }
                        
                        if (!IsReportTime) {
                            Alert("Invalid report time! Please try again later.", 28, 14, ConsoleColor.Red, cls: true);
                            screen = 0;
                            break;
                        }

                        if (reporter != null) {
                            while (true) {
                                float total = bbll.CheckTotalIncome(bbll.GetBillsFromInterval(timeLog, DateTime.Now));
                                Alert($"Expected Income from this Shift: ${total}", interrupt: false, cls: true);
                                Console.Write("    Enter the Actual Income of this Shift: ");

                                if (float.TryParse(Console.ReadLine(), out float actual))
                                {
                                    ConsoleKey? key = Alert($"Reporter: {reporter.Name} - Id: {reporter.Id}\nAre you sure to report this ? (Y/N)", 4, 8, ConsoleColor.Yellow);

                                    if (key == ConsoleKey.Y) {
                                        if (sbll.AddShift(timeLog, DateTime.Now, reporter.Id, total, actual)) {
                                            list.Clear();

                                            timeLog = DateTime.Now;
                                            Alert("Report successfully!", 4, 13);
                                            break;
                                        }
                                        else
                                            Alert("Some error occurs. Please try again or contact the maintenance department.", 18, 14, ConsoleColor.Red, cls: true);
                                    }
                                    else if (key == ConsoleKey.N || key == null) break;
                                    else {
                                        Alert("Please enter 'Y' or 'N' !", 4, 13, ConsoleColor.Red);
                                        continue;
                                    }
                                }
                                else Alert("Invalid input format! Please try again.", 4, 13, ConsoleColor.Red);
                            }
                        }

                        screen = 0;
                        break;
                }
            }
        }

        public static void PrintButton(string text, int B_length, int left, int top)
        {
            string str = new('═', B_length - 2);
            if (text.Length > B_length - 2)
                B_length = text.Length + 4;

            int PadLeft = (B_length + text.Length) / 2 - 1;
            int PadRight = B_length - 2 - PadLeft;

            Console.SetCursorPosition(left, top);
            Console.WriteLine($"╔{str}╗");
            Console.CursorLeft += left;
            Console.WriteLine($"║{text.PadLeft(PadLeft)}{new string(' ', PadRight)}║");
            Console.CursorLeft += left;
            Console.WriteLine($"╚{str}╝");
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

        public static int MainMenu()
        {
            Console.Clear();
            Console.Write("\n\n\n");
            Console.WriteLine("                  ██████  █████  ███████ ██   ██ ██ ███████ ██████      ███    ███ ███████ ███    ██ ██    ██");
            Console.WriteLine("                 ██      ██   ██ ██      ██   ██ ██ ██      ██   ██     ████  ████ ██      ████   ██ ██    ██");
            Console.WriteLine("                 ██      ███████ ███████ ███████ ██ █████   ██████      ██ ████ ██ █████   ██ ██  ██ ██    ██");
            Console.WriteLine("                 ██      ██   ██      ██ ██   ██ ██ ██      ██   ██     ██  ██  ██ ██      ██  ██ ██ ██    ██");
            Console.WriteLine("                  ██████ ██   ██ ███████ ██   ██ ██ ███████ ██   ██     ██      ██ ███████ ██   ████  ██████\n\n\n");

            PrintButton("1. Login", 40, 40, 11);
            PrintButton("2. Create Bill", 40, 40, 15);
            PrintButton("3. View History Transaction", 40, 40, 19);
            PrintButton("4. Report Shift", 40, 40, 23);

            return ReadCmd();
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

        public static int ChooseShiftScreen()
        {
            Alert("You can login 15 minutes before your shift start\n\n\n    Choose your shift:", top: 1, interrupt: false, cls: true);

            PrintButton($"1. Shift 1 ({startTime1:hh\\:mm} - {startTime2:hh\\:mm})", 40, 10, 6);
            PrintButton($"2. Shift 2 ({startTime2:hh\\:mm} - {endTime:hh\\:mm})", 40, 10, 10);

            Console.WriteLine("\n\n    --- Enter 'esc' button to go back ---");

            return ReadCmd(end: 2);
        }

        public static bool IsLoginTime(int shift) {
            now = DateTime.Now.TimeOfDay;
            if (shift == 1)
                return now.Add(new(0, 15, 0)) >= startTime1 && now < startTime2;
            else
                return now.Add(new(0, 15, 0)) >= startTime2 && now < endTime;
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

        public static string[]? OpenLoginScreen(int shift)
        {
            Console.CursorVisible = true;
            Alert($"You are logging to shift {shift}", top: 1, interrupt: false, cls: true);
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


            Console.SetCursorPosition(36, 14);
            string? username = GetStrCharByChar();
            if (username == null)
                return null;

            Console.SetCursorPosition(36, 18);
            string? password = GetStrCharByChar();
            if (password == null)
                return null;

            return new string[2] {username, password};
        }

        public static int GetCurrentShiftValue() {
            now = DateTime.Now.TimeOfDay;
            
            if (now >= startTime1 && now < startTime2)
                return 1;
            else if (now >= startTime2 && now < endTime)
                return 2;
            
            return 0;
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

        public static int CreateBillScreen(Cashier cashier) {
            Console.Clear();
            PrintButton("1. Add Product", 24, 14, 1);
            PrintButton("2. Modify Product", 24, 69, 1);
            PrintButton("3. Remove Product", 24, 14, 5);
            PrintButton("4. Confirm Bill", 24, 69, 5);

            Console.Write("\n\n\n");
            Console.WriteLine("               ██████ ██████  ███████  █████  ████████ ███████     ██████  ██ ██      ██");
            Console.WriteLine("              ██      ██   ██ ██      ██   ██    ██    ██          ██   ██ ██ ██      ██");
            Console.WriteLine("              ██      ██████  █████   ███████    ██    █████       ██████  ██ ██      ██");
            Console.WriteLine("              ██      ██   ██ ██      ██   ██    ██    ██          ██   ██ ██ ██      ██");
            Console.WriteLine("               ██████ ██   ██ ███████ ██   ██    ██    ███████     ██████  ██ ███████ ███████\n\n\n");


            Console.WriteLine($"Cashier: {cashier.Name}\n");

            return ReadCmd();
        }

    }
}