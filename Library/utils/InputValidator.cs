using System.Text.RegularExpressions;

namespace Library.utils
{
    public class InputValidator
    {
        public static (bool IsValid, List<string> Errors) ValidateRegister(string login, string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(login))
                errors.Add("Логин не может быть пустым.");

            if (login.Length < 7 || login.Length > 20)
                errors.Add("Логин должен содержать от 7 до 20 символов.");

            if (login.Contains(' '))
                errors.Add("Логин не должен содержать пробелов.");

            if (string.IsNullOrWhiteSpace(password))
                errors.Add("Пароль не может быть пустым.");

            if (password.Length < 6)
                errors.Add("Пароль должен содержать не менее 6 символов.");

            if (password.Contains(' '))
                errors.Add("Пароль не должен содержать пробелов.");

            if (!password.Any(char.IsUpper))
                errors.Add("Пароль должен содержать хотя бы одну заглавную букву.");

            if (!password.Any(char.IsDigit))
                errors.Add("Пароль должен содержать хотя бы одну цифру.");

            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]"))
                errors.Add("Пароль должен содержать хотя бы один спецсимвол (!@#$%^&* и т.д.).");

            return (errors.Count == 0, errors);
        }
    }
}

