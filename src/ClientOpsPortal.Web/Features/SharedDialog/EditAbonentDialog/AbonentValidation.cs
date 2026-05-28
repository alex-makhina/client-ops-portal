using System.Text.RegularExpressions;

namespace ClientOpsPortal.Web.Features.SharedDialog
{
    public static class AbonentValidationHelper
    {
        public static (bool IsValid, string ErrorMessage) ValidateName(string value, string fieldName, int min = 2, int max = 50)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (false, $"{fieldName} обязательна для заполнения");

            if (value.Length < min || value.Length > max)
                return (false, $"{fieldName} должна содержать от {min} до {max} символов");

            if (!Regex.IsMatch(value, @"^[А-Яа-яA-Za-z\s\-]+$"))
                return (false, $"{fieldName} может содержать только буквы, пробелы и дефисы");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateMiddleName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (true, string.Empty);

            if (value.Length > 50)
                return (false, "Отчество не может быть длиннее 50 символов");

            if (!Regex.IsMatch(value, @"^[А-Яа-яA-Za-z\s\-]+$"))
                return (false, "Отчество может содержать только буквы, пробелы и дефисы");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateIdentificationNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (false, "Идентификационный номер обязателен");

            if (value.Length < 5 || value.Length > 20)
                return (false, "Идентификационный номер должен содержать от 5 до 20 символов");

            if (!Regex.IsMatch(value, @"^[A-Z0-9]+$"))
                return (false, "Идентификационный номер может содержать только заглавные буквы и цифры");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (false, "Email обязателен для заполнения");

            try
            {
                var addr = new System.Net.Mail.MailAddress(value);
                return addr.Address == value
                    ? (true, string.Empty)
                    : (false, "Введите корректный Email");
            }
            catch
            {
                return (false, "Введите корректный Email");
            }
        }

        public static string NormalizeIdentificationNumber(string value)
        {
            return value?.Trim().ToUpper() ?? string.Empty;
        }

        public static string NormalizeName(string value)
        {
            return value?.Trim() ?? string.Empty;
        }
    }
}