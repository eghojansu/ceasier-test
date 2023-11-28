﻿using Ceasier.Sap;
using Ceasier;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;

namespace CeasierTests.Sap
{
    [TestClass]
    public class RfcFunTest
    {
        [TestMethod]
        public void SummarizedTest()
        {
            var config = new Beasier();
            var fnName = config.Configuration.GetSection("RFCTest1:Name").Value;
            var fnParams = CreateParams(config.Configuration.GetSection("RFCTest1:Params").Value.Split(';'));
            var resultTable = config.Configuration.GetSection("RFCTest1:Result").Value;
            var applyName = config.Configuration.GetSection("RFCTest1:ApplyName").Value;
            var applyParams = CreateParams(config.Configuration.GetSection("RFCTest1:ApplyParams").Value.Split(';'));

            Assert.IsNotNull(fnName);
            Assert.IsNotNull(fnParams);
            Assert.IsNotNull(resultTable);
            Assert.IsNotNull(applyName);
            Assert.IsNotNull(applyParams);
            Assert.IsTrue(fnParams.Count > 0);
            Assert.IsTrue(applyParams.Count > 0);

            var fn = config.GetRFCFunction(fnName, "PROD", "SYS");

            fn.SetResultTable(resultTable);
            fn.SetArgs(fnParams);
            fn.ApplyArgs(applyName, applyParams);
            fn.Run();

            var total = fn.Result.Count;
            var fields = new[]
            {
                new RfcField("MBLNR", "GRN"),
                new RfcField("ZEILE", "GRNLine", typeof(int)),
                new RfcField("BUDAT", "GRNDate", typeof(DateTime)),
                new RfcField("ERFMG", "Qty", typeof(decimal)),
            };
            var stock = fn.ToDataTable("test", fields);

            Assert.IsTrue(fn.Success);
            Assert.IsNull(fn.Message);
            Assert.IsNull(fn.MessageType);
            Assert.IsNotNull(stock);
            Assert.IsNotNull(fn.ResultFirst);
            Assert.AreEqual(total, stock.Rows.Count);
        }

        private Dictionary<string, object> CreateParams(string[] lines)
        {
            var values = new Dictionary<string, object>();

            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split('=');

                values.Add(parts[0], parts[1]);
            }

            return values;
        }
    }
}
