﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.OrmLite.Async;
using ServiceStack.OrmLite.Tests.Shared;

namespace ServiceStack.OrmLite.Tests
{
    [Explicit("Requires SqlServer")]
    public class ApiSqlServerTestsAsync
        : OrmLiteTestBase
    {
        private IDbConnection db;

        [SetUp]
        public void SetUp()
        {
            SuppressIfOracle("SQL Server tests");
            db = CreateSqlServerDbFactory().OpenDbConnection();
            db.DropAndCreateTable<Person>();
            db.DropAndCreateTable<PersonWithAutoId>();
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        [Test]
        public async Task API_SqlServer_Examples_Async()
        {
            await db.InsertAsync(Person.Rockstars);

            await db.SelectAsync<Person>(x => x.Age > 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            await db.SelectAsync<Person>(q => q.Where(x => x.Age > 40).OrderBy(x => x.Id));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)\nORDER BY \"Id\" ASC"));

            await db.SelectAsync<Person>(q => q.Where(x => x.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            await db.SelectAsync(db.From<Person>().Where(x => x.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            await db.SingleAsync<Person>(x => x.Age == 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT TOP 1 \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" = 42)"));

            await db.SingleAsync<Person>(q => q.Where(x => x.Age == 42));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT TOP 1 \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" = 42)"));

            await db.SingleAsync(db.From<Person>().Where(x => x.Age == 42));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT TOP 1 \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" = 42)"));

            await db.ScalarAsync<Person, int>(x => Sql.Max(x.Age));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Max(\"Age\") \nFROM \"Person\""));

            await db.ScalarAsync<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Max(\"Age\") \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            await db.CountAsync<Person>(x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            await db.CountAsync(db.From<Person>().Where(x => x.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));


            await db.SelectAsync<Person>("Age > 40");
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > 40"));

            await db.SelectAsync<Person>("SELECT * FROM Person WHERE Age > 40");
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age > 40"));

            await db.SelectAsync<Person>("Age > @age", new { age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > @age"));

            await db.SelectAsync<Person>("SELECT * FROM Person WHERE Age > @age", new { age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age > @age"));

            await db.SelectAsync<Person>("Age > @age", new Dictionary<string, object> { { "age", 40 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > @age"));

            await db.SelectAsync<Person>("SELECT * FROM Person WHERE Age > @age", new Dictionary<string, object> { { "age", 40 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age > @age"));

            await db.SelectFmtAsync<Person>("Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > 40"));

            await db.SelectFmtAsync<Person>("SELECT * FROM Person WHERE Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age > 40"));

            await db.SelectAsync<EntityWithId>(typeof(Person));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\" FROM \"Person\""));

            await db.SelectFmtAsync<EntityWithId>(typeof(Person), "Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\" FROM \"Person\" WHERE Age > 40"));

            await db.WhereAsync<Person>("Age", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            await db.WhereAsync<Person>(new { Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            await db.SelectByIdsAsync<Person>(new[] { 1, 2, 3 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Id\" IN (1,2,3)"));

            await db.SelectNonDefaultsAsync(new Person { Id = 1 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Id\" = @Id"));

            await db.SelectNonDefaultsAsync("Age > @Age", new Person { Age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > @Age"));

            //await db.SelectLazy<Person>().ToList();
            //Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\""));

            //db.SelectLazy<Person>("Age > @age", new { age = 40 }).ToList();
            //Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > @age"));

            //db.SelectLazyFmt<Person>("Age > {0}", 40).ToList();
            //Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > 40"));

            //db.WhereLazy<Person>(new { Age = 27 }).ToList();
            //Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            await db.SingleByIdAsync<Person>(1);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Id\" = @Id"));

            await db.SingleAsync<Person>(new { Age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            await db.SingleAsync<Person>("Age = @age", new { age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age = @age"));

            await db.SingleFmtAsync<Person>("Age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age = 42"));

            await db.SingleByIdAsync<Person>(1);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Id\" = @Id"));

            await db.SingleWhereAsync<Person>("Age", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            await db.ScalarAsync<int>(db.From<Person>().Select(Sql.Count("*")).Where(q => q.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));
            await db.ScalarAsync<int>(db.From<Person>().Select(x => Sql.Count("*")).Where(q => q.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Count(*) \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            await db.ScalarAsync<int>("SELECT COUNT(*) FROM Person WHERE Age > @age", new { age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age > @age"));

            await db.ScalarFmtAsync<int>("SELECT COUNT(*) FROM Person WHERE Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age > 40"));

            await db.ColumnAsync<string>(db.From<Person>().Select(x => x.LastName).Where(q => q.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"LastName\" \nFROM \"Person\"\nWHERE (\"Age\" = 27)"));

            await db.ColumnAsync<string>("SELECT LastName FROM Person WHERE Age = @age", new { age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age = @age"));

            await db.ColumnFmtAsync<string>("SELECT LastName FROM Person WHERE Age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age = 27"));

            await db.ColumnDistinctAsync<int>(db.From<Person>().Select(x => x.Age).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            await db.ColumnDistinctAsync<int>("SELECT Age FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age FROM Person WHERE Age < @age"));

            await db.ColumnDistinctFmtAsync<int>("SELECT Age FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age FROM Person WHERE Age < 50"));

            await db.LookupAsync<int, string>(db.From<Person>().Select(x => new { x.Age, x.LastName }).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Age\",\"LastName\" \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            await db.LookupAsync<int, string>("SELECT Age, LastName FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age, LastName FROM Person WHERE Age < @age"));

            await db.LookupFmtAsync<int, string>("SELECT Age, LastName FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age, LastName FROM Person WHERE Age < 50"));

            await db.DictionaryAsync<int, string>(db.From<Person>().Select(x => new { x.Id, x.LastName }).Where(x => x.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\",\"LastName\" \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            await db.DictionaryAsync<int, string>("SELECT Id, LastName FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Id, LastName FROM Person WHERE Age < @age"));

            await db.DictionaryFmtAsync<int, string>("SELECT Id, LastName FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Id, LastName FROM Person WHERE Age < 50"));

            await db.ExistsAsync<Person>(x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            await db.ExistsAsync(db.From<Person>().Where(x => x.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            await db.ExistsAsync<Person>(new { Age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            await db.ExistsAsync<Person>("Age = @age", new { age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age = @age"));
            await db.ExistsAsync<Person>("SELECT * FROM Person WHERE Age = @age", new { age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age = @age"));

            await db.ExistsFmtAsync<Person>("Age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age = 42"));
            await db.ExistsFmtAsync<Person>("SELECT * FROM Person WHERE Age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age = 42"));

            await db.SqlListAsync<Person>(db.From<Person>().Select("*").Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            await db.SqlListAsync<Person>("SELECT * FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age < @age"));

            await db.SqlListAsync<Person>("SELECT * FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age < @age"));

            await db.SqlColumnAsync<string>(db.From<Person>().Select(x => x.LastName).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"LastName\" \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            await db.SqlColumnAsync<string>("SELECT LastName FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age < @age"));

            await db.SqlColumnAsync<string>("SELECT LastName FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age < @age"));

            await db.SqlScalarAsync<int>(db.From<Person>().Select(Sql.Count("*")).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            await db.SqlScalarAsync<int>("SELECT COUNT(*) FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age < @age"));

            await db.SqlScalarAsync<int>("SELECT COUNT(*) FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age < @age"));

            var rowsAffected = await db.ExecuteNonQueryAsync("UPDATE Person SET LastName={0} WHERE Id={1}".SqlFmt("WaterHouse", 7));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE Person SET LastName='WaterHouse' WHERE Id=7"));

            rowsAffected = await db.ExecuteNonQueryAsync("UPDATE Person SET LastName=@name WHERE Id=@id", new { name = "WaterHouse", id = 7 });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE Person SET LastName=@name WHERE Id=@id"));


            await db.InsertAsync(new Person { Id = 7, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"Person\" (\"Id\",\"FirstName\",\"LastName\",\"Age\") VALUES (@Id,@FirstName,@LastName,@Age)"));

            await db.InsertAsync(new Person { Id = 8, FirstName = "Tupac", LastName = "Shakur", Age = 25 },
                      new Person { Id = 9, FirstName = "Tupac", LastName = "Shakur2", Age = 26 });

            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"Person\" (\"Id\",\"FirstName\",\"LastName\",\"Age\") VALUES (@Id,@FirstName,@LastName,@Age)"));


            await db.InsertAllAsync(new[] { new Person { Id = 10, FirstName = "Biggie", LastName = "Smalls", Age = 24 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"Person\" (\"Id\",\"FirstName\",\"LastName\",\"Age\") VALUES (@Id,@FirstName,@LastName,@Age)"));

            await db.InsertOnlyAsync(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, q => q.Insert(p => new { p.FirstName, p.Age }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"PersonWithAutoId\" (\"FirstName\",\"Age\") VALUES ('Amy',27)"));

            await db.InsertOnlyAsync(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, q => db.From<PersonWithAutoId>().Insert(p => new { p.FirstName, p.Age }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"PersonWithAutoId\" (\"FirstName\",\"Age\") VALUES ('Amy',27)"));

            await db.UpdateAsync(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"=@FirstName, \"LastName\"=@LastName, \"Age\"=@Age WHERE \"Id\"=@Id"));

            await db.UpdateAsync(new Person { Id = 8, FirstName = "Tupac", LastName = "Shakur3", Age = 27 },
                      new Person { Id = 9, FirstName = "Tupac", LastName = "Shakur4", Age = 28 });

            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"=@FirstName, \"LastName\"=@LastName, \"Age\"=@Age WHERE \"Id\"=@Id"));

            await db.UpdateAsync(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"=@FirstName, \"LastName\"=@LastName, \"Age\"=@Age WHERE \"Id\"=@Id"));

            await db.UpdateAllAsync(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"=@FirstName, \"LastName\"=@LastName, \"Age\"=@Age WHERE \"Id\"=@Id"));

            await db.UpdateAsync(new Person { Id = 1, FirstName = "JJ", Age = 27 }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"Id\"=1, \"FirstName\"='JJ', \"LastName\"=NULL, \"Age\"=27 WHERE (\"LastName\" = 'Hendrix')"));

            await db.UpdateAsync<Person>(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ' WHERE (\"LastName\" = 'Hendrix')"));

            await db.UpdateNonDefaultsAsync(new Person { FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ' WHERE (\"LastName\" = 'Hendrix')"));

            await db.UpdateOnlyAsync(new Person { FirstName = "JJ" }, p => p.FirstName);
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ'"));

            await db.UpdateOnlyAsync(new Person { FirstName = "JJ" }, p => p.FirstName, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ' WHERE (\"LastName\" = 'Hendrix')"));

            await db.UpdateOnlyAsync(new Person { FirstName = "JJ", LastName = "Hendo" }, q => q.Update(p => p.FirstName));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ'"));

            await db.UpdateOnlyAsync(new Person { FirstName = "JJ" }, q => q.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ' WHERE (\"FirstName\" = 'Jimi')"));

            await db.UpdateFmtAsync<Person>(set: "FirstName = {0}".SqlFmt("JJ"), where: "LastName = {0}".SqlFmt("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));

            await db.UpdateFmtAsync(table: "Person", set: "FirstName = {0}".SqlFmt("JJ"), where: "LastName = {0}".SqlFmt("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));

            await db.DeleteAsync<Person>(new { FirstName = "Jimi", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"FirstName\"=@FirstName AND \"Age\"=@Age"));

            await db.DeleteAsync<Person>(new { FirstName = "Jimi", Age = 27 },
                              new { FirstName = "Janis", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"FirstName\"=@FirstName AND \"Age\"=@Age"));

            await db.DeleteAsync(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"Id\"=@Id AND \"FirstName\"=@FirstName AND \"LastName\"=@LastName AND \"Age\"=@Age"));

            await db.DeleteNonDefaultsAsync(new Person { FirstName = "Jimi", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"FirstName\"=@FirstName AND \"Age\"=@Age"));

            await db.DeleteNonDefaultsAsync(new Person { FirstName = "Jimi", Age = 27 },
                                 new Person { FirstName = "Janis", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"FirstName\"=@FirstName AND \"Age\"=@Age"));

            await db.DeleteByIdAsync<Person>(1);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"Id\" = @0"));

            await db.DeleteByIdsAsync<Person>(new[] { 1, 2, 3 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"Id\" IN (1,2,3)"));

            await db.DeleteFmtAsync<Person>("Age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));

            await db.DeleteFmtAsync(typeof(Person), "Age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));

            await db.DeleteAsync<Person>(p => p.Age == 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));

            await db.DeleteAsync<Person>(q => q.Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));

            await db.DeleteAsync(db.From<Person>().Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));

            await db.DeleteFmtAsync<Person>(where: "Age = {0}".SqlFmt(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));

            await db.DeleteFmtAsync(table: "Person", where: "Age = {0}".SqlFmt(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));

            //db.Save(new Person { Id = 11, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
            //Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"Person\" (\"Id\",\"FirstName\",\"LastName\",\"Age\") VALUES (@Id,@FirstName,@LastName,@Age)"));
            //db.Save(new Person { Id = 11, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
            //Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"=@FirstName, \"LastName\"=@LastName, \"Age\"=@Age WHERE \"Id\"=@Id"));

            //db.Save(new Person { Id = 12, FirstName = "Amy", LastName = "Winehouse", Age = 27 },
            //        new Person { Id = 13, FirstName = "Amy", LastName = "Winehouse", Age = 27 });

            //db.SaveAll(new[]{ new Person { Id = 14, FirstName = "Amy", LastName = "Winehouse", Age = 27 },
            //                  new Person { Id = 15, FirstName = "Amy", LastName = "Winehouse", Age = 27 } });
        }

    }
}