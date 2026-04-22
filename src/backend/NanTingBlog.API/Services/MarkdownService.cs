using Markdig;

namespace NanTingBlog.API.Services;

/// <summary>
/// ToHTML
/// </summary>
public class MarkdownService(MarkdownPipeline pipe)
{
    private readonly MarkdownPipeline pipe = pipe;
    /// <summary>
    /// ToHTML
    /// </summary>
    public string ToHTML(string mardownText)
    {
        return Markdown.ToHtml(mardownText, pipe);
    }
}
