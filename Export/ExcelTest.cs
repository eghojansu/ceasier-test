using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Ceasier.Export;
using System;

namespace CeasierTests.Export
{
    [TestClass]
    public class ExcelTest
    {
        [TestMethod]
        public void SummarizedTest()
        {
            var data = new List<object>()
            {
                new { name = "foo", date = new DateTime(2023, 11, 28) },
                new { name = "bar", date = new DateTime(2023, 11, 29) },
            };
            var excel = new Excel(data).AddColumn("name").AddDate("date");
            var result = excel.GetResult();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UseShortcut()
        {
            var data = new List<object>()
            {
                new { name = "foo", date = new DateTime(2023, 11, 28) },
                new { name = "bar", date = new DateTime(2023, 11, 29) },
            };
            var columns = new ExcelColumn[] {
                ExcelColumn.Create("name"),
                ExcelColumn.Date("date"),
            };
            var result = Excel.Download("foo", columns, data);

            Assert.IsNotNull(result);
        }
    }
}
