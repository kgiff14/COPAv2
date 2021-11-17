using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Payment.DataAccess.PaymentChronicles.PaymentChronicle;
using COPA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COPAv2.BLL.Logging
{
    public interface ICOPALogger
    {
        public PaymentChronicle ConvertToPaymentChronicle(Payment payment, PaymentStep paymentStep, Exception e = null);

        public void RecordSuccessfulTransaction(ILogger logger, Payment payment);

        public void LogException(Exception exception, PaymentStep actualStep, bool isCardNumberVisible = true);
    }
}
