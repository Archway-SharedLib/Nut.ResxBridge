using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Nut.ResxBridge
{
    public class ResxCodeModel
    {
        private List<ResxMemberCodeModel> members = new List<ResxMemberCodeModel>();

        private static readonly string? version = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().Where(a => a.Key == "ApplicationVersion").Select(a => a.Value).FirstOrDefault();

        private ResxCodeModel(string namespaceName, string className, string modifier)
        {
            this.NamespaceName = namespaceName;
            this.ClassName = className;
            this.ClassModifer = modifier;
        }

        public string NamespaceName { get; }

        public string ClassName { get; }

        public string ClassModifer { get; }

        public IEnumerable<ResxMemberCodeModel> Members => members;

        public static ResxCodeModel Create(string namespaceName, string className, string modifier, Dictionary<string, string> resourceKeyValues)
        {
            var instance = new ResxCodeModel(namespaceName, className, modifier);
            foreach(var keyValue in resourceKeyValues)
            {
                if(ResxMemberCodeModel.TryCreate(className, keyValue.Key, keyValue.Value, out var model))
                {
                    instance.members.Add(model);
                }
            }
            return instance;
        }

        public string Version => version ?? string.Empty;
       
    }

    public class ResxMemberCodeModel
    {
        private ResxMemberCodeModel(string key, string value)
        {
            this.Key = key;
            this.Value = value;
            this.MemberName = CSharpUtil.ToValidIdentifier(key);
        }

        public string Key { get; }

        public string MemberName { get; }

        public string Value { get; }

        public bool IsMethod { get; private set; } = false;

        public int ArgumentCount { get; private set; } = 0;

        public static bool TryCreate(string className, string key, string value, out ResxMemberCodeModel model)
        {
            model = new ResxMemberCodeModel(key, value);
            if(className.Equals(model.MemberName, StringComparison.InvariantCulture))
            {
                return false;
            }

            var args = new List<string>();

            var regex = new Regex(@"{([0-9]|[1-9][0-9]*?)}");
            foreach (Match match in regex.Matches(value))
            {
                args.Add(match.Groups[1].Value);
            }
            if (!args.Any()) return true;

            var distinctArgs = args.Distinct().Select(v => int.Parse(v));
            var argsCount = distinctArgs.Count();
            if (distinctArgs.Max() != argsCount - 1)
            {
                return false;
            }
            model.ArgumentCount = argsCount;
            model.IsMethod = true;
            return true;
        }
    }
}
