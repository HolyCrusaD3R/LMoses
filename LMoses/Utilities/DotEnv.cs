namespace LMoses.Utilities;

/// <summary>
/// Minimal .env loader (KEY=VALUE lines) for local development.
/// Loads the first .env found by walking up from the current directory.
/// </summary>
public static class DotEnv
{
    public static void Load(string fileName = ".env", int maxParentDepth = 8, bool overwriteExistingEnvVars = false)
    {
        var envPath = FindUpwards(Directory.GetCurrentDirectory(), fileName, maxParentDepth);
        if (envPath is null || !File.Exists(envPath))
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(envPath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith('#')) continue;

            var idx = line.IndexOf('=');
            if (idx <= 0) continue;

            var key = line[..idx].Trim();
            var value = line[(idx + 1)..].Trim();

            // Strip simple quotes
            if ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\'')))
            {
                value = value[1..^1];
            }

            if (key.Length == 0) continue;

            var existing = Environment.GetEnvironmentVariable(key);
            if (!overwriteExistingEnvVars && !string.IsNullOrEmpty(existing))
            {
                continue;
            }

            Environment.SetEnvironmentVariable(key, value);
        }

        // Connect common env var name to ASP.NET options binding:
        // GEMINI_API_KEY -> Gemini:ApiKey (environment-variable form: Gemini__ApiKey)
        var geminiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        var geminiOptKey = Environment.GetEnvironmentVariable("Gemini__ApiKey");
        if (!string.IsNullOrWhiteSpace(geminiKey) && string.IsNullOrWhiteSpace(geminiOptKey))
        {
            Environment.SetEnvironmentVariable("Gemini__ApiKey", geminiKey);
        }
    }

    private static string? FindUpwards(string startDir, string fileName, int maxDepth)
    {
        var dir = new DirectoryInfo(startDir);
        for (var i = 0; i <= maxDepth && dir is not null; i++)
        {
            var candidate = Path.Combine(dir.FullName, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        return null;
    }
}



