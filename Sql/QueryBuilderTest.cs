using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using Ceasier;
using Ceasier.Sql;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using Ceasier.Sql.Driver;

namespace CeasierTests.Sql
{
    [TestClass]
    public class QueryBuilderTest
    {
        [TestMethod]
        public void QbInsert()
        {
            var qb = CreateQueryBuilder().Insert("test", new { foo = "bar" });
            var sql = "INSERT INTO test (foo) VALUES (@foo)";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(1, qb.Params.Count);
            Assert.AreEqual("bar", qb.Params["@foo"]);
        }

        [TestMethod]
        public void QbUpdate()
        {
            var qb = CreateQueryBuilder().Update("test", new { foo = "bar" }, null);
            var sql = "UPDATE test SET foo = @foo";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(1, qb.Params.Count);
            Assert.AreEqual("bar", qb.Params["@foo"]);
        }

        [TestMethod]
        public void QbUpdateWithFilter()
        {
            var qb = CreateQueryBuilder().Update("test", new { foo = "bar" }, new { id = 1});
            var sql = "UPDATE test SET foo = @foo WHERE id = @id";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(2, qb.Params.Count);
            Assert.AreEqual("bar", qb.Params["@foo"]);
            Assert.AreEqual(1, qb.Params["@id"]);
        }

        [TestMethod]
        public void QbDelete()
        {
            var qb = CreateQueryBuilder().Delete("test", null);
            var sql = "DELETE FROM test";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(0, qb.Params.Count);
        }

        [TestMethod]
        public void QbDeleteWithFilter()
        {
            var qb = CreateQueryBuilder().Delete("test", new { id = 1 });
            var sql = "DELETE FROM test WHERE id = @id";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(1, qb.Params.Count);
            Assert.AreEqual(1, qb.Params["@id"]);
        }

        [TestMethod]
        public void QbSelect()
        {
            var qb = CreateQueryBuilder().Select().From("test", "a").GroupBy("name").OrderBy("id desc").Limit(10);
            var sql = "SELECT TOP 10 * FROM test AS a GROUP BY name ORDER BY id desc";

            Assert.AreEqual(sql, qb.Sql);
        }

        [TestMethod]
        public void QbSelectWithOffset()
        {
            var qb = CreateQueryBuilder().Select("id").From("test").OrderBy("id desc").Limit(10, 5);
            var sql = "SELECT id FROM test ORDER BY id desc OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY";

            Assert.AreEqual(sql, qb.Sql);
        }

        [TestMethod]
        public void QbSelectWithColumns()
        {
            var qb = CreateQueryBuilder().Select(new string[] { "foo", "bar" }).From("test").From("bar");
            var sql = "SELECT foo, bar FROM bar";

            Assert.AreEqual(sql, qb.Sql);
        }

        [TestMethod]
        public void QbSelectWithCriteria()
        {
            var qb = CreateQueryBuilder().Select("foo", "bar").From("test").Where("id", 1).OrWhere("idx", 2).AndWhere("name", "foo");
            var sql = "SELECT foo, bar FROM test WHERE id = @id OR idx = @idx AND name = @name";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(3, qb.Params.Count);
            Assert.AreEqual(1, qb.Params["@id"]);
            Assert.AreEqual(2, qb.Params["@idx"]);
            Assert.AreEqual("foo", qb.Params["@name"]);
        }

        [TestMethod]
        public void QbSelectWithCriteriaMap()
        {
            var qb = CreateQueryBuilder().Select("foo", "bar").From("test").Where(new Dictionary<string, object>()
            {
                { "id", 1 },
                { "idx", 2 },
                { "name", "foo" },
            });
            var sql = "SELECT foo, bar FROM test WHERE id = @id AND idx = @idx AND name = @name";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(3, qb.Params.Count);
            Assert.AreEqual(1, qb.Params["@id"]);
            Assert.AreEqual(2, qb.Params["@idx"]);
            Assert.AreEqual("foo", qb.Params["@name"]);
        }

        [TestMethod]
        public void QbFind()
        {
            var qb = CreateQueryBuilder().Find("test", null, null);
            var sql = "SELECT * FROM test";

            Assert.AreEqual(sql, qb.Sql);
            Assert.IsTrue(qb.Scalar);
        }

        [TestMethod]
        public void QbFirst()
        {
            var qb = CreateQueryBuilder().First("test", false, new Dictionary<string, object>() { { "limit", 10 } });
            var sql = "SELECT TOP 1 * FROM test";

            Assert.AreEqual(sql, qb.Sql);
            Assert.IsFalse(qb.Scalar);
        }

        [TestMethod]
        public void QbCallFunction()
        {
            var qb = CreateQueryBuilder().CallFunction("test", null, false);
            var sql = "SELECT * FROM test()";

            Assert.AreEqual(sql, qb.Sql);
            Assert.IsFalse(qb.Scalar);
        }

        [TestMethod]
        public void QbCallFunctionParameters()
        {
            var qb = CreateQueryBuilder().CallFunction("test", new { foo = 1 }, false);
            var sql = "SELECT * FROM test(@foo)";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(1, qb.Params.Count);
            Assert.AreEqual(1, qb.Params["@foo"]);
            Assert.IsFalse(qb.Scalar);
        }

        [TestMethod]
        public void QbCallFunctionParametersScalar()
        {
            var qb = CreateQueryBuilder().CallFunction("test", null, true);
            var sql = "SELECT * FROM test()";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(0, qb.Params.Count);
            Assert.IsTrue(qb.Scalar);
        }

        [TestMethod]
        public void QbCreateTableDefinitions()
        {
            var sql = "(foo varchar(16), bar int) options";

            Assert.AreEqual(sql, QueryBuilder.CreateTableDefinitions(new string[]
            {
                "foo varchar(16)",
                "bar int",
                "--",
                "options",
            }));
        }

        private QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder(new Mssql());
        }
    }
}
