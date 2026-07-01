using System.Collections.Generic;
using System;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Zitn_exe_App.Data
{
    public class DbHelper
    {
        public static string ConnStr =
        "Data Source=10.0.64.19;Database=AIS20250725222335;User Id=sa;PWD=1qaz@WSX#EDC;Encrypt=False;TrustServerCertificate=True;";
        //"Data Source=10.2.96.123;Database=AIS0000000000;User Id=sa;PWD=Kd@123;Encrypt=False;TrustServerCertificate=True;";


        /// <summary>
        /// 执行非查询SQL（INSERT/UPDATE/DELETE）
        /// </summary>
        public static int ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 执行查询，返回DataTable
        /// </summary>
        public static DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询，返回第一行第一列的值
        /// </summary>
        public static object ExecuteScalar(string sql, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }
                    return cmd.ExecuteScalar();
                }
            }
        }
    }
}
