using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Payment.DataAccess.PaymentChronicles.PaymentChronicle;
using COPA.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COPAv2.BLL.Logging
{
    public class TestCOPALogger : ICOPALogger
    {
        public PaymentChronicle ConvertToPaymentChronicle(Payment payment, PaymentStep paymentStep, Exception e = null) =>
            Mock.Of<PaymentChronicle>();

        public void LogException(Exception exception, PaymentStep actualStep, bool isCardNumberVisible = true)
        {
            logger.Log(LogLevel.Trace, $"\n\tLastStep: {actualStep}" +
                                        $"\n\tException: {exception.Message}" +
                                        "\n\tException Stack Trace:\n" + $"{ exception.StackTrace }");
        }

        public void RecordSuccessfulTransaction(ILogger logger, Payment payment)
        {
            logger.Log(LogLevel.Trace, $"PaymentInvoiceNumber: {payment.PaymentInvoiceNumber}" +
                                    $"\n\tProvider: { payment.Website.ProviderName}" +
                                    $"\n\tAccount Number: {payment.Website.AccountNumber}" +
                                    $"\n\tAmount Paid: {payment.PaymentAmount}" +
                                    $"\n\tConfirmation Number: {payment.AssociatedTransaction.Confirmation.ConfirmationNumber}");
        }
    }
}
