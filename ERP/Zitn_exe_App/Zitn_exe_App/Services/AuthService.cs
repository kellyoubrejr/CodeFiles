using Zitn_exe_App.Data;
using Zitn_exe_App.Utils;

namespace Zitn_exe_App.Services
{
    public class AuthService
    {
        private readonly UserRepository _repo = new UserRepository();

        public string Login(string username, string password)
        {
            var user = _repo.GetUser(username);

            if (user == null)
                return "用户不存在";

            if (user.IsLocked)
                return "账号已锁定";

            string inputHash = EncryptHelper.Encrypt(username, password);

            if (user.PasswordHash != inputHash)
            {
                _repo.IncreaseFail(username);
                _repo.WriteLog(username, false, "密码错误");
                return "密码错误";
            }

            _repo.ResetFail(username);
            _repo.WriteLog(username, true, "登录成功");

            return "OK";
        }
    }
}