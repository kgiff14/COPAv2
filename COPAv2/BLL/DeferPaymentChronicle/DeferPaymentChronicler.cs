using Conservice.Logging;
using Conservice.Payment.DataAccess.PaymentChronicles;
using Conservice.Payment.DataAccess.PaymentChronicles.PaymentChronicle.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace COPAv2.BLL.DeferPaymentChronicle
{
    public class DeferPaymentChronicler : IDeferPaymentChronicler
    {
        public JsonSerializerSettings JsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        private readonly IPaymentChronicler paymentChronicler;

        private readonly ILogger logger;
        private string recoveryFile;
        private readonly string completedChronicles;

        /// <summary>
        /// Recovery File Directory Location.
        /// </summary>
        public readonly string RecoveryFileDir;
        internal readonly string completedChroniclesLocation;
        internal readonly string recoveryFileLocation;

        /// <summary>
        /// Constructor to enable Dependency Inversion and Unit Testing.
        /// </summary>
        /// <param name="recordPayment"></param>
        public DeferPaymentChronicler(IPaymentChronicler paymentChronicler, IConfiguration config, ILogger logger)
        {
            this.paymentChronicler = paymentChronicler;
            this.logger = logger;
            recoveryFile = config.GetSection("Chronicler").GetValue<string>("RecoveryFile");
            completedChronicles = config.GetSection("Chronicler").GetValue<string>("CompletedFile");
            RecoveryFileDir = config.GetSection("Chronicler").GetValue<string>("RecoveryLocal");
            completedChroniclesLocation = RecoveryFileDir + completedChronicles;
            recoveryFileLocation = RecoveryFileDir + recoveryFile;
        }

        public List<IPaymentChronicle> CheckCompletedChronicles(List<IPaymentChronicle> chronicles, string recoveryLocation = "", string completedLocation = "")
        {
            recoveryLocation = String.IsNullOrEmpty(recoveryLocation) ? recoveryFileLocation : recoveryLocation;
            completedLocation = String.IsNullOrEmpty(completedLocation) ? completedChroniclesLocation : completedLocation;
            if (File.Exists(completedLocation))
            {
                List<IPaymentChronicle> completeChronicles = ReadFile(completedLocation);
                foreach (var completeChronicle in completeChronicles)
                {
                    if (chronicles.Exists(x => x.LoggedAt == completeChronicle.LoggedAt
                    && x.PaymentID == completeChronicle.PaymentID
                    && x.PaymentStatusID == completeChronicle.PaymentStatusID
                    && x.PaymentStep == completeChronicle.PaymentStep))
                    {
                        chronicles.RemoveAll(x => x.LoggedAt == completeChronicle.LoggedAt
                        && x.PaymentID == completeChronicle.PaymentID
                        && x.PaymentStatusID == completeChronicle.PaymentStatusID
                        && x.PaymentStep == completeChronicle.PaymentStep);
                    }
                }
                //Updates original file.
                File.WriteAllText(recoveryLocation, JsonConvert.SerializeObject(chronicles, typeof(List<IPaymentChronicle>), JsonSettings));
                //Cleans up Completed Logs.
                while (File.Exists(completedLocation))
                {
                    File.Delete(completedLocation);
                }
            }
            return chronicles;
        }

        public void CheckRecoveryFile()
        {
            if (!Directory.Exists(RecoveryFileDir))
            {
                Directory.CreateDirectory(RecoveryFileDir);
            }
            else if (Directory.GetFiles(RecoveryFileDir).Length > 0)
            {
                LogSync();
            }
        }

        public void LogSync(string recoveryLocation = "", string completeLocation = "", int timeOut = 30)
        {
            recoveryLocation = String.IsNullOrEmpty(recoveryLocation) ? recoveryFileLocation : recoveryLocation;
            completeLocation = String.IsNullOrEmpty(completeLocation) ? completedChroniclesLocation : completeLocation;

            if (File.Exists(recoveryLocation))
            {
                List<IPaymentChronicle> currentLogs = ReadFile(recoveryLocation);
                currentLogs = CheckCompletedChronicles(currentLogs);
                if (currentLogs != null)
                {
                    foreach (var chronicle in currentLogs)
                    {
                        HttpStatusCode response = recordChronicle(chronicle);
                        var startTime = DateTime.Now;
                        while (!response.Equals(HttpStatusCode.OK)
                            && DateTime.Now < startTime.AddMinutes(timeOut))
                        {
                            Random r = new Random();
                            Thread.Sleep(TimeSpan.FromSeconds(r.Next(0, timeOut != 0 ? timeOut : 10 * 60)));
                            response = recordChronicle(chronicle);
                        }

                        if (!response.Equals(HttpStatusCode.OK))
                        {
                            logger.Log(LogLevel.Fatal, $"Failed to Send PaymentChronicle to COPA.PACKAGER: {response}; {chronicle}");
                            throw new Exception("Failed to Send PaymentChronicle to COPA.PACKAGER");
                        }
                        else
                        {
                            UpdateCompletedRecords(chronicle, completeLocation);
                        }
                    }
                }
                //Cleanup
                while (File.Exists(recoveryLocation))
                {
                    File.Delete(recoveryLocation);
                    Thread.Sleep(100);
                }
            }
        }

        public List<IPaymentChronicle> ReadFile(string location)
        {
            List<IPaymentChronicle> files = new List<IPaymentChronicle>();
            try
            {
                files = JsonConvert.DeserializeObject<List<IPaymentChronicle>>(File.ReadAllText(location), JsonSettings);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e);
                recoveryFile = null;
            }
            return files;
        }

        public HttpStatusCode recordChronicle(IPaymentChronicle chronicle)
        {
            if (chronicle.ChronicleException == null && chronicle.ChronicleSuccessful == null)
            {
                return paymentChronicler.LogPaymentStep(chronicle);
            }
            else if (chronicle.ChronicleException != null)
            {
                return paymentChronicler.LogPaymentException(chronicle);
            }
            else
            {
                return paymentChronicler.RecordSuccessfulTransaction(chronicle);
            }
        }

        public void UpdateCompletedRecords(IPaymentChronicle paymentChronicle, string location)
        {
            List<IPaymentChronicle> currentRecords = new List<IPaymentChronicle>();
            if (File.Exists(location))
            {
                currentRecords.AddRange(JsonConvert.DeserializeObject<List<IPaymentChronicle>>(File.ReadAllText(location), JsonSettings));
            }
            currentRecords.Add(paymentChronicle);
            File.WriteAllText(location, JsonConvert.SerializeObject(currentRecords, typeof(List<IPaymentChronicle>), JsonSettings));
        }
    }
}
