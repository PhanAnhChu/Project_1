using MySql.Data.MySqlClient;
using System.IO;

namespace DAL {
    public class DbConfig {
        static readonly MySqlConnection Con = new();

        public static MySqlConnection GetConnectionByQuery(string connectionStr = "server=localhost;userid=root;password=Steins@Gate@2;port=3306;database=MiniMart")
        {
            Con.ConnectionString = connectionStr;
            return Con;
        }

        public static MySqlConnection GetConnection() {
            try { Con.ConnectionString = File.ReadAllText("DbConfig.txt"); }
            catch {}

            return GetConnectionByQuery();
        }
    }
}