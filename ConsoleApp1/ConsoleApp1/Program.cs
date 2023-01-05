using RarBrute.PasswordFinder.BL;
using System.Diagnostics;

Stopwatch stopwatch = Stopwatch.StartNew();

int threadsCount;

if (!int.TryParse(args[0], out threadsCount))
    threadsCount = 100;

var finder = new PasswordFinder(
    "D:\\temp\\input\\input.rar",
    "D:\\temp\\output\\",
    "alphabet.txt",
    "passwords.txt",
    threadsCount);

Console.WriteLine($"Searching password with {threadsCount} threads");

finder.PasswordTry += OnPasswordTry;

var foundPassword = $"Found password {await finder.FindPassword()}";

if (string.IsNullOrEmpty(foundPassword))
{
    Console.WriteLine("No passwords found");
}
else
{
    Console.WriteLine($"Found password {foundPassword}");
}

finder.PasswordTry -= OnPasswordTry;

stopwatch.Stop();
Console.WriteLine($"Spent {stopwatch.Elapsed}");

void OnPasswordTry(object o, PasswordTryEventArgs e)
{
    var successString = e.Success ? "SUCCESS" : "FAILURE";

    if (e.Success)
        Console.WriteLine($"Password try {e.UsedPassword} with result {successString}");
    else
        Console.WriteLine($"Password try {e.UsedPassword} with result {successString} and error {e.Error}");
}





