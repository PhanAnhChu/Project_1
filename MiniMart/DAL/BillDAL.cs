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

        public List<Bill> GetBills(int page)
        {
            try
            {
                Con.Open();
                query = "SELECT * FROM Bills ORDER BY Id Desc LIMIT 10 OFFSET @page";

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

        public static List<Bill> GetBills(MySqlDataReader reader)
        {
            List<Bill> list = new();

            while (reader.Read()) {
                Bill bill = new()
                {
                    Id = reader.GetInt32("id"),
                    Cashier_id = reader.GetInt32("cashier_id"),
                    Created_date = reader.GetDateTime("created_date")
                };

                if (reader.IsDBNull(3))
                    bill.Customer_id = null;
                else
                    bill.Customer_id = reader.GetInt32("customer_id");

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
                    query = "INSERT INTO Bills(cashier_id, created_date, customer_id) VALUES (@c_id, @date, @customer)";

                    MySqlCommand cmd = new(query, Con);
                    cmd.Parameters.AddWithValue("@c_id", bill.Cashier_id);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@customer", bill.Customer_id);

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

                    query = "INSERT INTO Orders(bill_id, good_id, quantity, price) VALUES ";
                    List<MySqlParameter> parameters = new();

                    for (int i = 0; i < orders.Count; ++i) { 
                        cmd.Parameters.Clear();
                        Order order = orders[i];
                        query += $"(@bill_id{i}, @good_id{i}, @quantity{i}, @price{i}), ";

                        parameters.Add(new MySqlParameter($"@bill_id{i}", id));
                        parameters.Add(new MySqlParameter($"@good_id{i}", order.Good_id));
                        parameters.Add(new MySqlParameter($"@quantity{i}", order.Quantity));
                        parameters.Add(new MySqlParameter($"@price{i}", order.Price));

                        cmd.CommandText = "UPDATE Goods SET Quantity = Quantity - @quan WHERE Id = @id;";
                        cmd.Parameters.AddWithValue("@quan", order.Quantity);
                        cmd.Parameters.AddWithValue("@id", order.Good_id);

                        cmd.Prepare();
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

        public float CheckTotalPrice(Bill bill) {
            try {
                Con.Open();
                query = @"SELECT SUM(orders.price * orders.quantity) AS total
                          FROM orders WHERE orders.bill_id = @id;";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", bill.Id);

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
    }
}