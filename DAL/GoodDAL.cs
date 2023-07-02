using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Persistence;

namespace DAL {
    public class GoodDAL {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();
        public MySqlDataReader? Reader;

        public Good? GetGoodsById(int id)
        {
            try
            {
                Con.Open();
                query = "SELECT * FROM Goods WHERE Id = @id LIMIT 1";

                MySqlCommand cmd = new(query, Con);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.Prepare();

                Reader = cmd.ExecuteReader();
                return GetGoods(Reader)[0];
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