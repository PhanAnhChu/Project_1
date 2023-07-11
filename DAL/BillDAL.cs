using MySql.Data.MySqlClient;
using Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace DAL
{
    public class BillDAL
    {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();

        public Bill? GetBillById(int id)
        {
            try
            {
                Con.Open();
                query = "SELECT * FROM Bills WHERE Id = @id LIMIT 1";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetBills(Reader)[0];
            }
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}\n");
                return null;
            }
            finally {
                Con.Close();
            }
        }

        public List<Bill> GetBills(int page)
        {
            try
            {
                Con.Open();
                query = "SELECT * FROM Bills ORDER BY Id Desc LIMIT @page, 10";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@page", page*10);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetBills(Reader);
            }
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}\n");
                return new();
            }
            finally {
                Con.Close();
            }
        }

        public List<Bill> GetBillsFromInterval(DateTime startTime, DateTime endTime) {
            try {
                Con.Open();
                query = @"SELECT * FROM Bills WHERE Bills.created_date BETWEEN @start AND @end";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@start", startTime);
                cmd.Parameters.AddWithValue("@end", endTime);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetBills(Reader);
            }
            catch (Exception ex) {
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}\n");
                return new();
            }
            finally {
                Con.Close();
            }
        }

        public List<Bill> GetBillsFromShift(Shift shift) => GetBillsFromInterval(shift.StartTime, shift.EndTime);

        public static List<Bill> GetBills(MySqlDataReader reader)
        {
            List<Bill> list = new();

            while (reader.Read()) {
                Bill bill = new()
                {
                    Id = reader.GetInt32("id"),
                    Cashier_id = reader.GetInt32("cashier_id"),
                    Created_date = reader.GetDateTime("created_date"),
                    // Customer_name = reader.GetString("customer_name")
                };

                list.Add(bill);
            }

            return list;
        }

        public bool AddBill(Bill bill, List<Order> orders)
        {
            try
            {
                Con.Open();
                MySqlTransaction tran = Con.BeginTransaction();

                try
                {
                    int id = 0;
                    query = "INSERT INTO Bills(cashier_id, created_date) VALUES (@c_id, @date)";

                    MySqlCommand cmd = new(query, Con);
                    cmd.Parameters.AddWithValue("@c_id", bill.Cashier_id);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now); // .ToString("yyyy-MM-dd")
                    // cmd.Parameters.AddWithValue("@name", bill.Customer_name);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();

                    cmd.CommandText = "SELECT Id FROM Bills ORDER BY Id Desc LIMIT 1";
                    cmd.Prepare();

                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read()) {
                        id = reader.GetInt32("id");
                        break;
                    }
                    reader.Close();
    
                    Console.WriteLine(id);

                    query = "INSERT INTO Orders(bill_id, good_id, quantity) VALUES ";
                    List<MySqlParameter> parameters = new();

                    for (int i = 0; i < orders.Count; ++i) { 
                        cmd.Parameters.Clear();
                        Order order = orders[i];
                        query += $"(@bill_id{i}, @good_id{i}, @quantity{i}), ";

                        parameters.Add(new MySqlParameter($"@bill_id{i}", id));
                        parameters.Add(new MySqlParameter($"@good_id{i}", order.Good_id));
                        parameters.Add(new MySqlParameter($"@quantity{i}", order.Quantity));

                        cmd.CommandText = "UPDATE Goods SET Quantity = Quantity - @quan WHERE Id = @id;";
                        cmd.Parameters.AddWithValue("@quan", order.Quantity);
                        cmd.Parameters.AddWithValue("@id", order.Good_id);

                        cmd.ExecuteNonQuery();
                    }

                    cmd.Parameters.Clear();

                    cmd.CommandText = query.Remove(query.Length - 2);
                    cmd.Parameters.AddRange(parameters.ToArray());

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    tran.Commit();
                    return true;
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            } 
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}\n - StackTrace: {ex.StackTrace}\n\n");
                return false;
            }
            finally
            {
                Con.Close();
            }
        }

        public float CheckTotalPrice(int bill_id) {
            try {
                Con.Open();
                query = @"SELECT SUM(goods.price * orders.quantity) AS total
                          FROM orders
                          JOIN goods ON orders.good_id = goods.id
                          WHERE orders.bill_id = @id;";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", bill_id);

                cmd.Prepare();
                float result = float.MinValue;
                using MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) {
                    result = reader.GetFloat("total");
                }

                return result;
            }
            catch (Exception ex) {
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}\n");
                return float.MinValue;
            }
            finally {
                Con.Close();
            }
        }

        public float CheckTotalIncome(List<Bill> bills) {
            float total = 0;
            foreach (Bill bill in bills)
                total += CheckTotalPrice(bill.Id);
            
            return total;
        }
    }
}