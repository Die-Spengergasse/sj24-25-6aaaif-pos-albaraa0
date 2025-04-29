using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test
{
    public class PaymentServiceTests
    {
        private AppointmentContext GetEmptyDbContext()
        {
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppointmentContext(options);
        }

        [Theory]
        [InlineData(null, "Invalid cash desk number.")]
        [InlineData("InvalidDesk", "Invalid cash desk number.")]
        [InlineData("ValidDesk", "Invalid employee registration number.")]
        public void CreatePaymentExceptionsTest(string cashDeskNumber, string expectedMessage)
        {
            // Arrange
            var dbContext = GetEmptyDbContext();
            var service = new PaymentService(dbContext);
            var command = new NewPaymentCommand
            {
                CashDeskNumber = cashDeskNumber,
                EmployeeRegistrationNumber = "InvalidEmployee",
                PaymentType = "Cash"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => service.CreatePayment(command));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void CreatePaymentSuccessTest()
        {
            // Arrange
            var dbContext = GetEmptyDbContext();
            var service = new PaymentService(dbContext);

            var cashDesk = new CashDesk { Number = "ValidDesk" };
            var employee = new Employee { RegistrationNumber = "ValidEmployee" };
            dbContext.CashDesks.Add(cashDesk);
            dbContext.Employees.Add(employee);
            dbContext.SaveChanges();

            var command = new NewPaymentCommand
            {
                CashDeskNumber = "ValidDesk",
                EmployeeRegistrationNumber = "ValidEmployee",
                PaymentType = "Cash"
            };

            // Act
            var payment = service.CreatePayment(command);

            // Assert
            Assert.NotNull(payment);
            Assert.Equal("ValidDesk", payment.CashDesk.Number);
            Assert.Equal("ValidEmployee", payment.Employee.RegistrationNumber);
        }

        [Fact]
        public void ConfirmPaymentSuccessTest()
        {
            // Arrange
            var dbContext = GetEmptyDbContext();
            var service = new PaymentService(dbContext);

            var cashDesk = new CashDesk { Number = "ValidDesk" };
            var employee = new Employee { RegistrationNumber = "ValidEmployee" };
            var payment = new Payment(cashDesk, DateTime.UtcNow, employee, PaymentType.Cash);
            dbContext.Payments.Add(payment);
            dbContext.SaveChanges();

            // Act
            service.ConfirmPayment(payment.Id);

            // Assert
            var confirmedPayment = dbContext.Payments.First(p => p.Id == payment.Id);
            Assert.NotNull(confirmedPayment.Confirmed);
        }

        [Fact]
        public void AddPaymentItemSuccessTest()
        {
            // Arrange
            var dbContext = GetEmptyDbContext();
            var service = new PaymentService(dbContext);

            var cashDesk = new CashDesk { Number = "ValidDesk" };
            var employee = new Employee { RegistrationNumber = "ValidEmployee" };
            var payment = new Payment(cashDesk, DateTime.UtcNow, employee, PaymentType.Cash);
            dbContext.Payments.Add(payment);
            dbContext.SaveChanges();

            var command = new NewPaymentItemCommand
            {
                ArticleName = "TestItem",
                Amount = 1,
                Price = 10.0m,
                Payment = payment
            };

            // Act
            service.AddPaymentItem(command);

            // Assert
            var paymentItem = dbContext.PaymentItems.First();
            Assert.Equal("TestItem", paymentItem.ArticleName);
            Assert.Equal(1, paymentItem.Amount);
            Assert.Equal(10.0m, paymentItem.Price);
        }

        [Fact]
        public void DeletePaymentSuccessTest()
        {
            // Arrange
            var dbContext = GetEmptyDbContext();
            var service = new PaymentService(dbContext);

            var cashDesk = new CashDesk { Number = "ValidDesk" };
            var employee = new Employee { RegistrationNumber = "ValidEmployee" };
            var payment = new Payment(cashDesk, DateTime.UtcNow, employee, PaymentType.Cash);
            var paymentItem = new PaymentItem("TestItem", 1, 10.0m, payment);
            dbContext.Payments.Add(payment);
            dbContext.PaymentItems.Add(paymentItem);
            dbContext.SaveChanges();

            // Act
            service.DeletePayment(payment.Id, true);

            // Assert
            Assert.Empty(dbContext.Payments);
            Assert.Empty(dbContext.PaymentItems);
        }
    }
}