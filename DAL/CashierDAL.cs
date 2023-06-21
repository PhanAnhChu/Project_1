﻿using MySql.Data.MySqlClient;
using Persistence;
using System.Collections.Generic;

namespace DAL
{
    public class CashierDAL
    {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();
        public MySqlDataReader? Reader;

        public Cashier? GetCashierByLogin(string username, string password)
        {
            try
            {
                Con.Open();
                query = @"SELECT id, name FROM Cashiers
                          INNER JOIN Accounts ON id = cashier_id
                          WHERE username = @username AND password = @password LIMIT 1";


                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                cmd.Prepare();

                Reader = cmd.ExecuteReader();
                return GetCashiers(Reader)[0];
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

        public List<Cashier> GetCashiers(int page)
        {
            try
            {
                Con.Open();
                query = "SELECT * FROM Cashiers ORDER BY Id LIMIT @page, 10";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@page", page * 10);

                cmd.Prepare();

                Reader = cmd.ExecuteReader();
                return GetCashiers(Reader);
            }
            catch // (Exception ex)
            {
                // OPTIONAL CODE
            }
            finally
            {
                Con.Close();
                Reader?.Close();
            }
            return new List<Cashier>();
        }

        public static List<Cashier> GetCashiers(MySqlDataReader reader)
        {
            List<Cashier> list = new();

            while (reader.Read())
            {
                Cashier cashier = new()
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name")
                };

                list.Add(cashier);
            }

            return list;
        }
    }
}