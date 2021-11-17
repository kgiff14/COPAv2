using Conservice.Logging;
using Conservice.Payment.DataAccess.PaymentChronicles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COPAv2.BOL
{
    public class DeferPaymentChronicle
    {
        public JsonSerializerSettings JsonSettings { get; set; } = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        public IPaymentChronicler paymentChronicler { get; set; }

        public ILogger Logger { get; set; } = LoggerFactory.CreateCurrentClassLogger();
        public string recoveryFile { get; set; }
        public string completedChronicles { get; set; }

        /// <summary>
        /// Recovery File Directory Location.
        /// </summary>
        public string RecoveryFileDir { get; set; }
        public string CompletedChroniclesLocation { get; set; }
        public string RecoveryFileLocation { get; set; }
    }
}
