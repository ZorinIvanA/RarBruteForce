using Aspose.Zip.Rar;
using System.Text;

namespace RarBrute.PasswordFinder.BL
{
    public class PasswordFinder
    {
        public string SourceFile { get; set; }
        public string TargetPath { get; set; }
        public int ThreadsCount { get; set; }
        public string AlphabetPath { get; set; }
        public string PasswordsPath { get; set; }

        public string Alphabet { get; set; }

        public string FoundPassword { get; set; } = string.Empty;

        public string[] Passwords { get; set; }

        public event EventHandler<PasswordTryEventArgs> PasswordTry;
        public event EventHandler PasswordsListPrepared;

        public PasswordFinder(string sourceFile,
            string targetPath,
            string alphabetPath,
            string passwordsPath,
            int threadsCount = 1)
        {
            if (string.IsNullOrEmpty(sourceFile))
                throw new ArgumentNullException(nameof(sourceFile));
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException(nameof(targetPath));
            if (string.IsNullOrEmpty(alphabetPath))
                throw new ArgumentNullException(nameof(alphabetPath));
            if (string.IsNullOrWhiteSpace(passwordsPath))
                throw new ArgumentNullException(passwordsPath);

            this.TargetPath = targetPath;
            this.SourceFile = sourceFile;
            this.ThreadsCount = threadsCount;
            this.PasswordsPath = passwordsPath;
            this.AlphabetPath = alphabetPath;

            LoadAlphabet();
        }

        public async Task<string> FindPassword()
        {
            var passwordsList = File.ReadAllLines(PasswordsPath);
            Passwords = ReplaceWildcards(passwordsList);
            PasswordsListPrepared(this, EventArgs.Empty);
            var passwordsPerThread = Passwords.Length / ThreadsCount;

            var tasks = new List<Task>();
            for (var i = 0; i < ThreadsCount; ++i)
            {
                var threadTempI = i;
                tasks.Add(new Task(() => OneTaskRoutine(threadTempI * passwordsPerThread, passwordsPerThread)));
            }

            tasks.ForEach(x => x.Start());

            while (tasks.Count > 0)
            {
                var completed = await Task.WhenAny(tasks);

                if (completed.IsCompletedSuccessfully) // Successful?
                {
                    tasks.Clear();
                    return FoundPassword;
                }

                tasks.Remove(completed);
            }

            //Пока не придумал как корректно обозначать то что пароль не найден, не подобран
            return string.Empty;
        }

        private string[] ReplaceWildcards(string[] source)
        {
            var temp = new List<string>();

            foreach (var st in source)
            {
                if (!st.Contains("*"))
                    temp.Add(st);
                else
                {
                    var position = st.IndexOf('*');
                    temp.AddRange(ReplaceWildcardFromPosition(st, position));
                }
            }

            return temp.ToArray();
        }

        private string[] ReplaceWildcardFromPosition(string source, int position)
        {
            var temp = new List<string>();

            foreach (var symbol in Alphabet)
            {
                StringBuilder sb = new StringBuilder(source.Substring(0, position));
                sb.Append(symbol);
                sb.Append(source.Substring(position + 1));
                var tempStr = sb.ToString();
                var newwcPosition = tempStr.IndexOf('*');
                temp.Add(tempStr.Replace("*", string.Empty));

                if (newwcPosition >= 0)
                    temp.AddRange(ReplaceWildcardFromPosition(tempStr, newwcPosition));
            }

            return temp.ToArray();
        }

        private void LoadAlphabet()
        {
            var alphabets = File.ReadAllLines(this.AlphabetPath);
            Alphabet = alphabets.First();
        }

        private Task OneTaskRoutine(int currentNumber, int pwdPerThread)
        {
            var passwordsLocal = new string[pwdPerThread];
            var elementsToCopyLenght = (Passwords.Length - currentNumber >= pwdPerThread) ? pwdPerThread : Passwords.Length - currentNumber;
            Array.Copy(Passwords, currentNumber, passwordsLocal, 0, elementsToCopyLenght);

            foreach (var password in passwordsLocal)
            {
                var dirName = $"{TargetPath}\\{currentNumber}";
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);

                var options = new RarArchiveLoadOptions()
                {
                    DecryptionPassword = password
                };
                try
                {
                    using (var archive = new RarArchive(SourceFile, options))
                    {
                        archive.ExtractToDirectory(dirName);
                    }
                    FoundPassword = password;
                    OnPasswordTry(password, string.Empty, true);
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    OnPasswordTry(password, ex.Message, false);
                }
            }

            throw new Exception();//Task.FromException(new Exception($"Password not found from {currentNumber} to {currentNumber + pwdPerThread}"));
        }

        private void OnPasswordTry(string password, string error, bool success)
        {
            var passwordTryLocal = PasswordTry;
            if (passwordTryLocal != null)
            {
                var args = new PasswordTryEventArgs(password, error, success);
                passwordTryLocal(this, args);
            }
        }
    }
}