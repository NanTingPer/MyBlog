using NanTingBlog.API.Utils;
var mdPath = Path.Combine(AppContext.BaseDirectory, "饰品.md");
var yamlHeaderParse = new YamlHeaderParse(File.ReadAllText(mdPath));
yamlHeaderParse.AddHeader("测试头", false);
File.WriteAllText(mdPath, yamlHeaderParse.WriteToMarkdown());
var str = File.ReadAllText(mdPath);
_ = 1;