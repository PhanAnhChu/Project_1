using System;
using System.IO;
using MySql.Data.MySqlClient;

namespace DAL {
    public class ShiftDAL {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();

        public bool AddShift(DateTime startTime, DateTime endTime) {
            try
            {
                Con.Open();
                MySqlTransaction tran = Con.BeginTransaction();

                try
                {
                    query = "INSERT INTO Shifts(start_time, end_time) VALUES (@start, @end)";

                    MySqlCommand cmd = new(query, Con);
                    cmd.Parameters.AddWithValue("@start", startTime);
                    cmd.Parameters.AddWithValue("@end", endTime);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    return true;
                }
                catch (Exception ex)
                {
                    File.WriteAllText("log.txt", ex.Message);
                    tran.Rollback();
                    return false;
                }
            } 
            catch (Exception ex)
            {
                File.WriteAllText("log.txt", ex.Message);
                return false;
            }
            finally {
                Con.Close();
            }
        }
    }
}