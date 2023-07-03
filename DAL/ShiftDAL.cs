using System;
using MySql.Data.MySqlClient;

namespace DAL {
    public class ShiftDAL {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();
        public MySqlDataReader? Reader;

        public bool AddShift(DateTime startTime, DateTime endTime) {
            try
            {
                Con.Open();
                MySqlTransaction tran = Con.BeginTransaction();

                try
                {
                    query = "INSERT INTO Shifts(start_time, end_time) VALUES (@start, @end);";

                    MySqlCommand cmd = new(query, Con);
                    cmd.Parameters.AddWithValue("@start", startTime);
                    cmd.Parameters.AddWithValue("@date", endTime);

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