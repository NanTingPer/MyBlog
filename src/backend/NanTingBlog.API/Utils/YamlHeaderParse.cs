using System.Text;
using System.Text.Json;

namespace NanTingBlog.API.Utils;

/// <summary>
/// 解析markdown文档的yaml头
/// </summary>
public class YamlHeaderParse
{
    /// <summary>
    /// 是否有头
    /// </summary>
    public bool IsHeader { get; private set; }

    /// <summary>
    /// markdown文本（回写后会更新）
    /// </summary>
    public string MarkdownText { get; private set; }

    /// <summary>
    /// 解析后的yaml键值对
    /// </summary>
    private readonly Dictionary<string, YamlHeaderValue> _properties = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 构造函数，解析markdown中的yaml头
    /// </summary>
    /// <param name="markdownText">markdown完整内容</param>
    public YamlHeaderParse(string markdownText)
    {
        MarkdownText = markdownText;
        IsHeader = Parse(markdownText);
    }

    /// <summary>
    /// 获取指定键的值对象
    /// </summary>
    /// <param name="key">属性名称</param>
    /// <returns>YamlHeaderValue，不存在则返回null</returns>
    public YamlHeaderValue? GetValue(string key)
    {
        return _properties.GetValueOrDefault(key);
    }

    /// <summary>
    /// 获取字符串值
    /// </summary>
    /// <param name="key">属性名称</param>
    /// <returns>属性值，不存在则返回null</returns>
    public string? GetValueToString(string key)
    {
        return _properties.GetValueOrDefault(key)?.ToStringValue();
    }

    /// <summary>
    /// 获取字符串数组值
    /// </summary>
    /// <param name="key">属性名称</param>
    /// <returns>属性值数组，不存在则返回null</returns>
    public string[]? GetValueToArray(string key)
    {
        return _properties.GetValueOrDefault(key)?.ToArrayValue();
    }

    /// <summary>
    /// 获取日期时间值
    /// </summary>
    /// <param name="key">属性名称</param>
    /// <returns>属性值，不存在或解析失败则返回null</returns>
    public DateTimeOffset? GetValueToDateTime(string key)
    {
        return _properties.GetValueOrDefault(key)?.ToDateTimeValue();
    }

    /// <summary>
    /// 获取布尔值
    /// </summary>
    /// <param name="key">属性名称</param>
    /// <returns>布尔值，不存在或解析失败则返回null</returns>
    public bool? GetValueToBool(string key)
    {
        return _properties.GetValueOrDefault(key)?.ToBoolValue();
    }

    /// <summary>
    /// 添加或覆盖一个字符串类型的头属性
    /// </summary>
    /// <param name="key">属性名称</param>
    /// <param name="value">字符串值</param>
    public void AddHeader(string key, string value)
    {
        _properties[key] = new YamlHeaderValue(value);
    }

    /// <summary>
    /// 添加或覆盖一个字符串数组类型的头属性，序列化为 ["val1", "val2"] 格式
    /// </summary>
    /// <param name="key">属性名称</param>
    /// <param name="value">字符串数组值</param>
    public void AddHeader(string key, string[] value)
    {
        var serialized = JsonSerializer.Serialize(value);
        _properties[key] = new YamlHeaderValue(serialized);
    }

    /// <summary>
    /// 添加或覆盖一个日期时间类型的头属性，序列化为 ISO 8601 格式
    /// </summary>
    /// <param name="key">属性名称</param>
    /// <param name="value">日期时间值</param>
    public void AddHeader(string key, DateTimeOffset value)
    {
        _properties[key] = new YamlHeaderValue(value.ToString("o"));
    }

    /// <summary>
    /// 添加或覆盖一个布尔类型的头属性
    /// </summary>
    /// <param name="key">属性名称</param>
    /// <param name="value">布尔值</param>
    public void AddHeader(string key, bool value)
    {
        _properties[key] = new YamlHeaderValue(value.ToString());
    }

    /// <summary>
    /// 将当前头属性回写到MarkdownText中
    /// </summary>
    /// <returns>回写后的完整markdown文本</returns>
    public string WriteToMarkdown()
    {
        var yamlBuilder = new StringBuilder();
        yamlBuilder.AppendLine("---");

        foreach (var kvp in _properties) {
            yamlBuilder.AppendLine($"{kvp.Key}: {kvp.Value.Value}");
        }

        yamlBuilder.AppendLine("---");

        if (IsHeader) {
            var text = MarkdownText.TrimStart();
            var firstSep = text.IndexOf("---", 3);
            if (firstSep > 0) {
                MarkdownText = yamlBuilder.ToString() + text[(firstSep + 3)..].TrimStart('\r', '\n');
            }
        } else {
            MarkdownText = yamlBuilder.ToString() + MarkdownText;
            IsHeader = true;
        }

        return MarkdownText;
    }

    /// <summary>
    /// 解析yaml头内容
    /// </summary>
    /// <param name="markdownText">markdown完整内容</param>
    private bool Parse(string markdownText)
    {
        var text = markdownText.TrimStart();
        var parts = text.Split("---");
        if (parts.Length < 2) {
            return false;
        }

        var yamlContent = parts[1];
        var lines = yamlContent.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines) {
            var segments = line.Trim().Split(':', 2);
            if (segments.Length < 2) {
                continue;
            }

            var key = segments[0].Trim();
            var value = segments[1].Trim();

            // 去除首尾引号
            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) ||
                 (value.StartsWith('\'') && value.EndsWith('\'')))) {
                value = value[1..^1];
            }

            _properties[key] = new YamlHeaderValue(value);
        }
        return true;
    }

    /// <summary>
    /// 删除一个头
    /// </summary>
    /// <param name="key"></param>
    public void DeleteHeader(string key)
    {
        _properties.Remove(key);
    }
}

/// <summary>
/// yaml头中一个键值对的值，支持字符串、数组、日期时间的解析
/// </summary>
public class YamlHeaderValue
{
    /// <summary>
    /// 使用原始值创建一个YamlHeader值对象，自动检测类型
    /// </summary>
    /// <param name="value">原始值字符串</param>
    public YamlHeaderValue(string value)
    {
        Value = value;
        IsArray = DetectIsArray(value);
        IsDateTime = DetectIsDateTime(value);
        IsBool = DetectIsBool(value);
    }

    /// <summary>
    /// 是否是数组类型
    /// </summary>
    public bool IsArray { get; private set; }

    /// <summary>
    /// 是否是时间类型
    /// </summary>
    public bool IsDateTime { get; private set; }

    /// <summary>
    /// 是否是布尔类型
    /// </summary>
    public bool IsBool { get; private set; }

    /// <summary>
    /// 原始值
    /// </summary>
    public string? Value { get; private set; }

    /// <summary>
    /// 返回字符串值
    /// </summary>
    /// <returns>字符串值，原始值为null时返回null</returns>
    public string? ToStringValue()
    {
        return Value;
    }

    /// <summary>
    /// 解析并返回字符串数组值
    /// </summary>
    /// <returns>字符串数组，解析失败则返回null</returns>
    public string[]? ToArrayValue()
    {
        if (Value is null) {
            return null;
        }

        if (Value.StartsWith('[') && Value.EndsWith(']')) {
            try {
                return JsonSerializer.Deserialize<string[]>(Value);
            } catch {
            }
        }

        return [.. Value.Split(',')
            .Select(s => s.Trim().Trim('"', '\''))
            .Where(s => !string.IsNullOrEmpty(s))];
    }

    /// <summary>
    /// 解析并返回日期时间值
    /// </summary>
    /// <returns>DateTimeOffset，解析失败则返回null</returns>
    public DateTimeOffset? ToDateTimeValue()
    {
        if (Value is null) {
            return null;
        }

        if (DateTimeOffset.TryParse(Value, out var result)) {
            return result;
        }

        return null;
    }

    /// <summary>
    /// 检测原始值是否为数组格式（仅判断 JSON 数组 [xxx]）
    /// </summary>
    private static bool DetectIsArray(string value)
    {
        return value.StartsWith('[') && value.EndsWith(']');
    }

    /// <summary>
    /// 返回布尔值
    /// </summary>
    /// <returns>布尔值，解析失败则返回null</returns>
    public bool? ToBoolValue()
    {
        if (Value is null) {
            return null;
        }

        if (bool.TryParse(Value, out var result)) {
            return result;
        }

        return null;
    }

    /// <summary>
    /// 检测原始值是否为日期时间格式
    /// </summary>
    private static bool DetectIsDateTime(string value)
    {
        return DateTimeOffset.TryParse(value, out _);
    }

    /// <summary>
    /// 检测原始值是否为布尔格式（true/false，不区分大小写）
    /// </summary>
    private static bool DetectIsBool(string value)
    {
        return bool.TryParse(value, out _);
    }
}