namespace LMoses.Settings;

public sealed class SlidesOptions
{
    /// <summary>
    /// Directory (relative to AppContext.BaseDirectory) containing slide PDFs.
    /// By default we copy PDFs into output under ./Slides via the csproj Content items.
    /// </summary>
    public string DirectoryName { get; set; } = "Slides";

    /// <summary>
    /// How many PDFs to attach to a single Gemini request.
    /// </summary>
    public int MaxPdfsPerRequest { get; set; } = 2;
}


