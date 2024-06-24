using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ceasier.Sap;
using SAP.Middleware.Connector;
using System.Collections.Generic;
using System;

namespace CeasierTests.Sap
{
    [TestClass]
    public class RfcFieldTest
    {
        [TestMethod]
        public void ConstructOnlyField()
        {
            var field = new RfcField("field");

            Assert.AreEqual("field", field.Name);
            Assert.AreEqual("field", field.Column);
            Assert.IsFalse(field.IsNullable);
        }

        [TestMethod]
        public void ConstructFieldAndColumn()
        {
            var field = new RfcField("field", "column");

            Assert.AreEqual("field", field.Name);
            Assert.AreEqual("column", field.Column);
            Assert.IsFalse(field.IsNullable);
        }

        [TestMethod]
        public void ConstructFieldBoolean()
        {
            var field = new RfcField("field", true);

            Assert.AreEqual("field", field.Name);
            Assert.AreEqual("field", field.Column);
            Assert.IsTrue(field.IsNullable);
        }

        [TestMethod]
        public void ConstructFieldColumnInteger()
        {
            var field = new RfcField("field", "column", 1);

            Assert.AreEqual("field", field.Name);
            Assert.AreEqual("column", field.Column);
            Assert.AreEqual(1, field.Value);
            Assert.IsFalse(field.IsNullable);
        }

        [TestMethod]
        public void ConstructFieldColumnType()
        {
            var field = new RfcField("field", "column", typeof(bool));

            Assert.AreEqual("field", field.Name);
            Assert.AreEqual("column", field.Column);
            Assert.IsNull(field.Value);
            Assert.IsFalse(field.IsNullable);
            Assert.AreEqual(typeof(bool), field.ValueType);
        }

        [TestMethod]
        public void ConstructFieldOnlyType()
        {
            var field = new RfcField("field", typeof(bool));

            Assert.AreEqual("field", field.Name);
            Assert.AreEqual("field", field.Column);
            Assert.IsNull(field.Value);
            Assert.IsTrue(field.IsNullable);
            Assert.AreEqual(typeof(bool), field.ValueType);
        }

        [TestMethod]
        public void ConstructRFCGetter()
        {
            var field = new RfcField("field", (IRfcStructure row) => null);

            Assert.AreEqual("field", field.Name);
            Assert.AreEqual("field", field.Column);
            Assert.IsNull(field.Value);
            Assert.IsFalse(field.IsNullable);
            Assert.AreEqual(typeof(string), field.ValueType);
        }

        [TestMethod]
        public void ConstructDbRowGetter()
        {
            var field = new RfcField("field", (Dictionary<string, object> row) => null);

            Assert.AreEqual("field", field.Name);
            Assert.AreEqual("field", field.Column);
            Assert.IsNull(field.Value);
            Assert.IsFalse(field.IsNullable);
            Assert.AreEqual(typeof(string), field.ValueType);
        }

        [TestMethod]
        public void SafeDate()
        {
            Assert.IsNull(RfcField.ToSafeDate(null));
            Assert.IsNull(RfcField.ToSafeDate(""));
            Assert.IsNull(RfcField.ToSafeDate("2023-12"));
            Assert.IsNull(RfcField.ToSafeDate("this is not a date"));
            Assert.AreEqual("2023-12-13", RfcField.ToSafeDate("2023-12-13")?.ToString("yyyy-MM-dd"));
            Assert.AreEqual("2023-12-13", RfcField.ToSafeDate("13.12.2023")?.ToString("yyyy-MM-dd"));
        }

        [TestMethod]
        public void GetValueFromDb()
        {
            var fields = new RfcField[]
            {
                new RfcField("foo"),
                new RfcField("bar", typeof(int)),
                new RfcField("baz", typeof(decimal)),
                new RfcField("qux", typeof(DateTime)),
                new RfcField("quux", true),
                new RfcField("corge", (Dictionary<string, object> row) => row["corge"]),
                new RfcField("grault", typeof(double)),
                new RfcField("garply"),
            };
            var values = new Dictionary<string, object>()
            {
                { "foo", "bar" },
                { "bar", 1 },
                { "baz", (decimal) 1.0 },
                { "qux", DateTime.Now },
                { "quux", "should not taken" },
                { "corge", "taken" },
                { "grault", 1.0 },
            };

            Assert.AreEqual("bar", fields[0].GetValue(values));
            Assert.AreEqual(1, fields[1].GetValue(values));
            Assert.AreEqual((decimal) 1.0, fields[2].GetValue(values));
            Assert.AreEqual(values["qux"].ToString(), fields[3].GetValue(values).ToString());
            Assert.AreEqual("should not taken", fields[4].GetValue(values));
            Assert.AreEqual("taken", fields[5].GetValue(values));
            Assert.AreEqual(1.0, fields[6].GetValue(values));
            Assert.AreEqual(null, fields[7].GetValue(values));
        }
    }
}
