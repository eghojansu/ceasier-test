using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ceasier.Utils;

namespace CeasierTests.Utils
{
    [TestClass]
    public class RelativeDateParserTest
    {
        [TestMethod]
        public void SummarizedTest()
        {
            Assert.AreEqual(DateTime.Today.ToString("yyyy-MM-dd"), RelativeDateParser.Parse("now").ToString("yyyy-MM-dd"));
            Assert.AreEqual(DateTime.Today.ToString("yyyy-MM-dd"), RelativeDateParser.Parse("today").ToString("yyyy-MM-dd"));

            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), RelativeDateParser.Parse("yesterday").ToString("yyyy-MM-dd"));
            Assert.AreEqual(DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), RelativeDateParser.Parse("tomorrow").ToString("yyyy-MM-dd"));

            Assert.AreEqual(DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd"), RelativeDateParser.Parse("-3 day").ToString("yyyy-MM-dd"));
            Assert.AreEqual(DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), RelativeDateParser.Parse("+3 day").ToString("yyyy-MM-dd"));

            Assert.AreEqual(DateTime.Now.AddHours(1).ToString("HH"), RelativeDateParser.Parse("+1 hour").ToString("HH"));
            Assert.AreEqual(DateTime.Now.AddMinutes(-3).ToString("mm"), RelativeDateParser.Parse("-3 minute").ToString("mm"));
            Assert.AreEqual(DateTime.Now.AddSeconds(-10).ToString("ss"), RelativeDateParser.Parse("10 second ago").ToString("ss"));

            Assert.AreEqual(DateTime.Today.AddDays(7).ToString("dd"), RelativeDateParser.Parse("next week").ToString("dd"));
            Assert.AreEqual(DateTime.Today.AddMonths(1).ToString("MM"), RelativeDateParser.Parse("next month").ToString("MM"));
            Assert.AreEqual(DateTime.Today.AddYears(-1).ToString("yyyy"), RelativeDateParser.Parse("last year").ToString("yyyy"));
            Assert.AreEqual(DateTime.Today.AddDays(-1).AddHours(2).AddMinutes(10).ToString("yyyyMMdd"), RelativeDateParser.Parse("1 day 2 hours 10 minutes ago").ToString("yyyyMMdd"));
            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyyMMdd"), RelativeDateParser.Parse("1 day ago").ToString("yyyyMMdd"));

            Assert.AreEqual(new DateTime(2023, 11, 18).ToString("yyyy-MM-dd"), RelativeDateParser.Parse("2023-11-18").ToString("yyyy-MM-dd"));
            Assert.AreEqual(new DateTime(2023, 11, 18).ToString("yyyy-MM-dd"), RelativeDateParser.Parse("11/18/2023").ToString("yyyy-MM-dd"));

            Assert.ThrowsException<FormatException>(() => RelativeDateParser.Parse("10 ago"));
            Assert.ThrowsException<FormatException>(() => RelativeDateParser.Parse("not a date"));
            Assert.ThrowsException<Exception>(() => RelativeDateParser.Parse("10 invalid"));
        }
    }
}
