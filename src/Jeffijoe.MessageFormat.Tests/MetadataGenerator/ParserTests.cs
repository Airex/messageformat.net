﻿using Jeffijoe.MessageFormat.MetadataGenerator.Plural.Parsing;

using System.Collections.Generic;
using System.Xml;

using Xunit;

namespace Jeffijoe.MessageFormat.Tests.MetadataGenerator
{
    public class ParserTests
    {
        [Fact]
        public void CanParseLocales()
        {
            var rules = ParseRules(@"
<supplementalData>
    <plurals type=""cardinal"">
        <pluralRules locales=""am as bn doi fa gu hi kn pcm zu"">
        </pluralRules>
    </plurals>
</supplementalData>
");

            var rule = Assert.Single(rules);
            var expected = new[]
            {
                "am", "as", "bn", "doi", "fa", "gu", "hi", "kn", "pcm", "zu"
            };
            var actual = rule.Locales;
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void OtherCountIsIgnored()
        {
            var rules = ParseRules(@"
<supplementalData>
    <plurals type=""cardinal"">
        <pluralRules locales=""am"">
            <pluralRule count=""other""> @integer 2~17, 100, 1000, 10000, 100000, 1000000, … @decimal 1.1~2.6, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, …</pluralRule>
        </pluralRules>
    </plurals>
</supplementalData>
");
            var rule = Assert.Single(rules);
            Assert.Empty(rule.Conditions);
        }

        [Fact]
        public void CanParseSingleCount_RuleDescription_WithoutRelations()
        {
            var rules = ParseRules(GenerateXmlWithRuleContent("@integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …"));

            var rule = Assert.Single(rules);
            var condition = Assert.Single(rule.Conditions);
            var expected = "@integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …";
            Assert.Equal(expected, condition.RuleDescription);
        }

        [Fact]
        public void CanParseSingleCount_VisibleDigitsNumber()
        {
            var rules = ParseRules(
                GenerateXmlWithRuleContent(@"v = 0 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …"));
            var rule = Assert.Single(rules);
            var condition = Assert.Single(rule.Conditions);
            var orCondition = Assert.Single(condition.OrConditions);
            var actual = Assert.Single(orCondition.AndConditions);
            var expected = new Operation(OperandSymbol.VisibleFractionDigitNumber, Relation.Equals, new[] { 0 });

            AssertOperationEqual(expected, actual);
        }

        [Fact]
        public void CanParseSingleCount_AbsoluteNumber()
        {
            var rules = ParseRules(
                GenerateXmlWithRuleContent("n = 1 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …"));
            var rule = Assert.Single(rules);
            var condition = Assert.Single(rule.Conditions);
            var orCondition = Assert.Single(condition.OrConditions);
            var actual = Assert.Single(orCondition.AndConditions);
            var expected = new Operation(OperandSymbol.AbsoluteValue, Relation.Equals, new[] { 1 });

            AssertOperationEqual(expected, actual);
        }

        [Theory]
        [InlineData("n = 2 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …", Relation.Equals)]
        [InlineData("n != 2 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …", Relation.NotEquals)]
        public void CanParseVariousRelations(string ruleText, Relation expectedRelation)
        {
            var rules = ParseRules(GenerateXmlWithRuleContent(ruleText));
            var rule = Assert.Single(rules);
            var condition = Assert.Single(rule.Conditions);
            var orCondition = Assert.Single(condition.OrConditions);
            var actual = Assert.Single(orCondition.AndConditions);
            var expected = new Operation(OperandSymbol.AbsoluteValue, expectedRelation, new[] { 2 });

            AssertOperationEqual(expected, actual);
        }

        [Fact]
        public void CanParseOrRules()
        {
            var rules = ParseRules(GenerateXmlWithRuleContent("n = 2 or n = 1 or n = 0 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …"));
            var rule = Assert.Single(rules);
            var condition = Assert.Single(rule.Conditions);
            
            Assert.Equal(3, condition.OrConditions.Length);

            var actualFirst = Assert.Single(condition.OrConditions[0].AndConditions);
            var expectedFirst = new Operation(OperandSymbol.AbsoluteValue, Relation.Equals, new[] { 2 });
            AssertOperationEqual(expectedFirst, actualFirst);

            var actualSecond = Assert.Single(condition.OrConditions[1].AndConditions);
            var expectedSecond = new Operation(OperandSymbol.AbsoluteValue, Relation.Equals, new[] { 1 });
            AssertOperationEqual(expectedSecond, actualSecond);

            var actualThird = Assert.Single(condition.OrConditions[2].AndConditions);
            var expectedThird = new Operation(OperandSymbol.AbsoluteValue, Relation.Equals, new[] { 0 });
            AssertOperationEqual(expectedThird, actualThird);
        }

        [Fact]
        public void CanParseAndRules()
        {
            var rules = ParseRules(GenerateXmlWithRuleContent("n = 2 and n = 1 and n = 0 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …"));
            var rule = Assert.Single(rules);
            var condition = Assert.Single(rule.Conditions);

            var orCondition = Assert.Single(condition.OrConditions);
            Assert.Equal(3, orCondition.AndConditions.Length);

            var actualFirst = orCondition.AndConditions[0];
            var expectedFirst = new Operation(OperandSymbol.AbsoluteValue, Relation.Equals, new[] { 2 });
            AssertOperationEqual(expectedFirst, actualFirst);

            var actualSecond = orCondition.AndConditions[1];
            var expectedSecond = new Operation(OperandSymbol.AbsoluteValue, Relation.Equals, new[] { 1 });
            AssertOperationEqual(expectedSecond, actualSecond);

            var actualThird = orCondition.AndConditions[2];
            var expectedThird = new Operation(OperandSymbol.AbsoluteValue, Relation.Equals, new[] { 0 });
            AssertOperationEqual(expectedThird, actualThird);
        }

        private static string GenerateXmlWithRuleContent(string ruleText)
        {
            return $@"
<supplementalData>
    <plurals type=""cardinal"">
        <pluralRules locales=""am"">
            <pluralRule count=""one"">{ruleText}</pluralRule>
        </pluralRules>
    </plurals>
</supplementalData>
";
        }

        private static void AssertOperationEqual(Operation expected, Operation actual)
        {
            Assert.Equal(expected.OperandLeft, actual.OperandLeft);
            Assert.Equal(expected.Relation, actual.Relation);
            Assert.Equal(expected.OperandRight, actual.OperandRight);
        }

        private static IEnumerable<PluralRule> ParseRules(string xmlText)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xmlText);

            var parser = new PluralParser(xml);

            return parser.Parse();
        }
    }
}
