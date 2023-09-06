using MySql.Data.MySqlClient;
using Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace DAL
{
    public class OrderDAL
    {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();

        public List<Order> GetOrdersFromBill(Bill bill)
        {
            try
            {
                Con.Open();
                query = "SELECT DISTINCT Orders.* FROM Orders INNER JOIN Bills ON bill_id = @id";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", bill.Id);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetOrders(Reader);
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

        public static List<Order> GetOrders(MySqlDataReader reader)
        {
            List<Order> list = new();

            while (reader.Read())
            {
                list.Add(new()
                {
                    Id = reader.GetInt32("id"),
                    Bill_id = reader.GetInt32("bill_id"),
                    Good_id = reader.GetInt32("good_id"),
                    Quantity = reader.GetInt32("quantity"),
                    Price = reader.GetFloat("price")
                });
            }

            return list;
        }
    }
}