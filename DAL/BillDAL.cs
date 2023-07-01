using MySql.Data.MySqlClient;
using Persistence;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class BillDAL
    {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();
        public MySqlDataReader? Reader;

        public Bill? GetBillById(int id)
        {
            try
            {
                Con.Open();
                query = "SELECT * FROM Bills WHERE Id = @id";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.Prepare();

                Reader = cmd.ExecuteReader();
                return GetBills(Reader)[0];
            }
            catch // (Exception ex)
            {
                // Console.WriteLine(ex.Message);
            }
            finally
            {
                Con.Close();
                Reader?.Close();
            }
            return null;
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

                Reader = cmd.ExecuteReader();
                return GetBills(Reader);
            }
            catch // (Exception ex)
            {
                // Console.WriteLine(ex.Message);
            }
            finally
            {
                Con.Close();
                Reader?.Close();
            }
            return new List<Bill>();
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

                    tran.Rollback();
                    return false;
                }
                catch // (Exception ex)
                {
                    tran.Rollback();
                    return false;
                }
            } 
            catch { return false; }
        }
    }
}