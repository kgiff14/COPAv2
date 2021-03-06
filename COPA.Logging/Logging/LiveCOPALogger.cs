using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Payment.DataAccess.PaymentChronicles;
using Conservice.Payment.DataAccess.PaymentChronicles.PaymentChronicle;
using Conservice.Selenium.WebDriver;
using COPA.Models;
using COPA.Template.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COPA.Logging.Logging
{
    public class LiveCOPALogger : ICOPALogger
    {
        private readonly ILogger logger;
        private readonly ScreenshotHelper screenshotHelper;
        private readonly Driver Driver;
        private readonly IPaymentChronicler paymentChronicler;

        public LiveCOPALogger(ILogger logger, ScreenshotHelper screenshotHelper, Driver Driver, IPaymentChronicler paymentChronicler)
        {
            this.logger = logger;
            this.screenshotHelper = screenshotHelper;
            this.Driver = Driver;
            this.paymentChronicler = paymentChronicler;
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

        public void LogException(Exception exception, PaymentStep actualStep, Payment payment, bool isCardNumberVisible = true)
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
                        screenshotHelper.TrySaveScreenshot(Driver.Instance.PageSource, Driver.Instance.GetScreenshot(), false, payment);
                    }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.Trace, null, $"PaymentInvoiceNumber: {payment.PaymentInvoiceNumber}; Error encountered when attempting to save ScreenShot", null, e);
                    }
                }
                paymentChronicler.LogPaymentException(ConvertToPaymentChronicle(payment, actualStep, exception));
            }
            else
            {
                payment.PaymentStatus = PaymentStatus.CannotPayTransactionError;
                if (!isCardNumberVisible)
                {
                    try
                    {
                        screenshotHelper.TrySaveScreenshot(Driver.Instance.PageSource, Driver.Instance.GetScreenshot(), false, payment);
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
