using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using Persistence;

namespace DAL {
    public class GoodDAL {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();

        public Good? GetGoodsById(int id)
        {
            try
            {
                Con.Open();
                query = "SELECT * FROM Goods WHERE Id = @id LIMIT 1";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.Prepare();

                using MySqlDataReader Reader = cmd.ExecuteReader();
                return GetGoods(Reader)[0];
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

        public static List<Good> GetGoods(MySqlDataReader reader) {
            List<Good> list = new();

            while (reader.Read()) {
                Good good = new()
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Price = reader.GetFloat("price"),
                    Quantity = reader.GetInt32("quantity")
                };

                list.Add(good);
            }

            return list;
        }
    }
}