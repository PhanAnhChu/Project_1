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
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}");
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
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}");
                return new();
            }
            finally {
                Con.Close();
            }
        }

        public List<Bill> GetBillsFromShift(Shift shift) {
            try {
                Con.Open();
                query = @"SELECT * FROM Bills WHERE Bills.created_date BETWEEN @start AND @end";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@start", shift.StartTime);
                cmd.Parameters.AddWithValue("@end", shift.EndTime);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetBills(Reader);
            }
            catch (Exception ex) {
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}");
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
                    OrderDAL odal = new();
                    query = "INSERT INTO Bills(cashier_id, created_date) VALUES (@c_id, @date);";

                    int id = bill.Id;

                    MySqlCommand cmd = new(query, Con);
                    cmd.Parameters.AddWithValue("@c_id", id);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now); // .ToString("yyyy-MM-dd")
                    // cmd.Parameters.AddWithValue("@name", bill.Customer_name);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    if (odal.AddOrders(orders, id)) {
                        tran.Commit();
                        return true;
                    }

                    throw new Exception("Add orders failed!");
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            } 
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}");
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
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}");
                return float.MinValue;
            }
            finally {
                Con.Close();
            }
        }
    }
}