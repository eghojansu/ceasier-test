using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System;
using Ceasier.Sql;
using Ceasier;

namespace CeasierTests
{
    [TestClass]
    public class BeasierTest
    {
        [TestMethod]
        public void SummarizedTest()
        {
            var config = new Beasier();

            Assert.AreEqual("MyApp", config.Value.App.Name);
            Assert.AreEqual("MA", config.Value.App.Short);
            Assert.AreEqual("1.0.0", config.Value.App.Version);
            Assert.AreEqual(2023, config.Value.App.Year);

            Assert.AreEqual("MyName", config.Value.Company.Name);
            Assert.AreEqual("MN", config.Value.Company.Short);
            Assert.AreEqual("My Street 1st", config.Value.Company.Address);
            Assert.AreEqual("MyCity", config.Value.Company.City);

            Assert.AreEqual("MN MyApp", config.Value.Title);
            Assert.AreEqual("MA", config.Value.ShortTitle);

            Assert.IsTrue(Regex.IsMatch(config.GetDsn("MSCON"), @"^Data Source=[^;]+;Initial Catalog=TES_DB;User Id=sa;Password=[^;]+;Integrated Security=false;$"));
            Assert.IsTrue(Regex.IsMatch(config.GetDsn("PGCON"), @"^Server=[^;]+;User Id=postgres;Password=[^;]+;Database=TES_DB;Pooling=false;$"));

            Assert.IsInstanceOfType(config.GetMssql("MSCON"), typeof(Mssql));
            Assert.IsInstanceOfType(config.GetPgsql("PGCON"), typeof(Pgsql));

            Assert.IsNotNull(config.GetRFCActor("SYS").User);
            Assert.ThrowsException<Exception>(() => config.GetRFCActor("NONE"));

            Assert.AreEqual("PROD", config.GetRFCConnection("PROD").Name);
            Assert.IsNotNull(config.GetRFCConnection("PROD", "SYS").Actor.User);
            Assert.AreEqual("MYUSER", config.GetRFCConnection("PROD", "MYUSER", "MYPASSWORD").Actor.User);
            Assert.ThrowsException<Exception>(() => config.GetRFCConnection("NONE"));
        }

        [TestMethod]
        public void RFCConfiguration()
        {
            var config = new Beasier();
            var rfc1 = config.GetRFCConfiguration("PROD", "SYS");
            var rfc2 = config.GetRFCConfiguration("PROD", "foo", "bar");

            Assert.IsNotNull(rfc1);
            Assert.IsNotNull(rfc2);
        }
    }
}
