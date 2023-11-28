using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ceasier.Sap;

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
            Assert.AreEqual(typeof(bool), field.Type);
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
    }
}
