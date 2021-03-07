using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Nut.ResxBridge.Test
{
    public class CSharpUtilTest
    {
        private string[] invalidIdentifiers = new[] { "ABC-DEF", "123ABC", "public", "ABC!DEF", "A.BC DEF", "１２３４ABCＡＢⅭＤ" };

        [Fact]
        public void IsValidIdentifier_識別子としては正しくないものはfalse()
        {
            foreach(var identifier in invalidIdentifiers)
            {
                CSharpUtil.IsValidIdentifier(identifier).Should().BeFalse(identifier);
            }
        }

        [Fact]
        public void ToValidIdentifier_識別子として正しいものに変換する()
        {
            foreach (var identifier in invalidIdentifiers)
            {
                var validIdentifier = CSharpUtil.ToValidIdentifier(identifier);
                SyntaxFacts.IsValidIdentifier(validIdentifier).Should().BeTrue(validIdentifier);
            }
        }
    }
}
