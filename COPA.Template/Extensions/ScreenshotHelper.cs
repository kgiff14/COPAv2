using COPA.Models;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace COPA.Template.Extensions
{
    public class ScreenshotHelper
    {
        public static ScreenshotHelper GetInstance() =>
            new ScreenshotHelper();

        private readonly ImageManager.AutomationCOPAImageManager imageManager;
        public void TrySaveScreenshot(string pageSource, Screenshot screenshot, bool isSuccessImage, Payment payment)
        {
            //Regex comes from: http://www.richardsramblings.com/regex/credit-card-numbers/
            string creditCardRegex = @"\b(?:3[47]\d{2}([\ \-]?)\d{6}\1\d|(?:(?:4\d|5[1-5]|65)\d{2}|6011)([\ \-]?)\d{4}\2\d{4}\2)\d{4}\b";

            if (!Regex.Match(pageSource, creditCardRegex).Success)
            {
                SaveScreenshot(screenshot, isSuccessImage, payment);
            }
        }

        public void SaveScreenshot(Screenshot screenshot, bool isSuccessImage, Payment payment)
        {
            var controlNumber = imageManager.GetControlNumber(isSuccessImage);
            payment.AssociatedTransaction.Confirmation.ControlNumber = controlNumber;
            string filePath = Path.GetTempPath() + $"{controlNumber}.png";

            screenshot.SaveAsFile(filePath, ScreenshotImageFormat.Png);

            imageManager.Save(filePath, payment.Website.ProviderName);
            new FileInfo(filePath).Delete();
        }
    }
}
