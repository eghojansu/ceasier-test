using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ceasier.Utils;
using System.Collections.Generic;

namespace CeasierTests.Utils
{
    [TestClass]
    public class CommonTest
    {
        [TestMethod]
        public void SummarizedTest()
        {
            Assert.AreEqual(8, Common.Seed().Length);
            Assert.AreEqual(3, Common.Seed(3).Length);
        }

        [TestMethod]
        public void ObjectValues()
        {
            var _nothing = Common.ObjectValues(null);
            var _map = Common.ObjectValues(new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("foo", "bar") });
            var _general = Common.ObjectValues(new { name = "foo" });
            var _dictionary = Common.ObjectValues(new Dictionary<string, object>() { { "name", "foo" } });

            Assert.AreEqual(0, _nothing.Count);
            Assert.AreEqual(1, _map.Count);
            Assert.AreEqual(1, _general.Count);
            Assert.AreEqual(1, _dictionary.Count);
        }
    }
}
