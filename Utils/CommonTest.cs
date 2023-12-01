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
        public void CaseTitle()
        {
            Assert.AreEqual("Foo", Common.CaseTitle("Foo"));
            Assert.AreEqual("Foo", Common.CaseTitle("foo"));
            Assert.AreEqual("Foo Bar", Common.CaseTitle("foo-bar"));
            Assert.AreEqual("Foo Bar", Common.CaseTitle("foo_bar"));
            Assert.AreEqual("Foo Bar", Common.CaseTitle("foo bar"));
            Assert.AreEqual("Foo", Common.CaseTitle("foo "));
            Assert.AreEqual("SO Non", Common.CaseTitle("so_non"));
        }

        [TestMethod]
        public void CharSequences()
        {
            Assert.AreEqual("A", Common.CharSequence(1));
            Assert.AreEqual("B", Common.CharSequence(2));
            Assert.AreEqual("J", Common.CharSequence(10));
            Assert.AreEqual("Z", Common.CharSequence(26));
            Assert.AreEqual("AA", Common.CharSequence(27));
            Assert.AreEqual("AZ", Common.CharSequence(52));
            Assert.AreEqual("BA", Common.CharSequence(53));
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

        [TestMethod]
        public void ObjectMap()
        {
            var n1 = 0;
            var n2 = "";
            var n3 = "";
            var n4 = "";
            var n5 = 0;
            var expected = ";name=foo";

            Common.ObjectMap(null, (string n, object v) =>
            {
                n1++;
            });
            Assert.AreEqual(0, n1);

            Common.ObjectMap(new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("name", "foo") }, (string n, object v) =>
            {
                n2 = $"{n2};{n}={v}";
            });
            Assert.AreEqual(expected, n2);

            Common.ObjectMap(new Dictionary<string, object>() { { "name", "foo" } }, (string n, object v) =>
            {
                n3 = $"{n3};{n}={v}";
            });
            Assert.AreEqual(expected, n3);

            Common.ObjectMap(new { name = "foo" }, (string n, object v) =>
            {
                n4 = $"{n4};{n}={v}";
            });
            Assert.AreEqual(expected, n4);

            Common.ObjectMap(true, (string n, object v) =>
            {
                n5++;
            });
            Assert.AreEqual(0, n5);
        }
    }
}
