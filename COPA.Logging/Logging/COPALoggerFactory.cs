using Conservice.Application;
using Conservice.Logging;
using Conservice.Payment.DataAccess.PaymentChronicles;
using Conservice.Selenium.WebDriver;
using COPA.Template.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COPA.Logging.Logging
{
    public class COPALoggerFactory
    {
        private readonly ApplicationEnvironment environment;
        private readonly ILogger logger;
        private readonly IPaymentChronicler paymentChronicler;
        private readonly ScreenshotHelper screenshotHelper;
        private readonly Driver driver;

        public COPALoggerFactory(ApplicationEnvironment environment, ILogger logger, IPaymentChronicler paymentChronicler, ScreenshotHelper screenshotHelper, Driver driver)
        {
            this.environment = environment;
            this.logger = logger;
            this.paymentChronicler = paymentChronicler;
            this.screenshotHelper = screenshotHelper;
            this.driver = driver;
        }

        public ICOPALogger CreateCOPALogger()
        {
            if (environment == ApplicationEnvironment.Test)
            {
                return new TestCOPALogger(logger);
            }

            return new LiveCOPALogger(logger, screenshotHelper, driver, paymentChronicler);
        }
    }
}
