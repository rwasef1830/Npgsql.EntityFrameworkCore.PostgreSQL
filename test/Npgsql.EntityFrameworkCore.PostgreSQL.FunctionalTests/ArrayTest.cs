using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class ArrayTest : IClassFixture<ArrayFixture>
    {
        ArrayFixture Fixture { get; }

        public ArrayTest(ArrayFixture fixture)
        {
            Fixture = fixture;
        }

        ArrayContext CreateContext() => Fixture.CreateContext();

        [Fact]
        public void Roundtrip()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single();
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
            }
        }

        [Fact]
        public void Index_with_constant()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeArray[0] == 3).ToList();
                Assert.Equal(1, actual.Count);
                Assert.Contains(@"WHERE (""e"".""SomeArray""[1]) = 3", Sql);
            }
        }

        [Fact]
        public void Index_with_non_constant()
        {
            using (var ctx = CreateContext())
            {
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeArray[x] == 3).ToList();
                Assert.Equal(1, actual.Count);
                Assert.Contains(@"WHERE (""e"".""SomeArray""[(@__x_0)+1]) = 3", Sql);
            }
        }

        [Fact]
        public void Literal()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void SequenceEqual_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                var arr = new[] { 3, 4 };
                var x = ctx.SomeEntities.Single(e => e.SomeArray.SequenceEqual(arr));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                Assert.Contains(@"WHERE ""e"".""SomeArray"" = @", Sql);
                
            }
        }

        [Fact]
        public void SequenceEqual_with_array_initializer()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.SequenceEqual(new[] { 3, 4 }));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                Assert.Contains(@"WHERE ""e"".""SomeArray"" = $1", Sql);

            }
        }

        [Fact]
        public void Length()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Length == 2);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                Assert.Contains(@"WHERE array_length(""e"".""SomeArray"", 1) = 2", Sql);
            }
        }

        const string FileLineEnding = @"
";

        static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }

    public class ArrayContext : DbContext
    {
        public DbSet<SomeEntity> SomeEntities { get; set; }
        public ArrayContext(DbContextOptions options) : base(options) {}
        protected override void OnModelCreating(ModelBuilder builder)
        {

        }
    }

    public class SomeEntity
    {
        public int Id { get; set; }
        public int[] SomeArray { get; set; }
    }

    public class ArrayFixture : IDisposable
    {
        readonly DbContextOptions _options;
        private readonly TestSqlLoggerFactory _testSqlLoggerFactory = new TestSqlLoggerFactory();

        public ArrayFixture()
        {
            _testStore = NpgsqlTestStore.CreateScratch();
            _options = new DbContextOptionsBuilder()
                .UseNpgsql(_testStore.Connection, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkNpgsql()
                        .AddSingleton<ILoggerFactory>(_testSqlLoggerFactory)
                        .BuildServiceProvider())
                .Options;

            using (var ctx = CreateContext())
            {
                ctx.Database.EnsureCreated();
                ctx.SomeEntities.Add(new SomeEntity { SomeArray = new[] { 3, 4 } });
                ctx.SomeEntities.Add(new SomeEntity { SomeArray = new[] { 5, 6, 7 } });
                ctx.SaveChanges();
            }
        }

        readonly NpgsqlTestStore _testStore;
        public ArrayContext CreateContext() => new ArrayContext(_options);
        public void Dispose() => _testStore.Dispose();
    }
}
