using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RMRS.ConsoleApp.Helpers;

/// <summary>
/// Ввод и валидация
/// </summary>
public static class UserInteractiveHelper
{
    /// <summary>
    /// Количество строк в текущем ConsoleApp окне, view pane lines count
    /// </summary>
    /// Может испольоваться, например, при пагинации выводимого списка строк
    /// <returns>int</returns>
    public static int GetConsoleHeight()
    {
        try
        {
            var height = Console.WindowHeight;
            //Console.WriteLine($"Высота окна консоли: {height} строк");

            return height;
        }
        catch (IOException)
        {
            Console.WriteLine("Невозможно определить высоту окна. Возможно, вывод перенаправлен.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }

        return 0;
    }

    /// <summary>
    /// Ожидание нажатия любой клавиши или ESC
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns>true если нажата ESC</returns>
    public static bool PressAnyKeyOrEsc(string prompt = "Нажмите любую клавишу ...")
    {
        Console.WriteLine(prompt);
        var keyInfo = Console.ReadKey(true);

        return keyInfo.Key == ConsoleKey.Escape;
    }

    public static bool GetConfirmation(string prompt, string confirmationChars = "yY")
    {
        Console.Write($"{prompt}: ");
        var keyChar = Console.ReadKey().KeyChar;

        if (confirmationChars.Contains(keyChar))
        {
            return true;
        }

        return false;
    }

    public static int? GetInt(string fieldName, bool isRequired)
    {
        while (true)
        {
            Console.Write($"Введите {fieldName}: ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                if (isRequired)
                {
                    Console.WriteLine($"Ошибка! Значение {fieldName} - обязательное");
                    continue;
                }
                return null;
            }
            if (int.TryParse(input, out int value))
            {
                return value;
            }
        }
    }

    public static int GetId(string fieldName)
    {
        while (true)
        {
            var value = GetInt(fieldName, true);
            if (value > 0)
            {
                return value.Value;
            }
            Console.WriteLine($"Ошибка! Значение {fieldName} должно быть положительное");
        }
    }

    public static string GetString(string fieldName, bool isRequired, int maxLength)
    {
        while (true)
        {
            Console.Write($"Введите {fieldName}: ");
            var input = Console.ReadLine()?.Trim();

            if (isRequired && string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"Ошибка! Значение {fieldName} - обязательное");
                continue;
            }

            if (input?.Length > maxLength)
            {
                Console.WriteLine($"Ошибка! Длина поля {fieldName} должна быть меньше {maxLength} символов");
                continue;
            }

            return input ?? string.Empty;
        }
    }

    public static string GetEmail(string fieldName, bool isRequired)
    {
        while (true)
        {
            var email = GetString(fieldName, isRequired, 100);

            if (!isRequired && string.IsNullOrWhiteSpace(email))
            {
                return string.Empty;
            }

            // Воможна более качественная проверка email,
            // <see cref="https://learn.microsoft.com/ru-ru/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format"/>
            if (IsEmailValid(email))
            {
                return email;
            }

            Console.WriteLine($"Ошибка! Неверный формат поля {fieldName}");
        }
    }

    /// <summary>
    /// Валидация формата email
    /// </summary>
    /// <see cref="https://learn.microsoft.com/ru-ru/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format"/>
    /// Можно проводить более строгую проверку,
    /// На корректность домена, IdnMapping для Unicode, ...
    public static bool IsEmailValid(string email)
    {
        string pattern = "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    public static DateOnly? GetDate(string fieldName, bool isRequired)
    {
        while (true)
        {
            Console.Write($"Введите дату (yyyy-mm-dd) для {fieldName}: ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                if (isRequired)
                {
                    Console.WriteLine($"Ошибка! Значение {fieldName} - обязательное");
                    continue;
                }
                return null;
            }
            if (DateOnly.TryParse(input, out DateOnly date))
            {
                return date;
            }
            else
            {
                Console.WriteLine($"Ошибка! Неверный формат даты для поля {fieldName}");
            }
        }
    }

    public static DateOnly? GetBirthDate(string fieldName, bool isRequired)
    {
        while (true)
        {
            var date = GetDate(fieldName, isRequired);
            if (!isRequired && !date.HasValue)
            {
                return null;
            }
            if (date < DateOnly.FromDateTime(DateTime.Now))
            {
                return date;
            }
            Console.WriteLine($"Ошибка! Дата рождения должна быть в прошлом");
        }
    }

    public static decimal? GetDecimal(string fieldName, bool isRequired)
    {
        while (true)
        {
            Console.Write($"Введите {fieldName}: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                if (isRequired)
                {
                    Console.WriteLine($"Ошибка! Значение {fieldName} - обязательное");
                    continue;
                }
                return null;
            }
            if (decimal.TryParse(input,
                NumberStyles.Currency,
                CultureInfo.InvariantCulture,
                out decimal value))
            {
                return value;
            }

            else
            {
                Console.WriteLine($"Ошибка! Значение {fieldName} - число");
            }
        }
    }

    public static decimal? GetDecimalPositive(string fieldName, bool isRequired)
    {
        while (true)
        {
            var value = GetDecimal(fieldName, isRequired);
            if (!isRequired && !value.HasValue)
            {
                return null;
            }
            if (value > 0)
            {
                return value.Value;
            }
            Console.WriteLine($"Ошибка! Значение {fieldName} должно быть положительное");
        }
    }
}