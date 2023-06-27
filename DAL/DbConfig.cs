using MySql.Data.MySqlClient;
using System.IO;

namespace DAL {
    public class DbConfig {
        static readonly MySqlConnection Con = new();
        // khi kết nối nó nhận diện duy nhất dòng đầu tiên ( đây là lý do hôm qua đổi tài khoản nhưng vẫn ko thể chạy đc)
        public static MySqlConnection GetConnectionByQuery(string connectionStr = "server=localhost;userid=root;password=mesterJyn@1;port=3306;database=minimart")
        {
            Con.ConnectionString = connectionStr;
            return Con;
        }

        public static MySqlConnection GetConnection()
        {
            try { Con.ConnectionString = File.ReadAllText("DbConfig.txt"); }
            catch {}

            return GetConnectionByQuery();
        }
    }
}