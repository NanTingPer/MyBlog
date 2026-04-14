namespace NanTingBlog.API;

public class HttpClientType
{
    public string Name { get; set; }
    private HttpClientType(string name) => Name = name;

    public static implicit operator string(HttpClientType type) => type.Name;

    public readonly static HttpClientType Meilisearch = new HttpClientType("meilisearch");
}
