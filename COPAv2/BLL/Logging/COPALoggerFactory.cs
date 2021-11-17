using Conservice.Application;
using Conservice.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COPAv2.BLL.Logging
{
    public class COPALoggerFactory
    {
        private readonly ApplicationEnvironment environment;
        private readonly ILogger logger;

        public COPALoggerFactory(ApplicationEnvironment environment, ILogger logger)
        {
            this.environment = environment;
            this.logger = logger;
        }

        public ICOPALogger CreateCOPALogger()
        {
            if (environment == ApplicationEnvironment.Test)
            {
                return new TestCOPALogger();
            }

            return new LiveCOPALogger(logger);
        }
    }
}
