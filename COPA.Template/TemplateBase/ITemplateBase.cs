using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Selenium.WebDriver;
using COPA.Models;
using COPA.Template.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Conservice.Selenium.WebDriver.DriverStore;

namespace COPA.Template.TemplateBase
{
    public abstract class TemplateBaseAbstract
    {
        public Driver Driver { get; set; }
        internal TemplateBaseAbstract Template { get; set; }
        internal Payment Payment { get; set; }
        internal DriverTimeout WebDriverTimeout { get; set; } = DriverTimeout.T3;
        internal DriverTimeout CommandTimeout { get; set; } = DriverTimeout.T3;
        internal DriverType DriverType { get; set; } = DriverType.DefaultChromeDriver;
        internal PaymentStep? Step { get; set; }
        internal bool IsCardNumberVisible { get; set; } = false;
        internal List<Func<PaymentStep>> PortalHooks { get; set; }

        internal ILogger logger;

        public abstract PaymentStep StartWork(TransactionProcess transactionProcess);

        internal abstract TemplateBaseAbstract CreateTemplateInstance(TransactionProcess transactionProcess);

        internal abstract void SetTemplateConditions();

        internal abstract void StartDriver();

        internal abstract void DriverCleanup();
        internal abstract void LogException(Exception e);

        internal abstract List<Func<PaymentStep>> SetPortalHooks();
        internal abstract void RunTemplate();
        internal abstract bool IsSuccessStep();
    }
}
