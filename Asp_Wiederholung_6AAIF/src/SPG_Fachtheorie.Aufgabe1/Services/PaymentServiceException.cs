using System;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    public class PaymentServiceException : Exception
    {
        public PaymentServiceException()
        {
        }

        public PaymentServiceException(string? message) : base(message)
        {
        }

        public PaymentServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}