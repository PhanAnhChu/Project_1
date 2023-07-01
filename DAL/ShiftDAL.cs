using System;
using MySql.Data.MySqlClient;

namespace DAL {
    public class ShiftDAL {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();
        public MySqlDataReader? Reader;

        public bool AddShift() {
            try
            {
                Con.Open();
                MySqlTransaction tran = Con.BeginTransaction();

                try
                {
                    query = "INSERT INTO Shifts(start_time, end_time) VALUES (@start, @end);";

                    MySqlCommand cmd = new(query, Con);
                    cmd.Parameters.AddWithValue("@start", DateTime.Now);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now.AddHours(7));

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    return true;
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