using System;
using System.Collections.Generic;
using System.Linq;
using BLL;
using Persistence;

namespace ConsoleApp
{
    public class Program
    {
        static DateTime BeginTime = DateTime.Now;
        static TimeSpan now = BeginTime.TimeOfDay;

        // Control the start/end time of shift 1 & 2
        static readonly TimeSpan startTime1 = new(8, 0, 0);
        static readonly TimeSpan startTime2 = new(15, 0, 0);
        static readonly TimeSpan endTime = new(22, 0, 0);

        public static void Main()
        {
            bool running = true; // The Program will exit when this is set to 'false'
            int screen = 1; // Define the current screen
            List<List<Cashier>> loginList = new() { new(), new() }; // Store cashiers who login to shift 1 & 2
            List<Order> orders = new();

            // Interact with database
            BillBLL bbll = new(); // Do things with Bill
            ShiftBLL sbll = new(); // Do things with Shift
            while (running)
            {
                switch (screen)
                {
                    case 0: // Main Menu
                        orders.Clear();
                        screen = MainMenu(GetLoginStatus(loginList));
                        break;

                    case 1: // Choose Shift, then Login
                        if (IsLoginTime()) { // If currently is not valid time to login, alert user immediately
                            while (true) {
                                int shift = (DateTime.Now.TimeOfDay > startTime2.Add(new(0, -15, 0))) ? 2 : 1;
                                
                                Cashier? cashier = LoginScreen(shift); // Return null if User enter 'Esc'
                                if (cashier != null)
                                {
                                    if (cashier.Id != -1)
                                    {
                                        if (cashier.Status) {
                                            loginList[--shift].Add(cashier);

                                            screen = 0;
                                            Alert($"Login successfully. Hello {cashier.Name}.", 4, 27);
                                            break;
                                        }
                                        else if (Alert("ACCOUNT DON'T HAVE PERMISSION TO LOGIN!", 4, 27, ConsoleColor.Red) == ConsoleKey.Escape)
                                            break;
                                    }
                                    else if (Alert("ACCOUNT NOT FOUND!", 4, 27, ConsoleColor.Red) == ConsoleKey.Escape)
                                        break;
                                }
                                else {
                                    screen = 0;
                                    break;
                                }
                            }
                        }
                        else {
                            Alert("Invalid login time! Please try again later.", 4, 18, ConsoleColor.Red);
                            screen = 0;
                        }

                        break;

                    case 2: // Create Bill
                        ConsoleKey key = ConsoleKey.NoName;
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

                                Bill new_bill = new();
                                bool confirm = false;
                                int current_page = 1;
                                int pos = 1;
                                orders.Clear();

                                while (cashier != null) {
                                    key = CreateBillScreen(cashier, orders, current_page, pos, key);
                                    if (key == ConsoleKey.Escape)
                                        break;

                                    else if (key == ConsoleKey.A) {
                                        Order? order = CreateOrder(orders);
                                        if (order != null) {
                                            orders.Add(order);
                                            pos = 1;
                                        }
                                    }

                                    else if (key == ConsoleKey.C) {
                                        Console.CursorVisible = false;
                                        Alert("Are you sure to create this bill ? (Y/N)", 40, 12, ConsoleColor.Yellow, false, true);
                                        confirm = GetYNKey();
                                        if (confirm) {
                                            new_bill.Cashier_id = cashier.Id;
                                            break;
                                        }
                                    }

                                    else if (key == ConsoleKey.NoName)
                                        pos = (orders.Count > 0) ? 1 : 0;

                                    else if (key == ConsoleKey.UpArrow) --pos;
                                    else if (key == ConsoleKey.DownArrow) ++pos;
                                    else if (key == ConsoleKey.RightArrow) {
                                        pos = 1;
                                        ++current_page;
                                    }
                                    else if (key == ConsoleKey.LeftArrow) {
                                        pos = 1;
                                        --current_page;
                                    }
                                }

                                if (confirm) {
                                    Alert("Enter customer Id: ", color: ConsoleColor.Yellow, interrupt: false, cls: true);
                                    Alert("<- 1 if the customer is not our member", 35, interrupt: false);
                                    do {
                                        if (new_bill.Customer_id == int.MinValue)
                                            Alert("<- Invalid format                     ", 35, color: ConsoleColor.Red, interrupt: false);
                                        else if (new_bill.Customer_id < 1)
                                            Alert("<- Customer Id must be greater than 0 ", 35, color: ConsoleColor.Yellow, interrupt: false);

                                        ClearRow(23, 4, 6);
                                        new_bill.Customer_id = GetIntCharByChar(6);
                                    }
                                    while (new_bill.Customer_id == null || new_bill.Customer_id < 1);

                                    new_bill.Created_date = DateTime.Now;

                                    if (bbll.AddBill(new_bill, orders)) {
                                        Alert("Add Bill successfully!", 4, 18 + orders.Count);
                                        if (!new_bill.PrintInvoice(orders))
                                            Alert("Connect to database failed. Please try again or contact the database maintenance department.", 4, 14, ConsoleColor.Red, cls: true);
                                    }
                                    else Alert("Some error occurs. Please try again or contact the database maintenance department.", 18, 14, ConsoleColor.Red, cls: true);

                                    break;
                                }
                            }
                        }

                        screen = 0;
                        break;

                    case 3:
                        key = ConsoleKey.NoName;
                        int page = 0;
                        int idx = 1; // Range: 1 -> 10

                        while (key != ConsoleKey.Escape) {
                            key = PrintBillList(page, idx);
                            if (key == ConsoleKey.UpArrow) --idx;
                            
                            else if (key == ConsoleKey.DownArrow) ++idx;

                            else if (key == ConsoleKey.LeftArrow) {
                                --page;
                                idx = 1;
                            }
                            else if (key == ConsoleKey.RightArrow) {
                                ++page;
                                idx = 1;
                            }
                            else if (key == ConsoleKey.Enter) {
                                Bill bill = bbll.GetBills(page)[idx - 1];
                                bill.PrintInvoice(orders);
                            }
                            else screen = 0;
                        }

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
                                float total = bbll.CheckTotalIncome(bbll.GetBillsFromInterval(BeginTime, DateTime.Now));
                                Alert($"Expected Income from this Shift: {total} VND", interrupt: false, cls: true);
                                Console.Write("    Enter the Actual Income of this Shift: ");

                                if (float.TryParse(Console.ReadLine(), out float actual))
                                {
                                    Console.Write("\n\n\n    Enter the Confirmer Id: ");
                                    int? i = GetIntCharByChar(7);
                                    if (i != null) {
                                        Cashier? cashier = LoginScreen(GetCurrentShiftValue());
                                        if (cashier == null)
                                            continue;
                                        
                                        else if (cashier.Id == i) {
                                            Alert($"Reporter: {reporter.Name} - Id: {reporter.Id}\nConfirmer: {cashier.Name} - Id: {cashier.Name}\nAre you sure to report this ? (Y/N)", 4, 10, ConsoleColor.Yellow, cls: true);

                                            if (GetYNKey()) {
                                                if (sbll.AddShift(BeginTime, DateTime.Now, reporter.Id, total, actual, cashier.Id)) {
                                                    ClearRow(Console.CursorLeft, Console.CursorTop);
                                                    list.Clear();

                                                    BeginTime = DateTime.Now;
                                                    Alert("Report successfully!", 4, 13);
                                                    if (GetCurrentShiftValue() == 2) {
                                                        loginList[1].Add(cashier);
                                                        loginList[0].Clear();
                                                    }
                                                    break;
                                                }
                                                Alert("Some error occurs. Please try again or contact the maintenance department.", 18, 14, ConsoleColor.Red, cls: true);
                                            }
                                            else break;
                                        }
                                        else Alert("Confirmer Id is not valid.", 4, 13, ConsoleColor.Red);
                                    }
                                }
                            }
                        }

                        screen = 0;
                        break;
    
                    default:
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

        public static int MainMenu(int status = 1)
        {
            Console.Clear();
            Console.CursorVisible = false;
            Console.Write("\n\n\n");
            Console.WriteLine("                  ██████  █████  ███████ ██   ██ ██ ███████ ██████      ███    ███ ███████ ███    ██ ██    ██");
            Console.WriteLine("                 ██      ██   ██ ██      ██   ██ ██ ██      ██   ██     ████  ████ ██      ████   ██ ██    ██");
            Console.WriteLine("                 ██      ███████ ███████ ███████ ██ █████   ██████      ██ ████ ██ █████   ██ ██  ██ ██    ██");
            Console.WriteLine("                 ██      ██   ██      ██ ██   ██ ██ ██      ██   ██     ██  ██  ██ ██      ██  ██ ██ ██    ██");
            Console.WriteLine("                  ██████ ██   ██ ███████ ██   ██ ██ ███████ ██   ██     ██      ██ ███████ ██   ████  ██████\n\n\n");

            PrintButton("1. Login", 40, 40, 11);
            if (status == 1) {
                PrintButton("2. Create Bill", 40, 40, 14);
                PrintButton("3. View History Transaction", 40, 40, 17);
                PrintButton("4. Report Shift", 40, 40, 20);

                return ReadCmd();
            }
            else if (status == 2) {
                PrintButton("2. Report Shift", 40, 40, 20);

                return ReadCmd(end: 2);
            }
            else return ReadCmd(end: 1);
        }

        public static bool IsLoginTime() {
            now = DateTime.Now.TimeOfDay;
            return startTime1.Add(new(0, -15, 0)) <= now && now <= endTime;
        }

        public static int GetCurrentShiftValue() {
            now = DateTime.Now.TimeOfDay;
            
            if (now >= startTime1 && now < startTime2)
                return 1;
            else if (now >= startTime2 && now < endTime)
                return 2;
            
            return 0;
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

        public static void ClearRow(int left, int top, int length = 1) {
            Console.SetCursorPosition(left, top);
            Console.Write(new string(' ', length));
            Console.SetCursorPosition(left, top);
        }

        public static string? GetStrCharByChar(int max_length = 64, bool starMode = false)
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

                else if (chars.Count < max_length) {
                    chars.Push(key.KeyChar);
                    if (starMode) {
                        ClearRow(Console.CursorLeft - 1, Console.CursorTop);
                        Console.Write('*');
                    }
                }

                else ClearRow(--Console.CursorLeft, Console.CursorTop);
            }
        }

        public static Cashier? LoginScreen(int shift)
        {
            Console.CursorVisible = true;
            string start_time;
            string end_time;

            if (shift == 1) {
                start_time = startTime1.ToString(@"hh\:mm");
                end_time = startTime2.ToString(@"hh\:mm");
            }
            else {
                start_time = startTime2.ToString(@"hh\:mm");
                end_time = endTime.ToString(@"hh\:mm");
            }

            Alert($"You are logging to shift {shift} ({start_time} - {end_time})", top: 1, interrupt: false, cls: true);
            Console.ResetColor();
            Console.CursorVisible = true;

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


            CashierBLL CBLL = new();

            while (true) {
                ClearRow(36, 12, 39);
                string? username = GetStrCharByChar(39);
                if (username == null)
                    return null;

                ClearRow(36, 16, 39);
                string? password = GetStrCharByChar(39, true);
                if (password == null)
                    return null;
                
                Cashier? cashier = CBLL.GetCashierByLogin(username, password);
                if (cashier == null)
                    return new() { Id = -1 };
                else
                    return cashier;
            }
        }

        public static int? GetIntCharByChar(int max_length = 10) { // Return null if user enter 'Esc', and return int.MinValue if int.TryParse() false
            string? str = GetStrCharByChar(max_length);

            if (str == null) return null;

            if (int.TryParse(str, out int id))
                return id;
            
            return int.MinValue;
        }

        public static Cashier? ChooseCashierScreen(List<Cashier> cashiers) {
            Console.CursorVisible = true;
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

        public static ConsoleKey CreateBillScreen(Cashier cashier, List<Order> orders, int page, int pos, ConsoleKey cmd) {
            Console.Clear();
            Console.CursorVisible = false;
            Console.Write("\n\n");
            Console.WriteLine("                      ██████ ██████  ███████  █████  ████████ ███████     ██████  ██ ██      ██");
            Console.WriteLine("                     ██      ██   ██ ██      ██   ██    ██    ██          ██   ██ ██ ██      ██");
            Console.WriteLine("                     ██      ██████  █████   ███████    ██    █████       ██████  ██ ██      ██");
            Console.WriteLine("                     ██      ██   ██ ██      ██   ██    ██    ██          ██   ██ ██ ██      ██");
            Console.WriteLine("                      ██████ ██   ██ ███████ ██   ██    ██    ███████     ██████  ██ ███████ ███████\n\n");

            Alert("{A}: Add new order          {D}: Delete order          {M}: Modify order          {C}: Confirm bill", 4, 10, interrupt: false);

            Alert($"Cashier: {cashier.Name}", 4, 13, interrupt: false);
            Console.CursorTop += 5;

            // Print out current products selected
            GoodBLL gbll = new();
    
            string str = $"          +----+{new string('-', 13)}+{new string('-', 11)}+{new string('-', 15)}+{new string('-', 15)}+";
            float total = 0;
            int max_page = (int)MathF.Ceiling(orders.Count / 10);
            int page_count = (page < max_page) ? 10 : orders.Count % 10;

            Console.WriteLine(str);
            Console.WriteLine($"          | Id | {"Name", -12}| {"Quantity", -10}|     {"Price", -10}|     {"Total", -10}|");
            int start_pos = Console.CursorTop;
            Console.WriteLine(str);

            Order order;
            Good choosen_good = new() { Id = -1 };

            for (int i = 0; i < page_count; ++i) {
                order = orders[(page - 1)*10 + i];
                Good? good = gbll.GetGoodById(order.Good_id);
                if (good != null) {
                    if (pos == i + 1)
                        choosen_good = good;
    
                    Console.WriteLine($"          | {order.Id, -3}| {good.Name, -12}| {order.Quantity, -10}|{good.Price, 10} VND |{good.Price * order.Quantity, 10} VND |");
                    total += order.Quantity * good.Price;
                }
                else {
                    Alert("Connect to database failed. Please try again or contact the database maintenance department.", 4, 14, ConsoleColor.Red, cls: true);
                    return ConsoleKey.Escape;
                }
            }
    
            for (int i = 0; i < 10 - page_count; ++i)
                Console.WriteLine($"          |    |{' ', 13}|{' ', 11}|{' ', 15}|{' ', 15}|");

            Console.WriteLine(str);
            Console.CursorTop = start_pos + 16;
            Console.WriteLine($"\n{' ', 40}Total: {total} VND");
            Console.WriteLine("\n\n\n    --- Enter 'esc' button to go back ---");

            if (page_count > 0) {
                Console.CursorTop = start_pos + pos;
                Console.ForegroundColor = ConsoleColor.Green;
                order = orders[page*10 + pos - 11];
                Console.Write($"          | {order.Id, -3}| {choosen_good.Name, -12}| {order.Quantity, -10}|{choosen_good.Price, 10} VND |{choosen_good.Price * order.Quantity, 10} VND |");

                if (cmd == ConsoleKey.M) {
                    int nums_goods_left = gbll.GetCurrentQuantity(order.Good_id);
                    Console.CursorVisible = true;
                    Alert($"<- In storage: {nums_goods_left}. Modify quantity to 0 will remove the product", 75, Console.CursorTop, interrupt: false);
                    while (true) {
                        ClearRow(31, Console.CursorTop - 1, 10);
                        int? quantity = GetIntCharByChar();
                        if (quantity != null) {
                            if (nums_goods_left == int.MinValue) {
                                Alert("Some error occurs. Please try again or contact the database maintenance department.", 18, 14, ConsoleColor.Red, cls: true);
                                break;
                            }

                            if (quantity > nums_goods_left) {
                                ClearRow(75, Console.CursorTop, 78);
                                Alert("Not enough goods left, please try again.", 75, Console.CursorTop, ConsoleColor.Red, interrupt: false);
                            }
                            else if (quantity < 0) {
                                if (quantity == int.MinValue) {
                                    ClearRow(75, Console.CursorTop, 78);
                                    Alert("Invalid format, please try again.", 75, Console.CursorTop, ConsoleColor.Red, interrupt: false);
                                }
                                else {
                                    ClearRow(75, Console.CursorTop, 78);
                                    Alert("Quantity must be greater than 0.", 75, Console.CursorTop, ConsoleColor.Red, interrupt: false);
                                }
                            }
                            else if (quantity == 0) {
                                cmd = ConsoleKey.D;
                                break;
                            }
                            else {
                                order.Quantity = (int)quantity;
                                return ConsoleKey.N;
                            }
                        }
                        else break;
                    }
                }

                Console.ResetColor();

                if (cmd == ConsoleKey.D) {
                    orders.Remove(order);
                    return ConsoleKey.NoName;
                }
            }

            Console.ResetColor();
            ConsoleKey key;
            do {
                key = Console.ReadKey().Key;

                if ((key == ConsoleKey.D || key == ConsoleKey.M) && page_count > 0)
                    return key;

                if ((key == ConsoleKey.DownArrow && pos < page_count) || (key == ConsoleKey.UpArrow && pos > 1))
                    return key;
                
                if (key == ConsoleKey.Escape || key == ConsoleKey.A || key == ConsoleKey.C)
                    return key;
                
                if ((key == ConsoleKey.RightArrow && page < max_page) || (key == ConsoleKey.LeftArrow && page > 1))
                    return key;
                
                if (!(key == ConsoleKey.DownArrow || key == ConsoleKey.UpArrow || key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow) && key != ConsoleKey.Enter)
                    ClearRow(--Console.CursorLeft, Console.CursorTop);
            }
            while (true);
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
                                        if (quantity < 0)
                                            Alert("Invalid input.                    ", 36, 10, ConsoleColor.Red, false);
                                        else if (quantity <= gbll.GetCurrentQuantity((int)id)) // Return Int.MinValue if error happen, so this statement can only true if no error happen.
                                            return new() { Id = (orders.Count > 0) ? orders.Max(o => o.Id) + 1 : 1, Good_id = good.Id, Quantity = (int)quantity, Price = good.Price };
                                        else
                                            Alert("Not enough product in storage.    ", 36, 10, ConsoleColor.Red, false);
                                    }
                                    else break;
                                }
                            }
                            else Alert("Product not found.                           ", 44, 4, ConsoleColor.Red, false);
                        }
                        else Alert("Product is already exist in the bill.        ", 44, 4, ConsoleColor.Yellow, false);
                    }
                    else Alert("Invalid Id.                                  ", 44, 4, ConsoleColor.Red, false);
                }
                else return null;
            }
        }
    
        public static ConsoleKey PrintBillList(int page, int pos = 1) {
            Console.CursorVisible = false;
            Console.Clear();
            BillBLL bbll = new();
            List<Bill> bills = bbll.GetBills(page);
            

            Console.Write("\n\n");
            Console.WriteLine("                         ██   ██ ██ ███████ ████████  ██████  ██████  ██    ██");
            Console.WriteLine("                         ██   ██ ██ ██         ██    ██    ██ ██   ██  ██  ██");
            Console.WriteLine("                         ███████ ██ ███████    ██    ██    ██ ██████    ████");
            Console.WriteLine("                         ██   ██ ██      ██    ██    ██    ██ ██   ██    ██");
            Console.WriteLine("                         ██   ██ ██ ███████    ██     ██████  ██   ██    ██\n\n\n\n");

            Alert("Use arrow key to interact with the screen", 4, 8, interrupt: false);

            string str = $"          +----+{new string('-', 30)}+{new string('-', 15)}+";

            Console.WriteLine(str);
            Console.WriteLine($"          | Id |         {"Created Date", -21}|  {"Total Price", -13}|");
            Console.WriteLine(str);

            for (int i = 0; i < bills.Count; ++i) {
                Bill b = bills[i];
                if (i == pos - 1)
                    Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"          | {b.Id, -3}| {b.Created_date, -29}| {bbll.CheckTotalPrice(b), -14}|");

                Console.ResetColor();
            }

            if (bills.Count == 0) {
                Console.WriteLine($"          |{new string(' ', 51)}|");
                Console.WriteLine("          |               THERE'S NO BILLS HERE               |");
                Console.WriteLine($"          |{new string(' ', 51)}|");
            }


            bool right_exist = false, left_exist = false;

            Console.WriteLine(str);
            if (page > 0) {
                Alert("<<", 10, 19 + bills.Count, ConsoleColor.White, false);
                left_exist = true;
            }
            
            PrintButton($"{page + 1}", 6, 15, 18 + bills.Count);

            if (bills.Count == 10) {
                Alert(">>", 10, 19 + bills.Count, ConsoleColor.White, false);
                right_exist = true;
            }

            Console.Write("\n\n\n    --- Enter 'esc' button to go back ---");

            ConsoleKey key;
            while (true) {
                key = Console.ReadKey().Key;

                if (key == ConsoleKey.Escape)
                    return key;
                
                if (key == ConsoleKey.Enter && pos <= bills.Count)
                    return key;

                if (key == ConsoleKey.RightArrow && right_exist)
                    return key;
                
                if (key == ConsoleKey.LeftArrow && left_exist)
                    return key;
                
                if (key == ConsoleKey.DownArrow && bills.Count > pos)
                    return key;
                
                if (key == ConsoleKey.UpArrow && pos > 1)
                    return key;
            }
        }
    
        public static int GetLoginStatus(List<List<Cashier>> loginList) { // 1 -> Inside shift; 2 -> Report Time; 0 -> No one login
            int i = GetCurrentShiftValue();
            if (i == 1 && loginList[0].Count > 0 || i == 2 && loginList[1].Count > 0)
                return 1;
            
            if (i < 2 && loginList[1].Count > 0 || i == 2 && loginList[0].Count > 0)
                return 2;
            
            return 0;
        }

        public static bool GetYNKey() {
            char key;
            while (true) {
                key = char.ToLower(Console.ReadKey().KeyChar);
                if (key == 'y')
                    return true;
                
                if (key == 'n')
                    return false;
            }
        }
    }
}