using Ceasier.Sap;
using Ceasier;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using SAP.Middleware.Connector;

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
                new RfcField("seed", null, "SEED"),
                new RfcField("comment1", true),
                new RfcField("getter", (IRfcStructure a) => null),
            };
            var data = fn.ToDataTable("test", fields);

            Assert.IsTrue(fn.Success);
            Assert.IsNull(fn.Message);
            Assert.IsNull(fn.MessageType);
            Assert.IsNotNull(data);
            Assert.IsNotNull(fn.ResultFirst);
            Assert.AreEqual(total, data.Rows.Count);
        }

        [TestMethod]
        public void ApplyParamsManualTest()
        {
            var config = new Beasier();
            var fnName = config.Configuration.GetSection("RFCTest1:Name").Value;
            var fnParams = CreateParams(config.Configuration.GetSection("RFCTest1:Params").Value.Split(';'));
            var resultTable = config.Configuration.GetSection("RFCTest1:Result").Value;
            var applyName = config.Configuration.GetSection("RFCTest1:ApplyName").Value;
            var applyParams = CreateParams(config.Configuration.GetSection("RFCTest1:ApplyParams").Value.Split(';'));
            var applyColumns = CreateFields(applyParams);

            Assert.IsNotNull(fnName);
            Assert.IsNotNull(fnParams);
            Assert.IsNotNull(resultTable);
            Assert.IsNotNull(applyName);
            Assert.IsNotNull(applyParams);
            Assert.IsTrue(fnParams.Count > 0);
            Assert.IsTrue(applyParams.Count > 0);

            var fn = config.GetRFCFunction(fnName, "PROD", "SYS");
            var fnArgs = fn.GetTable(applyName);

            fn.SetResultTable(resultTable);
            fn.SetArgs(fnParams);
            fn.ApplyArgs(fnArgs, applyColumns, applyParams);
            fn.Run();

            var total = fn.Result.Count;
            var fields = new[]
            {
                new RfcField("MBLNR", "GRN"),
                new RfcField("ZEILE", "GRNLine", typeof(int)),
                new RfcField("BUDAT", "GRNDate", typeof(DateTime)),
                new RfcField("ERFMG", "Qty", typeof(decimal)),
                new RfcField("seed", null, "SEED"),
                new RfcField("comment1", true),
                new RfcField("getter", (IRfcStructure a) => null),
            };
            var data = fn.ToDataTable("test", fields);

            Assert.IsTrue(fn.Success);
            Assert.IsNull(fn.Message);
            Assert.IsNull(fn.MessageType);
            Assert.IsNotNull(data);
            Assert.IsNotNull(fn.ResultFirst);
            Assert.AreEqual(total, data.Rows.Count);
        }

        [TestMethod]
        public void ApplyParamsManualByFnNameTest()
        {
            var config = new Beasier();
            var fnName = config.Configuration.GetSection("RFCTest1:Name").Value;
            var fnParams = CreateParams(config.Configuration.GetSection("RFCTest1:Params").Value.Split(';'));
            var resultTable = config.Configuration.GetSection("RFCTest1:Result").Value;
            var applyName = config.Configuration.GetSection("RFCTest1:ApplyName").Value;
            var applyParams = CreateParams(config.Configuration.GetSection("RFCTest1:ApplyParams").Value.Split(';'));
            var applyColumns = CreateFields(applyParams);

            Assert.IsNotNull(fnName);
            Assert.IsNotNull(fnParams);
            Assert.IsNotNull(resultTable);
            Assert.IsNotNull(applyName);
            Assert.IsNotNull(applyParams);
            Assert.IsTrue(fnParams.Count > 0);
            Assert.IsTrue(applyParams.Count > 0);

            var fn = config.GetRFCFunction(fnName);
            var fnArgs = fn.GetTable(applyName);

            fn.SetResultTable(resultTable);
            fn.SetArgs(fnParams);
            fn.ApplyArgs(fnArgs, applyColumns, applyParams);
            fn.Run();

            var total = fn.Result.Count;
            var fields = new[]
            {
                new RfcField("MBLNR", "GRN"),
                new RfcField("ZEILE", "GRNLine", typeof(int)),
                new RfcField("BUDAT", "GRNDate", typeof(DateTime)),
                new RfcField("ERFMG", "Qty", typeof(decimal)),
                new RfcField("seed", null, "SEED"),
                new RfcField("comment1", true),
                new RfcField("getter", (IRfcStructure a) => null),
            };
            var data = fn.ToDataTable("test", fields);

            Assert.IsTrue(fn.Success);
            Assert.IsNull(fn.Message);
            Assert.IsNull(fn.MessageType);
            Assert.IsNotNull(data);
            Assert.IsNotNull(fn.ResultFirst);
            Assert.AreEqual(total, data.Rows.Count);
        }

        [TestMethod]
        public void NullStringRFCResultTest()
        {
            var config = new Beasier();
            var fnName = config.Configuration.GetSection("RFCTest2:Name").Value;
            var fnParams = CreateParams(config.Configuration.GetSection("RFCTest2:Params").Value.Split(';'));
            var resultTable = config.Configuration.GetSection("RFCTest2:Result").Value;

            Assert.IsNotNull(fnName);
            Assert.IsNotNull(fnParams);
            Assert.IsNotNull(resultTable);
            Assert.IsTrue(fnParams.Count > 0);

            var fn = config.GetRFCFunction(fnName, "PROD", "SYS");

            fn.SetResultTable(resultTable);
            fn.SetArgs(fnParams);
            fn.Run();

            var total = fn.Result.Count;
            var fields = new[]
            {
                new RfcField("MATNR", "matnr"),
                new RfcField("SOBKZ", "sobkz"),
            };
            var data = fn.ToDataTable("test", fields);

            Assert.IsTrue(fn.Success);
            Assert.IsNull(fn.Message);
            Assert.IsNull(fn.MessageType);
            Assert.IsNotNull(data);
            Assert.IsNotNull(fn.ResultFirst);
            Assert.AreEqual(total, data.Rows.Count);
        }

        [TestMethod]
        public void SettingReturnTable()
        {
            var config = new Beasier();
            var fnName = config.Configuration.GetSection("RFCTest1:Name").Value;
            var fn = config.GetRFCFunction(fnName, "PROD", "SYS");

            fn.SetReturnTable("TEST");

            Assert.IsTrue(fn.Success);
            Assert.IsNull(fn.Message);
            Assert.IsNull(fn.MessageType);

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var res = fn.Result;
            });
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

        private RfcField[] CreateFields(Dictionary<string, object> args)
        {
            var values = new List<RfcField>();

            foreach (var item in args)
            {
                values.Add(new RfcField(item.Key, item.Key, item.Value));
            }

            return values.ToArray();
        }
    }
}
