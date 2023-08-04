using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using Persistence;

namespace DAL {
    public class ShiftDAL {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();

        public bool AddShift(DateTime startTime, DateTime endTime, int reporter_id, float expected, float actual, int confirmer_id) {
            try
            {
                Con.Open();
                MySqlTransaction tran = Con.BeginTransaction();

                try
                {
                    query = "INSERT INTO Shifts(start_time, end_time, reporter_id, expected_income, actual_income, confirmer_id) VALUES (@start, @end, @id, @expected, @actual, @confirmer)";

                    MySqlCommand cmd = new(query, Con);
                    cmd.Parameters.AddWithValue("@start", startTime);
                    cmd.Parameters.AddWithValue("@end", endTime);
                    cmd.Parameters.AddWithValue("@id", reporter_id);
                    cmd.Parameters.AddWithValue("@expected", expected);
                    cmd.Parameters.AddWithValue("@actual", actual);
                    cmd.Parameters.AddWithValue("@confirmer", confirmer_id);

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

        public static float CheckTotalIncome(Shift shift) {
            try {
                BillDAL bdal = new();
                List<Bill> bills = bdal.GetBillsFromShift(shift);

                float total = 0;
                foreach (Bill bill in bills)
                    total += bdal.CheckTotalPrice(bill);
                
                return total;
            }
            catch (Exception ex) {
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}\n");
                return float.MinValue;
            }
        }
    }
}