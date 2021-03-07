using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nut.ResxBridge
{
    public partial class TypedResourceClassTemplate
    {
        public ResxCodeModel? Model { get; set; }

        public string EscStr(string text) => text.Replace("\"", "\"\"");

        public string ArgsStr(ResxMemberCodeModel member, string typeName) =>
            string.Join(", ", Enumerable.Range(0, member.ArgumentCount).Select(v => $"{typeName} _{v}"));

        public string ArgsStr(ResxMemberCodeModel member) =>
            string.Join(", ", Enumerable.Range(0, member.ArgumentCount).Select(v => $"_{v}"));

    }
}
