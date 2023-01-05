using Aspose.Zip.Rar;
using System.Text;

var threadsCount = int.Parse(args[0]);
var tempDir = "D:\\temp";
var alphabet = LoadAlphabet();
var passwordsList = File.ReadAllLines("./passwords.txt");

Console.WriteLine("Preparing passwords list");
var passwords = ReplaceWildcards(passwordsList);
Console.WriteLine($"Passwords prepared: {passwords.Length}");
var passwordsPerThread = (int)passwords.Length / threadsCount;

var tasks = new List<Task>();
for (var i = 0; i < threadsCount; ++i)
{
    var threadTempI = i;
    tasks.Add(new Task(() => OneTaskRoutine(threadTempI * passwordsPerThread, passwordsPerThread)));
    Console.WriteLine($"Adding task {i}");
}

tasks.ForEach(x => x.Start());

while (tasks.Count > 0)
{
    var completed = await Task.WhenAny(tasks);

    if (completed.IsCompletedSuccessfully) // Successful?
    {
        tasks.Clear();
        break;
    }

    tasks.Remove(completed);
}

Console.WriteLine("Finished");

void OneTaskRoutine(int currentNumber, int pwdPerThread)
{
    var passwordsLocal = new string[pwdPerThread];
    var elementsToCopyLenght = (passwords.Length - currentNumber >= pwdPerThread) ? pwdPerThread : passwords.Length - currentNumber;
    Array.Copy(passwords, currentNumber, passwordsLocal, 0, elementsToCopyLenght);

    bool succeeded = false;

    foreach (var password in passwordsLocal)
    {
        var dirName = $"{tempDir}\\output\\{currentNumber}";
        if (!Directory.Exists(dirName))
            Directory.CreateDirectory(dirName);

        var options = new RarArchiveLoadOptions()
        {
            DecryptionPassword = password
        };
        try
        {
            Console.WriteLine($"Try password {password}");
            using (var archive = new RarArchive(tempDir + "\\input\\input.rar", options))
            {
                archive.ExtractToDirectory(dirName);
            }
            Console.WriteLine($"Password {password} succeeded");
            succeeded = true;
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Password {password} failed");
            Console.WriteLine(ex.Message);
        }
    }

    if (!succeeded)
        throw new Exception($"No passwords found from {currentNumber} to {currentNumber + pwdPerThread}");
}


string[] ReplaceWildcards(string[] source)
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

string[] ReplaceWildcardFromPosition(string source, int position)
{
    var temp = new List<string>();

    foreach (var symbol in alphabet)
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

string LoadAlphabet()
{
    var alphabets = File.ReadAllLines("alphabet.txt");
    return alphabets.First();
}


