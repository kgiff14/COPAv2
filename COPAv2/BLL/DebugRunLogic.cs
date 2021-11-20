using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Payment.DataAccess.PaymentChronicles;
using Conservice.Selenium.WebDriver;
using Conservice.Selenium.WebFramework;
using COPA.Logging.Logging;
using COPA.Models;
using COPA.Template;
using COPA.Template.Enums;
using COPA.Template.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static Conservice.Selenium.WebDriver.DriverStore;

namespace COPAv2.BLL
{
    public class DebugRunLogic
    {
        public Driver Driver { get; set; }
        public Portal Portal { get; set; }
        internal Payment Payment { get; set; }
        internal DriverTimeout WebDriverTimeout { get; set; } = DriverTimeout.T3;
        internal DriverTimeout CommandTimeout { get; set; } = DriverTimeout.T3;
        internal DriverType DriverType { get; set; } = DriverType.DefaultChromeDriver;
        internal bool IsCardNumberVisible { get; set; } = false;
        internal TemplateBase TemplateBase { get; set; }
        internal TransactionProcess TransactionProcess { get; set; }
        public PaymentStepEvent PaymentStepEvent = new PaymentStepEvent();

        internal readonly ILogger logger;
        internal readonly ICOPALogger COPALogger;
        internal readonly ScreenshotHelper screenshotHelper;
        internal readonly IPaymentChronicler paymentChronicler;

        public DebugRunLogic(ILogger logger, ICOPALogger COPALogger, ScreenshotHelper screenshotHelper, PaymentStepEvent PaymentStepEvent, IPaymentChronicler paymentChronicler)
        {
            this.logger = logger;
            this.COPALogger = COPALogger;
            this.screenshotHelper = screenshotHelper;
            this.PaymentStepEvent = PaymentStepEvent;
            this.paymentChronicler = paymentChronicler;
            this.PaymentStepEvent.PortalHookPerformed += PaymentStepEvent_OnHookPerformed;
            this.PaymentStepEvent.StepUpdated += PaymentStepEvent_StepUpdated;
            TemplateBase = new TemplateBase(GetDriver(DriverType.DefaultChromeDriver, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)), Payment, PaymentStepEvent);
        }

        private void PaymentStepEvent_StepUpdated(object sender, PaymentStepEventArgs e)
        {
            if (PaymentStepEvent.RecoveryStep != PaymentStepEvent.Step)
            {
                paymentChronicler.LogPaymentStep(COPALogger.ConvertToPaymentChronicle(Payment, PaymentStepEvent.Step));
            }

            if (!PaymentStepEvent.IsSuccessStep)
            {
                //kill template process instance
            }
        }

        public PaymentStep StartWork()
        {
            try
            {
                DriverCleanup();
                StartDriver();
                TemplateBase.SetPortalHooks();
                RunTemplate();

                return PaymentStepEvent.Step;
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Trace, e);

                if (Payment.PaymentStatus == PaymentStatus.AwaitingConfirmation
                    || Payment.PaymentStatus == PaymentStatus.Paid)
                {
                    Payment.PaymentStatus = PaymentStatus.Unconfirmed;

                    if (!IsCardNumberVisible)
                    {
                        try
                        {
                            screenshotHelper.TrySaveScreenshot(Driver.Instance.PageSource, Driver.Instance.GetScreenshot(), false, Payment);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Trace, null, $"PaymentInvoiceNumber: {Payment.PaymentInvoiceNumber}; Error encountered when attempting to save ScreenShot", null, ex);
                        }
                    }
                }

                return PaymentStepEvent.Step;
            }
            finally
            {
                Driver.Dispose();
            }
        }

        private void StartDriver()
        {
            try
            {
                CreateTemplateInstance();
                TemplateBase.SetTemplateConditions();

                Driver = GetDriver(DriverType.DefaultChromeDriver, TimeSpan.FromMinutes((int)CommandTimeout), TimeSpan.FromMinutes((int)WebDriverTimeout));
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e);
                throw;
            }
        }

        private void DriverCleanup()
        {
            var driverNames = new string[] { "chromedriver", "geckodriver" };
            foreach (var name in driverNames)
            {
                logger.Log(LogLevel.Trace, $"\n\tKilling all {name} processes");
                var driverProcesses = System.Diagnostics.Process.GetProcessesByName(name);

                foreach (var driverProcess in driverProcesses)
                {
                    driverProcess.Kill();
                    driverProcess.Close();
                }
            }
        }

        private void CreateTemplateInstance()
        {
            var assembly = typeof(TemplateBase).Assembly;
            var classes = assembly.GetTypes().ToList();
            var templateType = classes.Single(temp => temp.Name.Equals(TransactionProcess.TemplateName));

            Payment = TransactionProcess.Payment;

            TemplateBase = (TemplateBase)Activator.CreateInstance(templateType);
        }

        private void CompleteTemplateInstance()
        {
            TemplateBase.Driver = Driver;
            TemplateBase.Payment = TransactionProcess.Payment;
            Payment = TransactionProcess.Payment;
        }


        private void RunTemplate()
        {
            using var process = new Process();
            process.StartInfo.FileName = "C:\\Users\\kordellgifford\\source\\repos\\COPAv2\\COPA.Template\\bin\\Debug\\net5.0\\COPA.Template.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = "WEEnergies_CC";
            var test = JsonSerializer.Serialize(Payment);
            var test2 = new List<string>
            {
                test,
                "WEEnergies"
            };
            process.Start("C:\\Users\\kordellgifford\\source\\repos\\COPAv2\\COPA.Template\\bin\\Debug\\net5.0\\COPA.Template.exe", );
            foreach (var hook in TemplateBase.PortalHooks)
            {
                PaymentStepEvent.RunHook(hook);

                if (true)
                {
                    break;
                }
            }
        }

        private void PaymentStepEvent_OnHookPerformed(object sender, PaymentStepEventArgs e)
        {
            PaymentStepEvent.Step = e.Step;
            PaymentStepEvent.IsSuccessStep = Enum.GetName(typeof(SuccessStep), PaymentStepEvent.Step) == string.Empty ? false : true;
        }
    }
}
