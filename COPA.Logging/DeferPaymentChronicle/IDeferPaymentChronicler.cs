using Conservice.Payment.DataAccess.PaymentChronicles.PaymentChronicle.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;

namespace COPAv2.BLL.DeferPaymentChronicle
{
    public interface IDeferPaymentChronicler
    {
        public void LogSync(String recoveryLocation = "", String completeLocation = "", int timeOut = 30);

        public void CheckRecoveryFile();

        public List<IPaymentChronicle> ReadFile(String location);

        public List<IPaymentChronicle> CheckCompletedChronicles(List<IPaymentChronicle> chronicles, String recoveryLocation = "", String completedLocation = "");

        public void UpdateCompletedRecords(IPaymentChronicle paymentChronicle, String location);

        public HttpStatusCode recordChronicle(IPaymentChronicle chronicle);
    }
}
