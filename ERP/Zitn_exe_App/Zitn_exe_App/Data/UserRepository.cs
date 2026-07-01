using Microsoft.Data.SqlClient;
using Zitn_exe_App.Models;

namespace Zitn_exe_App.Data
{
    public class UserRepository
    {
        public UserModel GetUser(string username)
        {
            using (SqlConnection conn = new SqlConnection(DbHelper.ConnStr))
            {
                conn.Open();

                string sql = @"
                            SELECT id, password_hash, is_locked, failed_count
                            FROM user_table
                            WHERE username = @username";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        if (!reader.Read())
                            return null;

                        return new UserModel
                        {
                            Id = reader.GetInt32(0),
                            PasswordHash = reader.GetString(1),
                            IsLocked = reader.GetBoolean(2),
                            FailedCount = reader.GetInt32(3)
                        };
                    }
                }
            } 
        }

        /// <summary>
        /// 增加失败次数，并根据失败次数判断是否锁定账号
        /// </summary>
        /// <param name="username"> 用户 </param>
        public void IncreaseFail(string username)
        {
            using (SqlConnection conn = new SqlConnection(DbHelper.ConnStr))
            {
                conn.Open();

                string sql = @"
                            UPDATE user_table
                            SET failed_count = failed_count + 1,
                                is_locked = CASE 
                                    WHEN failed_count + 1 >= 5 THEN 1 
                                    ELSE 0 
                                END
                            WHERE username = @username";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.ExecuteNonQuery();
                }                
            }            
        }

        /// <summary>
        /// 重置失败次数，并更新最后登录时间
        /// </summary>
        /// <param name="username">用户</param>
        public void ResetFail(string username)
        {
            using (SqlConnection conn = new SqlConnection(DbHelper.ConnStr))
            {
                conn.Open();

                string sql = @"
                            UPDATE user_table
                            SET failed_count = 0,
                                last_login = GETDATE()
                            WHERE username = @username";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 日志
        /// </summary>
        /// <param name="username">用户</param>
        /// <param name="success">标识</param>
        /// <param name="remark">描述</param>
        public void WriteLog(string username, bool success, string remark)
        {
            using (SqlConnection conn = new SqlConnection(DbHelper.ConnStr))
            {
                conn.Open();

                string sql = @"
                            INSERT INTO login_log(username, is_success, remark)
                            VALUES(@username, @is_success, @remark)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@is_success", success);
                    cmd.Parameters.AddWithValue("@remark", remark);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}