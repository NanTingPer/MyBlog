//// See https://aka.ms/new-console-template for more information
//using Meilisearch;
//using NanTingBlog.API.Dtos.Blogs;

//Console.WriteLine("Hello, World!");
////await Test.AddDocumentAsync();
//await Test.UpdateSettingAsync();
//await Test.SearchAsync();

//public static class Test
//{
//    public static async Task AddDocumentAsync()
//    {
//        // client的httpclient无法被释放 。。。。 尽量使用httpclient工厂
//        var client = new MeilisearchClient("http://localhost:7700/");
//        var index = client.Index(nameof(PostInfo).ToLower());
        
//        for(int i = 0; i < 10; i++) {
//            // index文档https://www.meilisearch.com/docs/resources/internals/primary_key
//            var testLog = new PostInfo() 
//            {
//                Author = "my",
//                CreateTime = DateTime.Now.Ticks,
//                Content = $"testlog{i}",
//                EditTime = DateTime.Now.Ticks,
//                Name = "哈哈"
//            };
//            var taskInfo = await index.AddDocumentsAsync([testLog]);
//            _ = 1;
//        }
//    }

//    public static async Task SearchAsync()
//    {
//        var client = new MeilisearchClient("http://localhost:7700/");
//        var index = client.Index(nameof(PostInfo).ToLower());
//        var sq = new SearchQuery()
//        {
//            AttributesToSearchOn = [nameof(PostInfo.Name).ToLower()]
//        };
//        var querys = await index.SearchAsync<PostInfo>("testlog1", sq);
//        foreach (var item in querys.Hits) {
            
//        }
//    }

//    public static async Task UpdateSettingAsync()
//    {
//        var client = new MeilisearchClient("http://localhost:7700/");
//        var index = client.Index(nameof(PostInfo).ToLower());
//        await index.UpdateSettingsAsync(new Settings()
//        {
//            SearchableAttributes = ["name", "content"],
//            FilterableAttributes = ["name", "content"],
//        });
//    }

//}

///// <summary>
///// 若要使属性能被索引，那么需要使用<see cref="Meilisearch.Index.UpdateSettingsAsync(Settings, CancellationToken)"/>并指定<see cref="Settings.SearchableAttributes"/>   <br/>
///// 若要指定搜索属性，那么需要使用<see cref="SearchQueryBase.AttributesToSearchOn"/>                                                                                    <br/>
///// </summary>
//public class Note{}