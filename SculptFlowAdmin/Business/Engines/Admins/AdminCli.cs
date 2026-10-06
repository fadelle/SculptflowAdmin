using SculptFlowAdmin.Business.Contracts.Services.Auth;
using SculptFlowAdmin.Business.Services.Auth;

namespace SculptFlowAdmin.Business.Engines.Admins;

/// <summary>Command-line helpers run through `dotnet run -- &lt;command&gt;` instead of starting the web server.</summary>
public static class AdminCli
{
    public static async Task CreateAdminAsync(IServiceProvider services, string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: dotnet run -- create-admin <email> \"<full name>\"   (password from ADMIN_PASSWORD, or typed in)");
            Environment.ExitCode = 1;
            return;
        }

        var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        if (string.IsNullOrEmpty(password))
        {
            Console.Write($"Password for {args[1]} (min {AdminAuthService.MinPasswordLength} chars): ");
            password = ReadHidden();
        }

        using var scope = services.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAdminAuthService>();
        try
        {
            var user = await auth.CreateAsync(args[1], args[2], password);
            Console.WriteLine($"Admin created: {user.Email} ({user.Id}).");
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            Environment.ExitCode = 1;
        }
    }

    private static string ReadHidden()
    {
        if (Console.IsInputRedirected) return Console.ReadLine() ?? string.Empty;
        var chars = new List<char>();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace) { if (chars.Count > 0) chars.RemoveAt(chars.Count - 1); continue; }
            chars.Add(key.KeyChar);
        }
        Console.WriteLine();
        return new string(chars.ToArray());
    }
}
