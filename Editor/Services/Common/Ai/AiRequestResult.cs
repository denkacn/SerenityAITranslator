using System.Net;

namespace SerenityAITranslator.Editor.Services.Common.Ai
{
    public class AiRequestResult
    {
        public bool IsSuccess;
        public HttpStatusCode StatusCode;
        public string Text;
        public byte[] Bytes;
        public string ErrorMessage;

        public static AiRequestResult Success(HttpStatusCode statusCode, string text, byte[] bytes)
        {
            return new AiRequestResult
            {
                IsSuccess = true,
                StatusCode = statusCode,
                Text = text,
                Bytes = bytes
            };
        }

        public static AiRequestResult Failure(HttpStatusCode statusCode, string errorMessage, string text = null)
        {
            return new AiRequestResult
            {
                IsSuccess = false,
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                Text = text
            };
        }
    }
}
