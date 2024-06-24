using Ceasier.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using NpgsqlTypes;
using Ceasier;
using Npgsql;
using System.Collections.Generic;
using Ceasier.Sql.Driver;
using System.Data.SqlClient;

namespace CeasierTests.Sql
{
    [TestClass]
    public class DbTest
    {
        [TestMethod]
        public void MssqlDriver()
        {
            var db = GetMssql();

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

            var readerRow = db.Read("my_test");
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
            Assert.IsTrue(db.TryRun(string.Join("\n", new string[] {
                "CREATE OR ALTER PROCEDURE count_test",
                "AS",
                "BEGIN",
                "DECLARE @c INT",
                "SELECT @c = count(*) FROM my_test",
                "RETURN @c",
                "END",
            })));
            Assert.IsTrue(db.TryRun(string.Join("\n", new string[] {
                "CREATE OR ALTER PROCEDURE count_test_multiply",
                "@power INT",
                "AS",
                "BEGIN",
                "DECLARE @c INT",
                "SELECT @c = count(*) FROM my_test",
                "RETURN @c * @power",
                "END",
            })));
            Assert.AreEqual(2, db.Sp<int>("count_test"));
            Assert.AreEqual(4, db.Sp<int>("count_test_multiply", new { power = 2 }));

            // procedure returning table
            Assert.IsTrue(db.TryRun(string.Join("\n", new string[] {
                "CREATE OR ALTER PROCEDURE result_test",
                "AS",
                "BEGIN",
                "SELECT foo a, bar b FROM my_test ORDER BY foo",
                "END",
            })));

            var fnRows = db.SpResult("result_test");
            var fnRow = db.SpFirst("result_test");

            Assert.AreEqual(2, fnRows.Count);
            Assert.AreEqual("a", fnRows[0]["a"]);
            Assert.AreEqual(1, fnRows[0]["b"]);
            Assert.AreEqual("a", fnRow["a"]);
            Assert.AreEqual(1, fnRow["b"]);
            Assert.AreEqual("a", db.Sp<string>("result_test", null, true));

            // insert batch with table type
            var typeExists = db.Query<int>($"SELECT ISNULL(TYPE_ID('my_test_type'), 0) c", null, true) > 0;

            Assert.AreEqual(!typeExists, db.TryRun(string.Join("\n", new string[] {
                "CREATE TYPE my_test_type AS TABLE",
                "(",
                "foo varchar(16) not null,",
                "bar int not null",
                ")",
            })));

            Assert.IsTrue(db.TryRun(string.Join("\n", new string[] {
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

            // manual function fetch
            var readerRow2 = db.Sp("result_test");
            var readerStr2 = "";

            while (readerRow2.Read())
            {
                readerStr2 += readerRow2["a"].ToString();
            }

            readerRow2.Close();

            Assert.AreEqual("abcd", readerStr2);

            // manual query fetch
            var readerRow3 = db.Query("select * from my_test");
            var readerStr3 = "";

            while (readerRow3.Read())
            {
                readerStr3 += readerRow3["foo"].ToString();
            }

            readerRow3.Close();

            Assert.AreEqual("abcd", readerStr3);

            // raw query
            Assert.AreEqual(1, db.Query<int>("update my_test set bar = 55 where foo = 'a'", null, false));
            Assert.AreEqual(55, db.Query<int>("select bar from my_test where foo = 'a'", null, true));
            Assert.AreEqual(55, db.Query<int>("select bar from my_test where foo = @foo", new { foo = "a" }, true));

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
        public void PgsqlDriver()
        {
            var db = GetPgsql();

            // ensure no table
            Assert.IsTrue(db.Drop("my_test", true));
            Assert.IsFalse(db.Exists("my_test"));

            // create table
            Assert.IsTrue(db.Create("my_test", new string[]
            {
                "foo varchar not null",
                "bar int not null",
                "--",
                ""
            }));
            Assert.IsTrue(db.Exists("my_test"));
            Assert.AreEqual(0, db.Count("my_test"));

            // insert batch with copy
            var dt = new DataTable("my_test");

            dt.Columns.Add("foo", typeof(string));
            dt.Columns.Add("bar", typeof(int));
            dt.Rows.Add("a", 1);
            dt.Rows.Add("b", 2);

            db.Insert(dt);

            Assert.AreEqual(2, db.Count("my_test"));

            // raw query
            var rows = db.QueryResult("select * from my_test where bar = $1", new object[] { new NpgsqlParameter<int>() { Value = 1 } });

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

            var readerRow = db.Read("my_test");
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

            // call function
            Assert.IsTrue(db.TryRun(string.Join(" ", new string[] {
                "CREATE OR REPLACE FUNCTION public.count_test()",
                "RETURNS integer",
                "LANGUAGE plpgsql",
                "AS $function$",
                "DECLARE _count INT;",
                "BEGIN",
                "SELECT count(*) INTO _count FROM my_test;",
                "RETURN _count;",
                "END;",
                "$function$;",
            })));
            Assert.IsTrue(db.TryRun(string.Join(" ", new string[] {
                "CREATE OR REPLACE FUNCTION public.count_test(_multiply int)",
                "RETURNS integer",
                "LANGUAGE plpgsql",
                "AS $function$",
                "DECLARE _count INT;",
                "BEGIN",
                "SELECT count(*) INTO _count FROM my_test;",
                "RETURN _count * _multiply;",
                "END;",
                "$function$;",
            })));
            Assert.AreEqual(2, db.Fn<int>("count_test"));
            Assert.AreEqual(4, db.Fn<int>("count_test", new { _multiply = 2 }));

            // function returning table
            Assert.IsTrue(db.TryRun(string.Join(" ", new string[] {
                "CREATE OR REPLACE FUNCTION public.result_test()",
                "RETURNS TABLE(a varchar, b int)",
                "LANGUAGE plpgsql",
                "AS $function$",
                "BEGIN",
                "RETURN QUERY SELECT foo, bar FROM my_test ORDER BY foo;",
                "END;",
                "$function$;",
            })));

            var fnRows = db.FnResult("result_test");
            var fnRow = db.FnFirst("result_test");

            Assert.AreEqual(2, fnRows.Count);
            Assert.AreEqual("a", fnRows[0]["a"]);
            Assert.AreEqual(1, fnRows[0]["b"]);
            Assert.AreEqual("a", fnRow["a"]);
            Assert.AreEqual(1, fnRow["b"]);

            // manual function fetch
            var readerRow2 = db.Fn("result_test");
            var readerStr2 = "";

            while (readerRow2.Read())
            {
                readerStr2 += readerRow2["a"].ToString();
            }

            readerRow2.Close();

            Assert.AreEqual("ab", readerStr2);

            // manual query fetch
            var readerRow3 = db.Query("select * from my_test");
            var readerStr3 = "";

            while (readerRow3.Read())
            {
                readerStr3 += readerRow3["foo"].ToString();
            }

            readerRow3.Close();

            Assert.AreEqual("ab", readerStr3);

            // raw query
            Assert.AreEqual(1, db.Query<int>("update my_test set bar = 55 where foo = 'a'", null, false));
            Assert.AreEqual(55, db.Query<int>("select bar from my_test where foo = 'a'", null, true));
            Assert.AreEqual(55, db.Query<int>("select bar from my_test where foo = $1", new object[] { "a" }, true));

            // getting result
            var resultFound = db.Result("select * from my_test", CommandType.Text);
            var resultFirst = db.ResultFirst("select * from my_test order by foo", CommandType.Text);

            Assert.AreEqual(2, resultFound.Count);
            Assert.IsNotNull(resultFirst);
            Assert.AreEqual("a", resultFirst["foo"]);

            // truncate
            Assert.IsTrue(db.Truncate("my_test", true));
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

            // error check
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                db.Insert("expected_sp", dt);
            });
            Assert.ThrowsException<PostgresException>(() =>
            {
                var dte = dt;

                dte.Rows.Add("c", null);

                db.Insert(dte);
            });
            Assert.IsFalse(db.TryRun("select * from non_exist_table"));

            // truncate shortcut
            Assert.IsTrue(db.Truncate("my_test"));
            Assert.AreEqual(0, db.Count("my_test"));

            // ensure nothing left
            Assert.IsTrue(db.Drop("my_test"));
            Assert.IsTrue(db.Drop("my_test", true));
            Assert.IsFalse(db.Exists("my_test"));
        }

        [TestMethod]
        public void PgsqlSelectLimited()
        {
            var qb = GetPgsql().Qb().From("test").Where("id", 1, "<>", "AND").OrWhere("idx", 2).AndWhere("name", "foo").AndWhere("remark", "0", "<>").OrWhere("remark", "1", "=").Limit(10, 5);
            var sql = "SELECT * FROM test WHERE id <> $1 OR idx = $2 AND name = $3 AND remark <> $4 OR remark = $5 LIMIT 10 OFFSET 5";

            Assert.AreEqual(sql, qb.Sql);
            Assert.AreEqual(5, qb.Params.Count);
            Assert.AreEqual(1, qb.Params["$1"]);
            Assert.AreEqual(2, qb.Params["$2"]);
            Assert.AreEqual("foo", qb.Params["$3"]);
            Assert.AreEqual("0", qb.Params["$4"]);
            Assert.AreEqual("1", qb.Params["$5"]);
        }

        [TestMethod]
        public void PgsqlDTCopy()
        {
            var dt = new DataTable("test");

            dt.Columns.Add("foo", typeof(string));
            dt.Columns.Add("bar", typeof(int));
            dt.Columns.Add("baz", typeof(decimal));
            dt.Columns.Add("qux", typeof(DateTime));

            var copy = new DTCopy(dt);
            var sql = "COPY test (foo, bar, baz, qux) FROM STDIN (FORMAT BINARY)";

            Assert.AreEqual(sql, copy.Cmd);
            Assert.AreEqual(4, copy.Count);
            Assert.AreEqual(4, copy.Columns.Count);
            Assert.AreEqual(NpgsqlDbType.Varchar, copy.Columns["foo"]);
            Assert.AreEqual(NpgsqlDbType.Integer, copy.Columns["bar"]);
            Assert.AreEqual(NpgsqlDbType.Numeric, copy.Columns["baz"]);
            Assert.AreEqual(NpgsqlDbType.Timestamp, copy.Columns["qux"]);
        }

        [TestMethod]
        public void InvalidConnection()
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
        public void GettingConnection()
        {
            var db = GetPgsql();

            Assert.AreSame(db.Connection, db.Connection);
        }

        private Db GetPgsql()
        {
            return new Beasier().GetPgsql("PGCON");
        }

        private Db GetMssql()
        {
            return new Beasier().GetMssql("MSCON");
        }
    }
}
