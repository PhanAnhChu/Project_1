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

        public Order? GetOrderById(int id)
        {
            try
            {
                Con.Open();
                query = "SELECT * FROM Orders WHERE Id = @id LIMIT 1";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetOrders(Reader)[0];
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

        public static List<Order> GetOrders(MySqlDataReader reader)
        {
            List<Order> list = new();

            while (reader.Read())
            {
                Order order = new()
                {
                    Id = reader.GetInt32("id"),
                    Bill_id = reader.GetInt32("bill_id"),
                    Good_id = reader.GetInt32("good_id"),
                    Quantity = reader.GetInt32("quantity")
                };

                list.Add(order);
            }

            return list;
        }

        public List<Order> GetOrdersFromBill(Bill bill)
        {
            try
            {
                Con.Open();
                query = @"SELECT o.id, bill_id, good_id, quantity FROM Orders o
                          INNER JOIN Bills
                          ON bill_id = @id";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", bill.Id);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetOrders(Reader);
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
    }
}