using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Payment.DataAccess.PaymentChronicles;
using Conservice.Payment.DataAccess.PaymentChronicles.PaymentChronicle;
using COPA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COPA.Logging.Logging
{
    public interface ICOPALogger
    {
        public PaymentChronicle ConvertToPaymentChronicle(Payment payment, PaymentStep paymentStep, Exception e = null);

        public void RecordSuccessfulTransaction(Payment payment);

        public void LogException(Exception exception, PaymentStep actualStep, Payment payment, bool isCardNumberVisible = true);
    }
}
