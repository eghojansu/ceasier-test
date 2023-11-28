using Ceasier.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using NpgsqlTypes;
using Ceasier;
using Npgsql;
using System.Collections.Generic;

namespace CeasierTests.Db
{
    [TestClass]
    public class PgsqlTest
    {
        [TestMethod]
        public void SummarizedTest()
        {
            var db = GetDb();

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

            // call function
            Assert.IsTrue(db.TryQuery(string.Join(" ", new string[] {
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
            Assert.IsTrue(db.TryQuery(string.Join(" ", new string[] {
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
            Assert.AreEqual(2, db.FnRun<int>("count_test"));
            Assert.AreEqual(4, db.FnRun<int>("count_test", new { _multiply = 2 }));

            // function returning table
            Assert.IsTrue(db.TryQuery(string.Join(" ", new string[] {
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

            // raw query
            Assert.AreEqual(1, db.Query<int>("update my_test set bar = 55 where foo = 'a'"));
            Assert.AreEqual(55, db.Query<int>("select bar from my_test where foo = 'a'", true));

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
            Assert.ThrowsException<Exception>(() =>
            {
                db.Insert("expected_sp", dt);
            });

            Assert.ThrowsException<PostgresException>(() =>
            {
                var dte = dt;

                dte.Rows.Add("c", null);

                db.Insert(dte);
            });

            // ensure nothing left
            Assert.IsTrue(db.Drop("my_test", true));
            Assert.IsFalse(db.Exists("my_test"));
        }

        [TestMethod]
        public void Select()
        {
            var qb = GetDb().Qb().From("test").Where("id", 1, "<>", "AND").OrWhere("idx", 2).AndWhere("name", "foo").AndWhere("remark", "0", "<>").OrWhere("remark", "1", "=").Limit(10, 5);
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
        public void DTCopy()
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

        private Pgsql GetDb()
        {
            return new Beasier().GetPgsql("PGCON");
        }
    }
}
