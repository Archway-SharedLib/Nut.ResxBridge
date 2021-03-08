using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Nut.ResxBridge
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        private const string ModifierInternal = "internal";
        private const string ModifierPublic = "public";

        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // System.Diagnostics.Debugger.Launch();
            }
#endif

            var models = new List<ResxCodeModel>();
            foreach (var res in context.AdditionalFiles)
            {
                if (!Path.GetExtension(res.Path).Equals(".resx", StringComparison.InvariantCultureIgnoreCase)) continue;

                var opts = context.AnalyzerConfigOptions.GetOptions(res);
                if (!opts.TryGetValue("build_metadata.AdditionalFiles.ResxBridge_Generate", out var needGenerate)
                    || !needGenerate.Equals("true", StringComparison.InvariantCultureIgnoreCase)) continue;
                if (!opts.TryGetValue("build_metadata.AdditionalFiles.ManifestResourceName", out var manifestResourceName)
                    || string.IsNullOrEmpty(manifestResourceName)) continue;
                if (!TryGetTypeNameFrom(res.Path, out var typeName)) continue;

                opts.TryGetValue("build_metadata.AdditionalFiles.ResxBridge_Modifier", out var modifier);
                modifier = AdjustModiferString(modifier);
                var namespaceName = ToNamespace(manifestResourceName);

                var strings = LoadStrings(res.Path);

                models.Add(ResxCodeModel.Create(namespaceName, typeName, modifier, strings));
            }

            foreach(var model in models)
            {
                var template = new ResxClassTemplate() { Model = model };
                var source = template.TransformText();
                context.AddSource($"{model.NamespaceName}.{model.ClassName}.g", SourceText.From(source, Encoding.UTF8));
            }
        }

        private string ToNamespace(string manifestResourceName)
            => string.Join(".", SkipLastN(manifestResourceName.Split('.'), 1));

        private IEnumerable<T> SkipLastN<T>(IEnumerable<T> source, int n)
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


        private string AdjustModiferString(string? modifier)
            => modifier is null ? ModifierInternal :
                (modifier.Equals(ModifierPublic, StringComparison.InvariantCultureIgnoreCase) ? ModifierPublic : ModifierInternal);

        private bool TryGetTypeNameFrom(string fullFileName, out string typeName)
        {
            // 型名にできないファイル名はエラー
            var fileName = Path.GetFileNameWithoutExtension(fullFileName);
            typeName = fileName;
            return CSharpUtil.IsValidIdentifier(fileName);
        }

        private Dictionary<string, string> LoadStrings(string path)
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

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
