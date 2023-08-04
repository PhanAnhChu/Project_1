using MySql.Data.MySqlClient;
using Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace DAL
{
    public class CustomerDAL
    {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();

        public Customer? GetCustomerById(int id)
        {
            try
            {
                Con.Open();
                query = @"SELECT * FROM Customers
                          WHERE id = @id LIMIT 1";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetCustomers(Reader)[0];
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

        public List<Customer> GetCustomers(int page)
        {
            try
            {
                Con.Open();
                query = "SELECT * FROM Customers ORDER BY Id LIMIT @page, 10";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@page", page * 10);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetCustomers(Reader);
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

        public static List<Customer> GetCustomers(MySqlDataReader reader)
        {
            List<Customer> list = new();

            while (reader.Read())
            {
                Customer Customer = new()
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Phone = reader.GetString("phone"),
                    Reward_Point = reader.GetInt32("reward_point")
                };

                list.Add(Customer);
            }

            return list;
        }

        public bool AddCustomer(Customer customer) {
            try
            {
                Con.Open();
                MySqlTransaction tran = Con.BeginTransaction();

                try
                {
                    query = "INSERT INTO Customers(name, phone, reward_point) VALUES (@name, @phone, @point)";

                    MySqlCommand cmd = new(query, Con);
                    cmd.Parameters.AddWithValue("@name", customer.Name);
                    cmd.Parameters.AddWithValue("@phone", customer.Phone);
                    cmd.Parameters.AddWithValue("@point", customer.Reward_Point);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    tran.Commit();
                    return true;
                }
                catch {
                    tran.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}\n");
                return false;
            }
            finally {
                Con.Close();
            }
        }
    }
}