using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace Nut.ResxBridge
{
    [Generator]
    public class SourceGenerator : IIncrementalGenerator
    {
        private record AdditionalTextModel(string Path, SourceText? Text, string Modifier, string NamespaceName, string TypeName);

        private const string ModifierInternal = "internal";
        private const string ModifierPublic = "public";


        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var targets = context.AdditionalTextsProvider
                .Combine(context.AnalyzerConfigOptionsProvider)
                .Where(Filter)
                .Select(Map)
                .Where(m => m is not null);

            context.RegisterSourceOutput(targets, GenerateSource!);

                //.Where(static af => af.Path.EndsWith(".resx", StringComparison.InvariantCultureIgnoreCase));

        }

        private static bool Filter((AdditionalText Left, AnalyzerConfigOptionsProvider Right) v)
        {
            var additionalText = v.Left;
            var optionsProvider = v.Right;

            if (!additionalText.Path.EndsWith(".resx", StringComparison.InvariantCultureIgnoreCase)) return false;

            var atOption = optionsProvider.GetOptions(additionalText);
            //if (!atOption.TryGetValue("build_metadata.AdditionalFiles.SourceItemType", out var itemType) 
            //    || itemType != "EmbeddedResource") return false;

            if (!atOption.TryGetValue("build_metadata.AdditionalFiles.ResxBridge_Generate", out var generate) 
                || !generate.Equals("true", StringComparison.InvariantCultureIgnoreCase)) return false;

            return true;
        }

        private static AdditionalTextModel? Map((AdditionalText Left, AnalyzerConfigOptionsProvider Right) v, CancellationToken cancellationToken)
        {
            var additionalText = v.Left;
            var optionsProvider = v.Right;

            var atOption = optionsProvider.GetOptions(additionalText);
            atOption.TryGetValue("build_metadata.AdditionalFiles.ResxBridge_Modifier", out var modifier);
            modifier = AdjustModiferString(modifier);

            if (!atOption.TryGetValue("build_metadata.AdditionalFiles.ManifestResourceName", out var manifestResourceName) 
                || string.IsNullOrEmpty(manifestResourceName)) return null;

            var sourceText = additionalText.GetText();
            if (sourceText is null) return null;

            if (!TryGetTypeNameFrom(additionalText.Path, out var typeName)) return null;

            var namespaceName = ToNamespace(manifestResourceName);

            return new AdditionalTextModel(additionalText.Path, sourceText, modifier, namespaceName, typeName);
        }

        private static bool TryGetTypeNameFrom(string fullFileName, out string typeName)
        {
            // 型名にできないファイル名はエラー
            var fileName = Path.GetFileNameWithoutExtension(fullFileName);
            typeName = fileName;
            return CSharpUtil.IsValidIdentifier(fileName);
        }

        private static string AdjustModiferString(string? modifier)
            => modifier is null ? ModifierInternal :
                (modifier.Equals(ModifierPublic, StringComparison.InvariantCultureIgnoreCase) ? ModifierPublic : ModifierInternal);

        private static string ToNamespace(string manifestResourceName)
            => string.Join(".", SkipLastN(manifestResourceName.Split('.'), 1));

        private static IEnumerable<T> SkipLastN<T>(IEnumerable<T> source, int n)
        {
            var it = source.GetEnumerator();
            var hasRemainingItems = false;
            var cache = new Queue<T>(n + 1);

            do
            {
                if (hasRemainingItems = it.MoveNext())
                {
                    cache.Enqueue(it.Current);
                    if (cache.Count > n)
                        yield return cache.Dequeue();
                }
            } while (hasRemainingItems);
        }

        private static Dictionary<string, string> LoadStrings(string path)
        {
            return XDocument.Load(path)
                .Root.Elements("data")
                .Where(elem =>
                {
                    if (elem.Attribute("type") is not null) return false;
                    if (elem.Attribute("name") is null) return false;
                    var spaceAttr = elem.Attribute(XNamespace.Xml + "space");
                    if (spaceAttr is not null) return spaceAttr.Value.Equals("preserve");
                    return false;
                }).Select(elem =>
                {
                    return (DataElem: elem, ValueElem: elem.Element("value"));
                }).Where(elem => {
                    if (elem.ValueElem is null) return false;
                    if (elem.ValueElem.Nodes().Count() > 1) return false;
                    if (elem.ValueElem.FirstNode?.NodeType != System.Xml.XmlNodeType.Text) return false;
                    return true;
                })
                .ToDictionary(
                    elem => elem.DataElem.Attribute("name").Value,
                    elem => elem.ValueElem.Value
                );
        }

        static void GenerateSource(SourceProductionContext spc, AdditionalTextModel sourceModel)
        {
            var strings = LoadStrings(sourceModel.Path);

            var model = ResxCodeModel.Create(sourceModel.NamespaceName, sourceModel.TypeName, sourceModel.Modifier, strings);

            var template = new ResxClassTemplate() { Model = model };
            var source = template.TransformText();
            spc.AddSource($"{model.NamespaceName}.{model.ClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    } 
}
