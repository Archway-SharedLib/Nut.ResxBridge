# Nut.ResxBridge

Resxファイルから型付けされたクラスを生成します。

Resx source:

![リソースファイル](./docs/images/resx_file.png)

Generated source:

```cs
internal static class Strings
{
    private static ResourceManager resourceManager = new ResourceManager(typeof(ConsoleApp1.Resources.Strings));

    private static string GetResourceString(string resourceKey, string defaultString = null)
    {
        string resourceString = null;
        try
        {
            resourceString = resourceManager.GetString(resourceKey);
        }
        catch (MissingManifestResourceException) { }

        if (defaultString != null && resourceKey.Equals(resourceString))
        {
            return defaultString;
        }

        return resourceString;
    }

    private static string Format(string resourceFormat, params object[] args)
    {
        if (args != null)
        {
            return string.Format(resourceFormat, args);
        }

        return resourceFormat;
    }

    public static string Key_3 => GetResourceString(@"Key 3", @"Key 3");
    public static string Key_4 => GetResourceString(@"Key""4", @"Key 4");
    public static string Key_2 => GetResourceString(@"Key.2", @"Key 2");
    public static string Key1 => GetResourceString(@"Key1", @"Key 1");
    public static string Method1(object _0, object _1, object _2)
            => Format(GetResourceString(@"Method1", @"{0}{1}""{2} Method"), 
                _0, _1, _2);
    public static string Val1 => GetResourceString(@"Val1", @"Value""1");

}
```
