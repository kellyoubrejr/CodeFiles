using ZT.EncryptLib;

namespace Zitn_exe_App.Utils
{
    public class EncryptHelper
    {
        public static string Encrypt(string username, string password)
        {
            var u = new LogInUser(username, password);
            return u.Credential;
        }
    }
}
