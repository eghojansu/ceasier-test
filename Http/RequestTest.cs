using Ceasier.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace CeasierTests.Http
{
    [TestClass]
    public class RequestTest
    {
        [TestMethod]
        public void ConstructQueryString()
        {
            var queries = "foo=bar&bar=1&filters[0][field]=name&filters[0][value]=1";
            var req = new Request(queries);

            Assert.AreEqual("bar", req.Value("foo"));
            Assert.AreEqual("1", req.Value("bar"));
            Assert.AreEqual(1, req.Value<int>("bar"));
            Assert.IsNull(req.Value("none"));
            Assert.AreEqual("name", req.Value<List<object>>("filters.*.field").First());

            var filters = req.GetQueries("filters.*.field", "filters.*.value", "FOO");

            Assert.IsNotNull(filters);
            Assert.AreEqual("name", filters["filters.*.field"].First());
            Assert.AreEqual("1", filters["filters.*.value"].First());
            Assert.AreEqual("bar", filters["FOO"].First());
        }

        [TestMethod]
        public void ConstructValues()
        {
            var queries = new NameValueCollection()
            {
                { "foo", "bar" },
                { "bar", "1" },
            };
            var form = new NameValueCollection()
            {
                { "baz", "qux" },
            };
            var req = new Request(queries, form);

            Assert.AreEqual("bar", req.Value("foo"));
            Assert.AreEqual("1", req.Value("bar"));
            Assert.AreEqual(1, req.Value<int>("bar"));
            Assert.AreEqual("qux", req.Value("baz"));
        }

        [TestMethod]
        public void ConstructDefault()
        {
            var req = new Request();

            Assert.IsNull(req.Value("none"));
        }
    }
}
