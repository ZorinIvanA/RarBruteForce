using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RarBrute.PasswordFinder.BL
{
    public class PasswordTryEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string UsedPassword { get; set; }
        public string Error { get; set; }

        public PasswordTryEventArgs(string usedPassword, string error, bool success = false)
        {
            if (string.IsNullOrWhiteSpace(usedPassword))
                throw new ArgumentNullException(nameof(usedPassword));

            UsedPassword = usedPassword;
            Success = success;
            Error = error ?? string.Empty;
        }
    }
}
