using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Payment.DataAccess.PaymentChronicles;
using Conservice.Payment.DataAccess.PaymentChronicles.PaymentChronicle;
using COPA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COPAv2.BLL.Logging
{
    public class LiveCOPALogger : ICOPALogger
    {
        private readonly ILogger logger;

        public LiveCOPALogger(ILogger logger)
        {
            this.logger = logger;
        }

        public PaymentChronicle ConvertToPaymentChronicle(Payment payment, PaymentStep paymentStep, Exception e = null)
        {
            var chronicle = new PaymentChronicle()
            {
                PaymentID = payment.PaymentID,
                PaymentTransactionID = payment.AssociatedTransaction.TransactionID,
                LoggedAt = DateTime.Now,
                PaymentStatusID = payment.PaymentStatus,
                PaymentStep = paymentStep,
            };

            if (e != null)
            {
                chronicle.ChronicleException = new ChronicleException()
                {
                    PaymentErrorControlNumber = payment.AssociatedTransaction.Confirmation.ControlNumber,
                    ExceptionMessage = e.Message,
                    ExceptionTypeName = e.GetType().Name,
                    StackTrace = e.StackTrace
                };
            }
            else if (payment.AssociatedTransaction.Confirmation != null && payment.AssociatedTransaction.PaidAt != null)
            {
                chronicle.ChronicleSuccessful = new ChronicleSuccessful()
                {
                    Fee = payment.AssociatedTransaction.Fee,
                    PaymentControlNumber = string.IsNullOrEmpty(payment.AssociatedTransaction.Confirmation.ControlNumber) ? "N/A" : payment.AssociatedTransaction.Confirmation.ControlNumber,
                    PaymentDestinationTypeID = payment.AssociatedTransaction.PaymentDestinationType,
                    PaymentUserID = AutomationUser.JangoUser,
                    PaymentMethodID = payment.AssociatedTransaction.PaymentMethod,
                    ConfirmationNumber = payment.AssociatedTransaction.Confirmation.ConfirmationNumber,
                    TransactionAmount = payment.AssociatedTransaction.TransactionAmount,
                    PaidAt = payment.AssociatedTransaction.PaidAt.Value
                };
            }

            return chronicle;
        }

        public void LogException(Exception exception, PaymentStep actualStep, Payment payment, IPaymentChronicler paymentChronicler, bool isCardNumberVisible = true)
        {
            logger.Log(LogLevel.Trace, $"PaymentInvoiceNumber: {payment.PaymentInvoiceNumber}\n\tLogging Payment Exception: PaymentID:{payment.PaymentID} Step:{actualStep} Exception:{exception}");

            if (payment.PaymentStatus == PaymentStatus.AwaitingConfirmation
                || payment.PaymentStatus == PaymentStatus.Paid)
            {
                logger.Log(LogLevel.Trace, $"\n\tPayment is Unconfirmed");
                payment.PaymentStatus = PaymentStatus.Unconfirmed;

                if (!isCardNumberVisible)
                {
                    try
                    {
                        TrySaveScreenshot(Driver.Instance.PageSource, Driver.Instance.GetScreenshot(), false);
                    }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.Trace, null, $"PaymentInvoiceNumber: {payment.PaymentInvoiceNumber}; Error encountered when attempting to save ScreenShot", null, e);
                    }
                }
                PaymentChronicler.LogPaymentException(ConvertToPaymentChronicle(payment, actualStep, exception));
            }
            else
            {
                payment.PaymentStatus = PaymentStatus.CannotPayTransactionError;
                if (!IsCardNumberVisible)
                {
                    try
                    {
                        TrySaveScreenshot(Driver.Instance.PageSource, Driver.Instance.GetScreenshot(), false);
                    }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.Trace, null, $"PaymentInvoiceNumber: {payment.PaymentInvoiceNumber}; Error encountered when attempting to save ScreenShot", null, e);
                    }
                }
                paymentChronicler.LogPaymentException(ConvertToPaymentChronicle(payment, actualStep, exception));
            }
        }

        public void RecordSuccessfulTransaction(Payment payment)
        {
            throw new NotImplementedException();
        }
    }
}
