﻿// MessageFormat for .NET
// - PatternParser_Parse_Tests.cs
// Author: Jeff Hansen <jeff@jeffijoe.com>
// Copyright (C) Jeff Hansen 2014. All rights reserved.

using System.Linq;
using System.Text;

using Jeffijoe.MessageFormat.Parsing;
using Jeffijoe.MessageFormat.Tests.TestHelpers;

using Moq;

using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions;

namespace Jeffijoe.MessageFormat.Tests.Parsing
{
    public class PatternParser_Parse_Tests
    {
        private readonly ITestOutputHelper outputHelper;

        public PatternParser_Parse_Tests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        [Theory]
        [InlineData("test, select, args", "test", "select", "args")]
        [InlineData("test, select, stuff {dawg}", "test", "select", "stuff {dawg}")]
        [InlineData("test, select, stuff \\{{dawg}\\}", "test", "select", "stuff \\{{dawg}\\}")]
        [InlineData("test, select, stuff {dawg, select, {name is \\{{name}\\}}}", "test", "select", 
            "stuff {dawg, select, {name is \\{{name}\\}}}")]
        public void Parse(string source, string expectedKey, string expectedFormat, string expectedArgs)
        {
            var literalParserMock = new Mock<ILiteralParser>();
            var sb = new StringBuilder(source);
            literalParserMock.Setup(x => x.ParseLiterals(sb));
            literalParserMock.Setup(x => x.ParseLiterals(sb))
                             .Returns(new[] { new Literal(0, source.Length, 1, 1, new StringBuilder(source)) });

            var subject = new PatternParser(literalParserMock.Object);

            // Warm up (JIT)
            Benchmark.Start("Parsing formatter patterns (first time before JIT)", this.outputHelper);
            subject.Parse(sb);
            Benchmark.End(this.outputHelper);
            Benchmark.Start("Parsing formatter patterns (after warm-up)", this.outputHelper);
            var actual = subject.Parse(sb);
            Benchmark.End(this.outputHelper);
            Assert.Equal(1, actual.Count());
            var first = actual.First();
            Assert.Equal(expectedKey, first.Variable);
            Assert.Equal(expectedFormat, first.FormatterName);
            Assert.Equal(expectedArgs, first.FormatterArguments);
        }

        [Fact]
        public void Parse_exits_early_when_no_literals_have_been_found()
        {
            var literalParserMock = new Mock<ILiteralParser>();
            var subject = new PatternParser(literalParserMock.Object);
            literalParserMock.Setup(x => x.ParseLiterals(It.IsAny<StringBuilder>())).Returns(new Literal[0]);
            Assert.Empty(subject.Parse(new StringBuilder()));
        }
    }
}