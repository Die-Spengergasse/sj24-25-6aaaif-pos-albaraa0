using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Linq;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    public class PaymentService
    {
        private readonly AppointmentContext _db;

        public PaymentService(AppointmentContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IQueryable<PaymentItem> PaymentItems => _db.PaymentItems.AsQueryable();
        public IQueryable<Payment> Payments => _db.Payments.AsQueryable();

        public Payment CreatePayment(NewPaymentCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException(nameof(cmd));

            var currentTime = DateTime.UtcNow;

            var desk = _db.CashDesks.FirstOrDefault(d => d.Number == cmd.CashDeskNumber)
                ?? throw new ArgumentException("Invalid cash desk number.", nameof(cmd.CashDeskNumber));

            var staff = _db.Employees.FirstOrDefault(e => e.RegistrationNumber == cmd.EmployeeRegistrationNumber)
                ?? throw new ArgumentException("Invalid employee registration number.", nameof(cmd.EmployeeRegistrationNumber));

            if (_db.Payments.Any(p => p.CashDesk.Number == cmd.CashDeskNumber && p.Confirmed == null))
            {
                throw new PaymentServiceException("An open payment already exists for this cash desk.");
            }

            if (!Enum.TryParse<PaymentType>(cmd.PaymentType, out var paymentType))
            {
                throw new ArgumentException("Invalid payment type.", nameof(cmd.PaymentType));
            }

            if (paymentType == PaymentType.CreditCard && !_db.Managers.Any(m => m.RegistrationNumber == cmd.EmployeeRegistrationNumber))
            {
                throw new PaymentServiceException("Only managers can create credit card payments.");
            }

            var payment = new Payment(desk, currentTime, staff, paymentType);
            _db.Payments.Add(payment);
            SaveChanges();
            return payment;
        }

        public void ConfirmPayment(int paymentId)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.Id == paymentId)
                ?? throw new ArgumentException("Payment not found.", nameof(paymentId));

            if (payment.Confirmed.HasValue)
            {
                throw new ArgumentException("Payment is already confirmed.", nameof(paymentId));
            }

            payment.Confirmed = DateTime.UtcNow;
            SaveChanges();
        }

        public void AddPaymentItem(NewPaymentItemCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException(nameof(cmd));

            var payment = _db.Payments.FirstOrDefault(p => p.Id == cmd.Payment.Id)
                ?? throw new PaymentServiceException("Payment not found.");

            if (payment.Confirmed.HasValue)
            {
                throw new PaymentServiceException("Cannot add items to a confirmed payment.");
            }

            var paymentItem = new PaymentItem(cmd.ArticleName, cmd.Amount, cmd.Price, cmd.Payment);
            _db.PaymentItems.Add(paymentItem);
            SaveChanges();
        }

        public void DeletePayment(int paymentId, bool deleteItems)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment == null) return;

            if (deleteItems)
            {
                var paymentItems = _db.PaymentItems.Where(p => p.Payment.Id == paymentId).ToList();
                if (paymentItems.Any())
                {
                    _db.PaymentItems.RemoveRange(paymentItems);
                }
            }

            _db.Payments.Remove(payment);
            SaveChanges();
        }

        private void SaveChanges()
        {
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new PaymentServiceException(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}