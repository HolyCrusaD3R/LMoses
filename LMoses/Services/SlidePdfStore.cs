using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using LMoses.Settings;

namespace LMoses.Services;

public sealed class SlidePdfStore
{
    private static readonly Regex NonAlphaNum = new(@"[^a-z0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly SlidesOptions _options;

    public SlidePdfStore(IOptions<SlidesOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyList<SlidePdf> GetBestPdfsForQuestion(string question)
    {
        var all = GetAllPdfs();
        if (all.Count == 0)
        {
            return Array.Empty<SlidePdf>();
        }

        var q = Normalize(question);
        var scored = all
            .Select(p => new { Pdf = p, Score = Score(q, p.FileName) })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Pdf.FileName)
            .ToList();

        // If everything scores 0, just take the first N deterministically.
        var take = Math.Max(1, _options.MaxPdfsPerRequest);
        var best = scored.Where(x => x.Score > 0).Select(x => x.Pdf).Take(take).ToList();
        if (best.Count > 0)
        {
            return best;
        }

        return scored.Select(x => x.Pdf).Take(take).ToList();
    }

    public IReadOnlyList<SlidePdf> GetAllPdfs()
    {
        var slidesDir = Path.Combine(AppContext.BaseDirectory, _options.DirectoryName);
        if (!Directory.Exists(slidesDir))
        {
            return Array.Empty<SlidePdf>();
        }

        return Directory.EnumerateFiles(slidesDir, "*.pdf", SearchOption.TopDirectoryOnly)
            .Select(p => new SlidePdf(p))
            .OrderBy(p => p.FileName)
            .ToList();
    }

    private static int Score(string normalizedQuestion, string fileName)
    {
        // Token overlap between question and filename.
        var normalizedFile = Normalize(fileName);
        var score = 0;

        foreach (var token in normalizedQuestion.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (token.Length < 3) continue;
            if (normalizedFile.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 2;
            }
        }

        // Small manual boosts for common course terms → PDFs in this repo.
        if (normalizedQuestion.Contains("k means", StringComparison.OrdinalIgnoreCase) ||
            normalizedQuestion.Contains("kmean", StringComparison.OrdinalIgnoreCase) ||
            normalizedQuestion.Contains("clustering", StringComparison.OrdinalIgnoreCase))
        {
            if (normalizedFile.Contains("clustering", StringComparison.OrdinalIgnoreCase))
            {
                score += 10;
            }
        }

        if (normalizedQuestion.Contains("norm", StringComparison.OrdinalIgnoreCase) ||
            normalizedQuestion.Contains("l1", StringComparison.OrdinalIgnoreCase) ||
            normalizedQuestion.Contains("l2", StringComparison.OrdinalIgnoreCase))
        {
            if (normalizedFile.Contains("norm", StringComparison.OrdinalIgnoreCase))
            {
                score += 10;
            }
        }

        return score;
    }

    private static string Normalize(string s)
    {
        s = s.ToLowerInvariant();
        s = NonAlphaNum.Replace(s, " ");
        return s.Trim();
    }
}

public sealed record SlidePdf(string FullPath)
{
    public string FileName => Path.GetFileName(FullPath);
}


