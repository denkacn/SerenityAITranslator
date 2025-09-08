namespace SerenityAITranslator.Editor.Tools
{
    using System;
    using System.IO;

    public static class FileNameSanitizer
    {
        public static string Sanitize(string input, int maxLength = 255)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("The name cannot be empty", nameof(input));
            
            var trimmed = input.Trim();
            var invalids = Path.GetInvalidFileNameChars();
            
            foreach (var ch in invalids)
            {
                trimmed = trimmed.Replace(ch, '_');
            }
            
            if (trimmed.Length > maxLength)
                trimmed = trimmed.Substring(0, maxLength);

            return trimmed;
        }
    }

}