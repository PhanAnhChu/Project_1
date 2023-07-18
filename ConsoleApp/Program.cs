using System;
using System.Collections.Generic;
using BLL;
using Persistence;

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
            List<List<Cashier>> loginList = new() { new(), new() { new() { Id = 1, Name = "Phan Anh" } } }; // Store cashiers who login to shift 1 & 2
            List<Order> orders = new();

            // Interact with database
            CashierBLL cbll = new(); // Do things with Cashier
            BillBLL bbll = new(); // Do things with Bill
            ShiftBLL sbll = new(); // Do things with Shift
            GoodBLL gbll = new();

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
                        Bill bill = new();

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

                                    cmd = CreateBillScreen(cashier, orders);
                                    if (cmd == int.MinValue)
                                        break;

                                    switch (cmd) {
                                        case 1:
                                            Order? order = CreateOrder(orders);
                                            if (order != null)
                                                orders.Add(order);
        
                                            break;

                                        case 2:
                                            Alert("- MODIFY PRODUCT -", 40, 15, interrupt: false);
                                            Alert("Enter Product Id:", 8, 17, ConsoleColor.Yellow, false);
                                            Alert("<- Input must be between 0 and 2,147,483,647.", 44, 17, ConsoleColor.Green, false);
                                            int index = int.MinValue;

                                            while (true) {
                                                ClearRow(26, 17, 10);
                                                int? id = GetIntCharByChar();
                                                int? quantity;
                                                if (id != null) {
                                                    index = orders.FindIndex(o => o.Id == id);
                                                    if (index != -1) {
                                                        int nums_goods_left = gbll.GetCurrentQuantity(orders[index].Good_id);
                                                        if (nums_goods_left == int.MinValue) {
                                                            Alert("Some error occurs. Please try again or contact the database maintenance department.", 18, 14, ConsoleColor.Red, cls: true);
                                                            break;
                                                        }

                                                        int position = index + 22;
                                                        Alert("<- Modify quantity to 0 will remove the product from this bill", 45, position, interrupt: false);
                                                        while (true) {
                                                            Alert($"<- In storage: {nums_goods_left}{new string(' ', 45)}", 45, position, interrupt: false);
                                                            ClearRow(31, position, 10);
                                                            quantity = GetIntCharByChar();
                                                            if (quantity != null) {
                                                                if (quantity > nums_goods_left) {
                                                                    ClearRow(85, position, 23);
                                                                    Alert("Not enough goods left, please try again.", 45, position, ConsoleColor.Red, interrupt: false);
                                                                }
                                                                else if (quantity < -1) {
                                                                    if (quantity == int.MinValue) {
                                                                        ClearRow(78, position, 30);
                                                                        Alert("Invalid format, please try again.", 45, position, ConsoleColor.Red, interrupt: false);
                                                                    }
                                                                    else {
                                                                        ClearRow(77, position, 31);
                                                                        Alert("Quantity must be greater than 0.", 45, position, ConsoleColor.Red, interrupt: false);
                                                                    }
                                                                }
                                                                else break;
                                                            }
                                                            else 
                                                                break;
                                                        }

                                                        if (quantity == 0)
                                                            orders.RemoveAt(index);
                                                        else if (quantity != null) {
                                                            orders[index].Quantity = (int)quantity;
                                                            break;
                                                        }
                                                    }
                                                    else Alert("Invalid Id.                                     ", 44, 17, ConsoleColor.Red, false);
                                                }
                                                else break;
                                            }

                                            break;

                                        case 3:
                                            Alert("- REMOVE PRODUCT -", 40, 15, interrupt: false);
                                            Alert("Enter Product Id:", 8, 17, ConsoleColor.Yellow, false);
                                            Alert("<- Input must be between 0 and 2,147,483,647.", 44, 17, ConsoleColor.Green, false);

                                            while (true) {
                                                ClearRow(26, 17, 10);
                                                int? id = GetIntCharByChar();

                                                if (id != null) {
                                                    order = orders.Find(o => o.Id == id);
                                                    if (order != null) {
                                                        orders.Remove(order);
                                                        break;
                                                    }
                                                    else
                                                        Alert("Product not found.                           ", 44, 17, ConsoleColor.Red);
                                                }
                                                else break;
                                            }

                                            break;

                                        case 4:
                                            Console.Clear();
                                            bill = new() { Cashier_id = cashier.Id, Created_date = DateTime.Now };
                                            bill.PrintInvoice();

                                            while (true) {
                                                Alert("Are you sure to create this bill ? (Y/N)", 40, 24, ConsoleColor.Yellow, false);
                                                ConsoleKey key = Console.ReadKey().Key;
                                                if (key == ConsoleKey.Y) {
                                                    confirm = true;
                                                    break;
                                                }
                                                else if (key == ConsoleKey.N || key == ConsoleKey.Escape) break;
                                                else {
                                                    Alert("Please enter 'Y' or 'N' !", 40, 24, ConsoleColor.Red);
                                                    continue;
                                                }
                                            }
                                            break;
                                    }

                                    if (confirm) {
                                        if (bbll.AddBill(bill, orders)) {
                                            orders.Clear();
                                            Alert("Add Bill successfully!", 4, 18 + orders.Count);
                                        }
                                        else Alert("Some error occurs. Please try again or contact the database maintenance department.", 18, 14, ConsoleColor.Red, cls: true);

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
            PrintButton("2. Create Bill", 40, 40, 14);
            PrintButton("3. View History Transaction", 40, 40, 17);
            PrintButton("4. Report Shift", 40, 40, 20);

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
            Alert("You can login 15 minutes before your shift start\n\n\n    - CHOOSE YOUR SHIFT -", top: 1, interrupt: false, cls: true);

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

        public static void ClearRow(int left, int top, int length = 1) {
            Console.SetCursorPosition(left, top);
            Console.Write(new string(' ', length));
            Console.SetCursorPosition(left, top);
        }

        public static string? GetStrCharByChar(int max_length = 64)
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
                        ClearRow(Console.CursorLeft, Console.CursorTop);
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

                else if (chars.Count < max_length)
                    chars.Push(key.KeyChar);

                else ClearRow(--Console.CursorLeft, Console.CursorTop);
            }
        }

        public static string[]? OpenLoginScreen(int shift)
        {
            Console.CursorVisible = true;
            Alert($"You are logging to shift {shift}", top: 1, interrupt: false, cls: true);
            Console.ResetColor();

            Console.Write("\n\n");
            Console.WriteLine("                                ██       ██████   ██████  ██ ███    ██");
            Console.WriteLine("                                ██      ██    ██ ██       ██ ████   ██");
            Console.WriteLine("                                ██      ██    ██ ██   ███ ██ ██ ██  ██");
            Console.WriteLine("                                ██      ██    ██ ██    ██ ██ ██  ██ ██");
            Console.WriteLine("                                ███████  ██████   ██████  ██ ██   ████\n\n");
            Console.WriteLine("                        ╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("                        ║ Username:                                         ║");
            Console.WriteLine("                        ╚═══════════════════════════════════════════════════╝\n");
            Console.WriteLine("                        ╔═══════════════════════════════════════════════════╗");
            Console.WriteLine("                        ║ Password:                                         ║");
            Console.WriteLine("                        ╚═══════════════════════════════════════════════════╝\n\n\n");
            Console.Write("                                --- Enter 'esc' button to go back ---");


            Console.SetCursorPosition(36, 12);
            string? username = GetStrCharByChar();
            if (username == null)
                return null;

            Console.SetCursorPosition(36, 16);
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

        public static int? GetIntCharByChar(int max_length = 10) { // Return null if user enter 'Esc', and return int.MinValue if int.TryParse() false
            string? str = GetStrCharByChar(max_length);

            if (str == null) return null;

            if (int.TryParse(str, out int id))
                return id;
            
            return int.MinValue;
        }

        public static Cashier? ChooseCashierScreen(List<Cashier> cashiers) {
            Alert("Enter your Cashier Id:", 4, 1, ConsoleColor.Yellow, false, true);

            string str = $"        +----+{new string('-', 34)}+";

            // Print out the cashiers table
            Console.Write("\n\n");
            Console.WriteLine(str);
            Console.WriteLine($"        | Id |               {"Name", -19}|");
            Console.WriteLine(str);

            foreach (Cashier cashier in cashiers)
                Console.WriteLine($"        | {cashier.Id, -3}| {cashier.Name, -33}|");

            Console.WriteLine(str);
            Console.Write("\n\n    --- Enter 'esc' button to go back ---");

            Alert("<- Input must be between 0 and 2,147,483,647.", 45, 1, ConsoleColor.Green, false);
            while (true) {
                ClearRow(27, 1, 10);
                int? id = GetIntCharByChar();

                if (id != null) {
                    if (id > int.MinValue) {
                        if (id > 0) {
                            Cashier? cashier = cashiers.Find(c => c.Id == id);
                            if (cashier != null)
                                return cashier;

                            Alert("Cashier not found.                           ", 45, 1, ConsoleColor.Red, false);
                        }
                        else
                            Alert("Id must be greater than 0.                           ", 45, 1, ConsoleColor.Red, false);
                    }
                    else
                        Alert("Invalid Id.                                  ", 45, 1, ConsoleColor.Red, false);
                }
                else return null;
            }
        }

        public static int CreateBillScreen(Cashier cashier, List<Order> orders) {
            Console.Clear();
            Console.Write("\n\n");
            Console.WriteLine("                      ██████ ██████  ███████  █████  ████████ ███████     ██████  ██ ██      ██");
            Console.WriteLine("                     ██      ██   ██ ██      ██   ██    ██    ██          ██   ██ ██ ██      ██");
            Console.WriteLine("                     ██      ██████  █████   ███████    ██    █████       ██████  ██ ██      ██");
            Console.WriteLine("                     ██      ██   ██ ██      ██   ██    ██    ██          ██   ██ ██ ██      ██");
            Console.WriteLine("                      ██████ ██   ██ ███████ ██   ██    ██    ███████     ██████  ██ ███████ ███████\n\n");
            PrintButton("1. Add Product", 23, 10, 9);
            PrintButton("2. Modify Product", 23, 36, 9);
            PrintButton("3. Remove Product", 23, 62, 9);
            PrintButton("4. Confirm Bill", 23, 88, 9);

            Alert($"Cashier: {cashier.Name}", 4, 13, interrupt: false);
            Console.CursorTop += 5;

            // Print out current products selected
            GoodBLL gbll = new();
    
            string str = $"          +----+{new string('-', 13)}+{new string('-', 11)}+";

            Console.WriteLine(str);
            Console.WriteLine($"          | Id | {"Name", -12}| {"Quantity", -10}|");
            Console.WriteLine(str);

            // An order are only created if 'Good_Id' is valid. So in case of error, it's not the code that causes it
            foreach (Order order in orders) {
                Good? good = gbll.GetGoodById(order.Good_id);
                if (good != null)
                    Console.WriteLine($"          | {order.Id, -3}| {good.Name, -12}| {order.Quantity, -10}|");
                else {
                    Alert("Connect to database failed. Please try again or contact the database maintenance department.", 4, 14, ConsoleColor.Red, cls: true);
                    return int.MinValue;
                }
            }

            if (orders.Count == 0) {
                Console.WriteLine($"          |{new string(' ', 30)}|");
                Console.WriteLine($"          |   THERE'S NO PRODUCTS HERE   |");
                Console.WriteLine($"          |{new string(' ', 30)}|");
            }

            Console.WriteLine(str);
            Console.Write("\n\n\n    --- Enter 'esc' button to go back ---");

            return ReadCmd();
        }

        public static Order? CreateOrder(List<Order> orders) { // Return null if user enter 'Esc'
            GoodBLL gbll = new();

            Alert("- ADD PRODUCT -", top: 1, interrupt: false, cls: true);
            Alert("Enter Product Id:", 8, 4, ConsoleColor.Yellow, false);
            Alert("<- Input must be between 0 and 2,147,483,647.", 44, 4, ConsoleColor.Green, false);

            while (true) {
                ClearRow(26, 4, 10);
                int? id = GetIntCharByChar();

                if (id != null) {
                    if (id > int.MinValue) {
                        if (id > 0) {
                            if (orders.Find(o => o.Good_id == id) == null) {
                                Good? good = gbll.GetGoodById((int)id);
                                if (good != null) {
                                    Alert($"- SELECT -\n    Product's name: {good.Name}    -    In Storage: {good.Quantity}", top: 7, interrupt: false);
                                    Alert("Quantity:", 8, 10, ConsoleColor.Yellow, false);
                                    Alert("<- Input should be greater than 0.", 36, 10, ConsoleColor.Green, false);

                                    while (true) {
                                        ClearRow(18, 10, 10);
                                        int? quantity = GetIntCharByChar();
                                        if (quantity != null) {
                                            if (quantity > int.MinValue) {
                                                if (quantity > 0) {
                                                    if (quantity <= gbll.GetCurrentQuantity((int)id)) // Return Int.MinValue if error happen, so this statement can only true if no error happen.
                                                        return new() { Id = orders.Count + 1, Good_id = (int)id, Quantity = (int)quantity };

                                                    Alert("Not enough product in storage.    ", 36, 10, ConsoleColor.Red, false);
                                                }
                                                else
                                                    Alert("Quantity must be greater than 0   ", 36, 10, ConsoleColor.Red, false);
                                            }
                                            else
                                                Alert("Invalid input.                    ", 36, 10, ConsoleColor.Red, false);
                                        }
                                        else break;
                                    }
                                }
                                else Alert("Product not found.                           ", 44, 4, ConsoleColor.Red, false);
                            }
                            else Alert("Product is already exist in the bill.        ", 44, 4, ConsoleColor.Yellow, false);
                        }
                        else Alert("Product Id must be greater than 0            ", 44, 4, ConsoleColor.Red, false);
                    }
                    else Alert("Invalid Id.                                  ", 44, 4, ConsoleColor.Red, false);
                }
                else return null;
            }
        }
    }
}