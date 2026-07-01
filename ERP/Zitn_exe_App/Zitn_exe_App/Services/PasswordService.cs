using System;
using Microsoft.Data.SqlClient;
using Zitn_exe_App.Data;
using Zitn_exe_App.Utils;

namespace Zitn_exe_App.Services
{
    public class PasswordService
    {
        public string ChangePassword(string username, string oldPassword, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(DbHelper.ConnStr))
            {
                conn.Open();

                string sql = "SELECT password_hash FROM user_table WHERE username=@username";

                string dbPwd = null;

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    var result = cmd.ExecuteScalar();

                    if (result == null)
                        return "用户不存在";

                    dbPwd = result.ToString();
                }

                string oldHash = EncryptHelper.Encrypt(username, oldPassword);

                if (dbPwd != oldHash)
                    return "旧密码错误";

                string newHash = EncryptHelper.Encrypt(username, newPassword);

                string updateSql = @"
                                    UPDATE user_table
                                    SET password_hash = @pwd
                                    WHERE username = @username";

                using (var cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("@pwd", newHash);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.ExecuteNonQuery();
                }

                return "OK";
            }
        }
    }
}
