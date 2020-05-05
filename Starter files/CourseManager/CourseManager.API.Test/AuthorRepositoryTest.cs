using CourseManager.API.DbContexts;
using CourseManager.API.Entities;
using CourseManager.API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace CourseManager.API.Test
{
    public class AuthorRepositoryTest
    {
        private readonly ITestOutputHelper _ouput;

        public AuthorRepositoryTest(ITestOutputHelper ouput)
        {
            _ouput = ouput;
        }

        [Fact]
        public void GetAuthors_PageSIzeIsThree_ReturnsThreeAuthors()
        {
            // Arrange
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());
            var options = new DbContextOptionsBuilder<CourseContext>().UseSqlite(connection).Options;

            using (var ctx = new CourseContext(options))
            {
                ctx.Database.OpenConnection();
                ctx.Database.EnsureCreated();

                ICollection<Country> countriesLst = new List<Country>
                {
                    new Country{ Id="BE", Description="Belgium"},
                    new Country{ Id="US", Description="United States of America"}
                };

                ctx.Countries.AddRange(countriesLst);

                ICollection<Author> authorsLst = new List<Author>
                {
                    new Author{CountryId="BE", FirstName="Kevin", LastName="Dockx"},
                    new Author{CountryId="BE", FirstName="Gill", LastName="Cleeren"},
                    new Author{CountryId="US", FirstName="Julie", LastName="Leerman"},
                    new Author{CountryId="BE", FirstName="Shawn", LastName="Wildermuth"},
                    new Author{CountryId="US", FirstName="Devorah", LastName="Kurata"}
                };

                ctx.Authors.AddRange(authorsLst);

                ctx.SaveChanges();

                var authorRepository = new AuthorRepository(ctx);


                // ACT
                var authors = authorRepository.GetAuthors(1, 3);

                // ASSERT
                Assert.Equal(3, authors.Count());
            }
        }

        [Fact]
        public void GetAuthor_EmptyGuid_ThrowsArgumentException()
        {
            // ARRANGE
            //var options = new DbContextOptionsBuilder<CourseContext>().UseInMemoryDatabase($"CourseDatabaseForTesting{Guid.NewGuid()}").Options;
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());
            var options = new DbContextOptionsBuilder<CourseContext>().UseSqlite(connection).Options;

            using (var ctx = new CourseContext(options))
            {
                ctx.Database.OpenConnection();
                ctx.Database.EnsureCreated();

                var authorRepository = new AuthorRepository(ctx);

                Assert.Throws<ArgumentException>(() =>
                {
                    authorRepository.GetAuthor(Guid.Empty);
                });
            }
        }

        [Fact]
        public void AddAuthor_AuthorWithoutCountryId_AuthorHasBEAsCountryId()
        {
            // ARRANGE
            var logger = new LogToActionLoggerProvider((log) => _ouput.WriteLine(log));
            var loggerFactory = new LoggerFactory(new[] { logger });

            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());
            var options = new DbContextOptionsBuilder<CourseContext>()
                .UseLoggerFactory(loggerFactory)
                .UseSqlite(connection)
                .Options;

            using (var ctx = new CourseContext(options))
            {
                ctx.Database.OpenConnection();
                ctx.Database.EnsureCreated();

                ICollection<Country> countriesLst = new List<Country>
                {
                    new Country{ Id="BE", Description="Belgium"}
                };

                ctx.Countries.AddRange(countriesLst);

                ctx.SaveChanges();

                var authorRepository = new AuthorRepository(ctx);

                var authorToAdd = new Author
                {
                    FirstName = "adonis",
                    LastName = "cruz v",
                    Id = Guid.Parse("ec622423-d92b-430b-a4b9-b14caafdda6c")
                };

                // ACT
                authorRepository.AddAuthor(authorToAdd);
                authorRepository.SaveChanges();
            }

            using (var ctx = new CourseContext(options))
            {
                ctx.Database.OpenConnection();
                ctx.Database.EnsureCreated();

                // ASSERT
                var authorRepository = new AuthorRepository(ctx);
                var addedAuthor = authorRepository.GetAuthor(Guid.Parse("ec622423-d92b-430b-a4b9-b14caafdda6c"));
                Assert.Equal("BE", addedAuthor.CountryId);
            }
        }
    }
}
