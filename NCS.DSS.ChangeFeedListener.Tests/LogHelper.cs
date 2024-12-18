namespace NCS.DSS.ChangeFeedListener.Tests
{
    public static class LogHelper
    {        
        public static bool LogMessageMatcher(object formattedLogValueObject, string message)
        {
            var logValues = formattedLogValueObject as IReadOnlyList<KeyValuePair<string, object>>;

            if (logValues == null)
                return false;

            var loggedMessage = logValues.FirstOrDefault(logValue => logValue.Key == "{OriginalFormat}").Value?.ToString();

            var documentId = logValues.FirstOrDefault(logValue => logValue.Key == "DocumentID").Value?.ToString();
            if (loggedMessage == null)
                return false;

            loggedMessage = loggedMessage.Replace("{DocumentID}", documentId);

            return loggedMessage?.Contains(message) ?? false;
        }
    }
}
