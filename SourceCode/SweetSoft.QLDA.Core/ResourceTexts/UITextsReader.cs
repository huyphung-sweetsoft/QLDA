using System;
using System.Globalization;

namespace SweetSoft.QLDA.Core.ResourceTexts
{
    public class UITextsReader
    {
        public enum UITextsArea
        {
            FrontEnd,
            BackEnd
        }

        private static readonly string frontEndBaseName = "FrontEndTexts";
        private static readonly string backEndBaseName = "BackEndTexts";
        private static readonly Type resourceReader = typeof(UITextsReader);

        /// <summary>
        /// Get the resource text by ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public static string GetResourceText(UITextsArea uiTextsArea, string messageId)
        {
            if (uiTextsArea == UITextsArea.FrontEnd)
                return ResourceReader.GetResourceText(messageId, resourceReader, frontEndBaseName);
            else
                return ResourceReader.GetResourceText(messageId, resourceReader, backEndBaseName);
        }

        public static string GetFrontEndResourceText(string messageId)
        {
            return ResourceReader.GetResourceText(messageId, resourceReader, frontEndBaseName);
        }

        public static string GetBackEndResourceText(string messageId)
        {
            return ResourceReader.GetResourceText(messageId, resourceReader, backEndBaseName);
        }
        public static string GetFrontEndResourceText(CultureInfo cultureInfo, string messageId)
        {
            return ResourceReader.GetResourceText(cultureInfo, messageId, resourceReader, frontEndBaseName);
        }

        public static string GetBackEndResourceText(CultureInfo cultureInfo, string messageId)
        {
            return ResourceReader.GetResourceText(cultureInfo, messageId, resourceReader, backEndBaseName);
        }
        /// <summary>
        /// Get the resource text by ID with args is array of string for replacement in the text
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetResourceText(UITextsArea uiTextsArea, string messageId, params object[] args)
        {
            if (uiTextsArea == UITextsArea.FrontEnd)
                return ResourceReader.GetResourceText(messageId, resourceReader, frontEndBaseName, args);
            else
                return ResourceReader.GetResourceText(messageId, resourceReader, backEndBaseName, args);
        }

        public static string GetFrontEndResourceText(string messageId, params object[] args)
        {
            return ResourceReader.GetResourceText(messageId, resourceReader, frontEndBaseName, args);
        }

        public static string GetBackEndResourceText(string messageId, params object[] args)
        {
            return ResourceReader.GetResourceText(messageId, resourceReader, backEndBaseName, args);
        }
        public static void RenewResourceManager()
        {
            //ResourceReader.RenewResourceManager();
        }
    }
}
