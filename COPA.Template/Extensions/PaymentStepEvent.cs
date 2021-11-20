using Conservice.Payment.DataAccess.Enums;
using COPA.Template.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COPA.Template.Extensions
{
    public class PaymentStepEvent
    {
        public event EventHandler<PaymentStepEventArgs> PortalHookPerformed;
        public event EventHandler<PaymentStepEventArgs> PortalHookCompleted;
        public event EventHandler<PaymentStepEventArgs> StepUpdated;
        public event EventHandler<PaymentStepEventArgs> FailureStep;

        public delegate PaymentStep PaymentStepDelegate();
        public bool IsSuccessStep { get; set; }

        public PaymentStep Step 
        {
            get => PaymentStep.AutomationEncounteredError;
            set
            {
               OnStepUpdated(value);
            }
  
        }
        public PaymentStep RecoveryStep { get; set; }

        public void RunHook(PaymentStepDelegate hook)
        {
            OnPortalHookPerformed(hook);
            OnPortalHookCompleted();
        }

        protected virtual void OnPortalHookPerformed(PaymentStepDelegate hook)
        {
            if (PortalHookPerformed != null)
            {
                PortalHookPerformed(this, new PaymentStepEventArgs(hook.Invoke()));
            }

            throw new Exception("No valid argument to run.");
        }

        protected virtual void OnPortalHookCompleted()
        {

        }

        protected virtual void OnStepUpdated(PaymentStep paymentStep)
        {
            RecoveryStep = paymentStep;
            IsSuccessStep = Enum.GetName(typeof(SuccessStep), paymentStep) == string.Empty ? false : true;

            if (!IsSuccessStep)
            {
                OnFailureStep();
            }
        }

        protected virtual void OnFailureStep()
        {

        }
    }
}
