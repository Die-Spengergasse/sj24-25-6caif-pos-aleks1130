using Spg.Fachtheorie.Aufgabe3.API.Test;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe3.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SPG_Fachtheorie.Aufgabe3.Test
{
    public class EmployeesControllerTests
    {
        [Theory]
        [InlineData("manager", 1)]  // GET /api/employees?type=manager
        [InlineData("cashier", 2)]  // GET /api/employees?type=cashier
        public async Task GetAllEmployeesSuccessTest(
            string type, int expectedRegistrationNumber)
        {
            // ARRANGE
            var factory = new TestWebApplicationFactory();
            factory.InitializeDatabase(db =>
            {
                var manager = new Manager(
                    1, "FN", "LN", new DateOnly(2004, 2, 1),
                    3000M, null, "SUV");
                var cashier = new Cashier(
                    2, "FN", "LN", new DateOnly(2004, 2, 1),
                    3000M, null, "Feinkost");
                db.AddRange(cashier, manager);
                db.SaveChanges();
            });

            // ACT
            var (statusCode, content) = await factory.GetHttpContent<List<EmployeeDto>>($"/api/employees?type={type}");

            // ASSERT
            Assert.True(statusCode == System.Net.HttpStatusCode.OK);
            Assert.NotNull(content);
            Assert.True(content.First().RegistrationNumber == expectedRegistrationNumber);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]
        [InlineData(2, HttpStatusCode.NotFound)]
        public async Task GetEmployeeTest(int registrationNumber, HttpStatusCode expectedStatusCode)
        {
            // ARRANGE
            var factory = new TestWebApplicationFactory();
            factory.InitializeDatabase(db =>
            {
                var manager = new Manager(
                    1, "FN", "LN", new DateOnly(2004, 2, 1),
                    3000M, null, "SUV");
                db.AddRange(manager);
                db.SaveChanges();
            });

            // ACT
            var (statusCode, content) = await factory.GetHttpContent<EmployeeDetailDto>($"/api/employees/{registrationNumber}");

            // ASSERT
            Assert.True(statusCode == expectedStatusCode);
            if (expectedStatusCode == HttpStatusCode.OK)
            {
                Assert.NotNull(content);
                Assert.True(content.RegistrationNumber == registrationNumber);
            }
        }

        [Theory]
        [InlineData(999, HttpStatusCode.BadRequest)]
        [InlineData(1000, HttpStatusCode.Created)]
        public async Task AddManagerTest(int registrationNumber, HttpStatusCode expectedStatusCode)
        {
            // ARRANGE
            var factory = new TestWebApplicationFactory();
            factory.InitializeDatabase(db =>
            {
            });
            var cmd = new NewManagerCommand(
                registrationNumber, "FN", "LN", new DateOnly(2000, 1, 1), 3000M,
                null, "SUV");

            // ACT
            var (statusCode, jsonElement) = await factory.PostHttpContent(
                "/api/employees/manager", cmd);

            // ASSERT
            Assert.True(statusCode == expectedStatusCode);
            if (statusCode == HttpStatusCode.Created)
            {
                Assert.True(jsonElement.GetProperty("registrationNumber").GetInt32() == 1000);
            }
        }
    }
}
