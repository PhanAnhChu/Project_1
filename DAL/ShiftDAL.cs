using System;
using System.Collections.Generic;
using System.IO;
using BLL;
using MySql.Data.MySqlClient;
using Persistence;

namespace DAL {
    public class ShiftDAL {
        string? query;
        public MySqlConnection Con = DbConfig.GetConnection();

        public bool AddShift(DateTime startTime, DateTime endTime, int id, float expected, float actual) {
            try
            {
                Con.Open();
                MySqlTransaction tran = Con.BeginTransaction();

                try
                {
                    query = "INSERT INTO Shifts(start_time, end_time, reporter_id, expected_income, actual_income) VALUES (@start, @end, @id, @expected, @actual)";

                    MySqlCommand cmd = new(query, Con);
                    cmd.Parameters.AddWithValue("@start", startTime);
                    cmd.Parameters.AddWithValue("@end", endTime);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@expected", expected);
                    cmd.Parameters.AddWithValue("@actual", actual);

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
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}");
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
                    total += bdal.CheckTotalPrice(bill.Id);
                
                return total;
            }
            catch (Exception ex) {
                File.AppendAllText("log.txt", $"{DateTime.Now} : {ex.Message}");
                return float.MinValue;
            }
        }
    }
}