using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Nut.ResxBridge.Test
{
    public class ResxMemberCodeModelTest
    {
        [Fact]
        public void TryCreate_通常の文字列はIsMethodがfalseで返される()
        {
            ResxMemberCodeModel.TryCreate("test", "sample", "サンプルです。", out var result).Should().BeTrue();
            result.IsMethod.Should().BeFalse();
        }

        [Fact]
        public void TryCreate_クラス名と同じキーでは作成できない()
        {
            ResxMemberCodeModel.TryCreate("test", "test", "サンプルです。", out var _).Should().BeFalse();
        }

        [Fact]
        public void TryCreate_キーが識別子にふさわしい場合はMemberNameはそのまま利用される()
        {
            ResxMemberCodeModel.TryCreate("test", "sample_key_desu", "サンプルです。", out var result).Should().BeTrue();
            result.MemberName.Should().Be("sample_key_desu");
        }

        [Fact]
        public void TryCreate_キーが識別子にふさわしくない場合はMemberNameは変換される()
        {
            ResxMemberCodeModel.TryCreate("test", "sample-key.desu", "サンプルです。", out var result).Should().BeTrue();
            result.MemberName.Should().Be("sample_key_desu");
        }

        [Fact]
        public void TryCreate_値の中に置換プレイスホルダーがある場合はメソッド扱いでプレイスホルダーの数が設定される()
        {
            ResxMemberCodeModel.TryCreate("test", "sample", "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", out var result).Should().BeTrue();
            result.IsMethod.Should().BeTrue();
            result.ArgumentCount.Should().Be(12);
        }

        [Fact]
        public void TryCreate_置換プレイスホルダーの中の数値が不正な場合は作成できない()
        {
            // 04 が NG
            ResxMemberCodeModel.TryCreate("test", "sample", "{0}{1}{2}{3}{04}{5}{6}{7}{8}{9}{10}{11}", out var _).Should().BeFalse();
        }

        [Fact]
        public void TryCreate_置換プレイスホルダーが0から連続していない場合は作成できない()
        {
            // 3,4,5 が ない
            ResxMemberCodeModel.TryCreate("test", "sample", "{0}{1}{2}{6}{7}{8}{9}{10}{11}", out var _).Should().BeFalse();
        }

        [Fact]
        public void TryCreate_置換プレイスホルダーの数値が重複している場合はサマられてカウントされる()
        {
            ResxMemberCodeModel.TryCreate("test", "sample", "{0}{1}{6}{2}{3}{3}{4}{5}{5}{5}", out var result).Should().BeTrue();
            result.ArgumentCount.Should().Be(7);
        }

    }
}
