using Ceasier.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using Ceasier;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace CeasierTests.Db
{
    [TestClass]
    public class MssqlTest
    {
        [TestMethod]
        public void SummarizedTest()
        {
            var db = GetDb();

            // ensure table not exists
            Assert.IsTrue(db.Drop("my_test", true));
            Assert.IsFalse(db.Exists("my_test"));

            // create table
            Assert.IsTrue(db.Create("my_test", new string[]
            {
                "foo varchar(16) not null",
                "bar int not null",
                "--",
                "",
            }));
            Assert.IsTrue(db.Exists("my_test"));
            Assert.AreEqual(0, db.Count("my_test"));

            // insert batch
            var dt = new DataTable("my_test");

            dt.Columns.Add("foo", typeof(string));
            dt.Columns.Add("bar", typeof(int));
            dt.Rows.Add("a", 1);
            dt.Rows.Add("b", 2);

            db.Insert(dt);

            Assert.AreEqual(2, db.Count("my_test"));

            // raw query
            var rows = db.QueryResult("select * from my_test where foo = @foo and bar = @b", new { foo = "a", bar = new SqlParameter("b", 1) });

            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("a", rows[0]["foo"]);
            Assert.AreEqual(1, rows[0]["bar"]);

            // Finding
            var rowsFound = db.Find("my_test");

            Assert.AreEqual(2, rowsFound.Count);

            var rowsOrdered = db.Find("my_test", null, new Dictionary<string, object>() { { "order", "foo DESC" } });

            Assert.AreEqual(2, rowsOrdered.Count);
            Assert.AreEqual("b", rowsOrdered[0]["foo"]);

            // first
            var rowFirst = db.First("my_test");

            Assert.AreEqual("a", rowFirst["foo"]);
            Assert.AreEqual(1, rowFirst["bar"]);

            var rowFound = db.First("my_test", new { foo = "b" });

            Assert.AreEqual("b", rowFound["foo"]);
            Assert.AreEqual(2, rowFound["bar"]);

            var rowNotFound = db.First("my_test", new { foo = "x" });

            Assert.IsNull(rowNotFound);

            // manual fetch

            var readerRow = db.Fetch("my_test");
            var readerStr = "";

            while (readerRow.Read())
            {
                readerStr += readerRow["foo"].ToString();
            }

            readerRow.Close();

            Assert.AreEqual("ab", readerStr);

            // raw result
            var rowsRaw = db.QueryResult("select * from my_test order by foo desc");

            Assert.AreEqual(2, rowsRaw.Count);
            Assert.AreEqual("b", rowsRaw[0]["foo"]);

            // raw first
            var firstRaw = db.QueryFirst("select * from my_test order by foo desc");

            Assert.AreEqual("b", firstRaw["foo"]);
            Assert.AreEqual(2, firstRaw["bar"]);

            // procedure call
            Assert.IsTrue(db.TryQuery(string.Join("\n", new string[] {
                "CREATE OR ALTER PROCEDURE count_test",
                "AS",
                "BEGIN",
                "DECLARE @c INT",
                "SELECT @c = count(*) FROM my_test",
                "RETURN @c",
                "END",
            })));
            Assert.IsTrue(db.TryQuery(string.Join("\n", new string[] {
                "CREATE OR ALTER PROCEDURE count_test_multiply",
                "@power INT",
                "AS",
                "BEGIN",
                "DECLARE @c INT",
                "SELECT @c = count(*) FROM my_test",
                "RETURN @c * @power",
                "END",
            })));
            Assert.AreEqual(2, db.SPRun<int>("count_test"));
            Assert.AreEqual(4, db.SPRun<int>("count_test_multiply", new { power = 2 }));

            // procedure returning table
            Assert.IsTrue(db.TryQuery(string.Join("\n", new string[] {
                "CREATE OR ALTER PROCEDURE result_test",
                "AS",
                "BEGIN",
                "SELECT foo a, bar b FROM my_test ORDER BY foo",
                "END",
            })));

            var fnRows = db.SPResult("result_test");
            var fnRow = db.SPFirst("result_test");

            Assert.AreEqual(2, fnRows.Count);
            Assert.AreEqual("a", fnRows[0]["a"]);
            Assert.AreEqual(1, fnRows[0]["b"]);
            Assert.AreEqual("a", fnRow["a"]);
            Assert.AreEqual(1, fnRow["b"]);

            // insert batch with table type
            var noType = db.Query<int>($"SELECT ISNULL(TYPE_ID('my_test_type'), 0) c", true) < 1;

            if (noType)
            {
                db.TryQuery(string.Join("\n", new string[] {
                    "CREATE TYPE my_test_type AS TABLE",
                    "(",
                    "foo varchar(16) not null,",
                    "bar int not null",
                    ")",
                }));
            }

            Assert.IsTrue(db.TryQuery(string.Join("\n", new string[] {
                "CREATE OR ALTER PROCEDURE insert_test",
                "@test_value my_test_type READONLY",
                "AS",
                "BEGIN",
                "INSERT INTO my_test (foo, bar) SELECT foo, bar FROM @test_value",
                "END",
            })));

            var dt2 = new DataTable("test_value");

            dt2.Columns.Add("foo", typeof(string));
            dt2.Columns.Add("bar", typeof(int));
            dt2.Rows.Add("c", 3);
            dt2.Rows.Add("d", 4);

            db.Insert("insert_test", dt2);

            Assert.AreEqual(4, db.Count("my_test"));

            // raw query
            Assert.AreEqual(1, db.Query<int>("update my_test set bar = 55 where foo = 'a'"));
            Assert.AreEqual(55, db.Query<int>("select bar from my_test where foo = 'a'", true));

            // truncate
            Assert.IsTrue(db.Truncate("my_test"));
            Assert.AreEqual(0, db.Count("my_test"));

            // Manipulate
            Assert.AreEqual(1, db.Insert("my_test", new { foo = "e", bar = 5 }));
            Assert.AreEqual(1, db.Insert("my_test", new { foo = "f", bar = 6 }));

            Assert.AreEqual(1, db.Update("my_test", new { foo = "e1" }, new { bar = 5 }));
            Assert.AreEqual(1, db.Update("my_test", new { foo = "e1" }, new { bar = 5 }));

            Assert.AreEqual(1, db.Delete("my_test", new { bar = 5 }));
            Assert.AreEqual(1, db.Count("my_test"));

            Assert.AreEqual(1, db.Delete("my_test", null));
            Assert.AreEqual(0, db.Count("my_test"));

            // error checking
            Assert.ThrowsException<Exception>(() =>
            {
                db.Truncate("my_test", true);
            });

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var dte = dt;

                dte.Rows.Add("c", null);

                db.Insert(dte);
            });

            // ensure nothing left
            Assert.IsTrue(db.Drop("my_test", true));
            Assert.IsFalse(db.Exists("my_test"));
            Assert.IsFalse(db.Drop("my_test"));
        }

        [TestMethod]
        public void DbInvalidConnection()
        {
            Assert.ThrowsException<Exception>(() =>
            {
                var db = new Beasier().GetMssql("MSCON_INVALID");

                db.Query("select 1 foo");
            });
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var db = new Beasier().GetMssql("MSCON_NONE");

                db.Query("select 1 foo");
            });
        }

        [TestMethod]
        public void QbInsert()
        {
            var qb = CreateMsQueryBuilder().Insert("test", new { foo = "bar" });
            var sql = "INSERT INTO test (foo) VALUES (@foo)";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(1, qb.Params.Count);
            Assert.AreEqual("bar", qb.Params["@foo"]);
        }

        [TestMethod]
        public void QbUpdate()
        {
            var qb = CreateMsQueryBuilder().Update("test", new { foo = "bar" }, null);
            var sql = "UPDATE test SET foo = @foo";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(1, qb.Params.Count);
            Assert.AreEqual("bar", qb.Params["@foo"]);
        }

        [TestMethod]
        public void QbUpdateWithFilter()
        {
            var qb = CreateMsQueryBuilder().Update("test", new { foo = "bar" }, new { id = 1});
            var sql = "UPDATE test SET foo = @foo WHERE id = @id";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(2, qb.Params.Count);
            Assert.AreEqual("bar", qb.Params["@foo"]);
            Assert.AreEqual(1, qb.Params["@id"]);
        }

        [TestMethod]
        public void QbDelete()
        {
            var qb = CreateMsQueryBuilder().Delete("test", null);
            var sql = "DELETE FROM test";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(0, qb.Params.Count);
        }

        [TestMethod]
        public void QbDeleteWithFilter()
        {
            var qb = CreateMsQueryBuilder().Delete("test", new { id = 1 });
            var sql = "DELETE FROM test WHERE id = @id";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(1, qb.Params.Count);
            Assert.AreEqual(1, qb.Params["@id"]);
        }

        [TestMethod]
        public void QbSelect()
        {
            var qb = CreateMsQueryBuilder().Select().From("test", "a").GroupBy("name").OrderBy("id desc").Limit(10);
            var sql = "SELECT TOP 10 * FROM test AS a GROUP BY name ORDER BY id desc";

            Assert.AreEqual(sql, qb.Sql);
        }

        [TestMethod]
        public void QbSelectWithOffset()
        {
            var qb = CreateMsQueryBuilder().Select("id").From("test").OrderBy("id desc").Limit(10, 5);
            var sql = "SELECT id FROM test ORDER BY id desc OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY";

            Assert.AreEqual(sql, qb.Sql);
        }

        [TestMethod]
        public void QbSelectWithColumns()
        {
            var qb = CreateMsQueryBuilder().Select(new string[] { "foo", "bar" }).From("test").From("bar");
            var sql = "SELECT foo, bar FROM bar";

            Assert.AreEqual(sql, qb.Sql);
        }

        [TestMethod]
        public void QbSelectWithCriteria()
        {
            var qb = CreateMsQueryBuilder().Select("foo", "bar").From("test").Where("id", 1).OrWhere("idx", 2).AndWhere("name", "foo");
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
            var qb = CreateMsQueryBuilder().Select("foo", "bar").From("test").Where(new Dictionary<string, object>()
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
            var qb = CreateMsQueryBuilder().Find("test", null, null);
            var sql = "SELECT * FROM test";

            Assert.AreEqual(sql, qb.Sql);
            Assert.IsTrue(qb.Scalar);
        }

        [TestMethod]
        public void QbFirst()
        {
            var qb = CreateMsQueryBuilder().First("test", false, new Dictionary<string, object>() { { "limit", 10 } });
            var sql = "SELECT TOP 1 * FROM test";

            Assert.AreEqual(sql, qb.Sql);
            Assert.IsFalse(qb.Scalar);
        }

        [TestMethod]
        public void QbCallFunction()
        {
            var qb = CreateMsQueryBuilder().CallFunction("test", null, false);
            var sql = "SELECT * FROM test()";

            Assert.AreEqual(sql, qb.Sql);
            Assert.IsFalse(qb.Scalar);
        }

        [TestMethod]
        public void QbCallFunctionParameters()
        {
            var qb = CreateMsQueryBuilder().CallFunction("test", new { foo = 1 }, false);
            var sql = "SELECT * FROM test(@foo)";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(1, qb.Params.Count);
            Assert.AreEqual(1, qb.Params["@foo"]);
            Assert.IsFalse(qb.Scalar);
        }

        [TestMethod]
        public void QbCallFunctionParametersScalar()
        {
            var qb = CreateMsQueryBuilder().CallFunction("test", true, false);
            var sql = "SELECT * FROM test()";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(0, qb.Params.Count);
            Assert.IsTrue(qb.Scalar);
        }

        private MsQueryBuilder CreateMsQueryBuilder()
        {
            return new MsQueryBuilder();
        }

        private Mssql GetDb()
        {
            return new Beasier().GetMssql("MSCON");
        }
    }
}
