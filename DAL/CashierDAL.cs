using MySql.Data.MySqlClient;
using Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace DAL
{
    public class CashierDAL
    {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();

        public Cashier? GetCashierByLogin(string username, string password)
        {
            try
            {
                Con.Open();
                query = @"SELECT * FROM Cashiers
                          WHERE username = @username AND password = @password LIMIT 1";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetCashiers(Reader)[0];
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

        public Cashier? GetCashierById(int id)
        {
            try
            {
                Con.Open();
                query = @"SELECT * FROM Cashiers
                          WHERE id = @id LIMIT 1";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetCashiers(Reader)[0];
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

        public List<Cashier> GetCashiers(int page)
        {
            try
            {
                Con.Open();
                query = "SELECT * FROM Cashiers ORDER BY Id LIMIT @page, 10";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@page", page * 10);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetCashiers(Reader);
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

        public static List<Cashier> GetCashiers(MySqlDataReader reader)
        {
            List<Cashier> list = new();

            while (reader.Read())
            {
                Cashier cashier = new()
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Status = reader.GetBoolean("status")
                };

                list.Add(cashier);
            }

            return list;
        }
    }
}