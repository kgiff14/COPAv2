using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Selenium.WebDriver;
using Conservice.Selenium.WebFramework;
using COPA.Models;
using COPA.Template;
using COPA.Template.Enums;
using COPA.Template.Extensions;
using COPAv2.BLL.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        internal PaymentStep Step { get; set; }
        internal bool IsCardNumberVisible { get; set; } = false;
        internal List<Func<PaymentStep>> PortalHooks { get; set; }
        internal TemplateBase TemplateBase { get; set; }
        internal TransactionProcess TransactionProcess { get; set; }

        internal readonly ILogger logger;
        internal readonly ICOPALogger COPALogger;
        internal readonly ScreenshotHelper screenshotHelper;

        public DebugRunLogic(ILogger logger, ICOPALogger COPALogger, ScreenshotHelper screenshotHelper)
        {
            this.logger = logger;
            this.COPALogger = COPALogger;
            this.screenshotHelper = screenshotHelper;
        }

        public PaymentStep StartWork()
        {
            try
            {
                DriverCleanup();
                StartDriver();
                TemplateBase.SetPortalHooks();
                RunTemplate();

                return Step;
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

                return Step;
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
            foreach (var hook in PortalHooks)
            {
                Step = hook.Invoke();

                if (true)
                {
                    break;
                }
            }
        }
    }
}
