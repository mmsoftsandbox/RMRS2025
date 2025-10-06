using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using RMRS.ConsoleApp.Helpers;
using static RMRS.ConsoleApp.DataModel.Employee;

namespace RMRS.ConsoleApp.DataModel;

public partial class Employee
{
    /// <summary>
    /// Перечень полей сущности Employee.
    /// Строго типиированный, для использования во всем приложении.
    /// Можно реалиовать через рефлексию.
    /// </summary>
    public enum Props
    {
        EmployeeId,
        FirstName,
        LastName,
        Email,
        DateOfBirth,
        Salary
    }

    /// <summary>
    /// Спецификация колонок для вывода. 
    /// Ширина колонки и показать/скрыть.
    /// Можно вывести в отдельный json файл для конфигурирования
    /// Или завести пользовательские атрибуты
    /// </summary>
    public static Dictionary<string, (int Width, bool Hidden)> ColumnsSpecification { get; } = new Dictionary<string, (int, bool)> {
            { nameof(Employee.Props.EmployeeId), (10, false)},
            { nameof(Employee.Props.FirstName), (50, false)},
            { nameof(Employee.Props.LastName), (50, false)},
            { nameof(Employee.Props.Email), (20, false)},
            { nameof(Employee.Props.DateOfBirth), (10, false)},
            { nameof(Employee.Props.Salary), (10, false)},
        };

    public static string DisplayName { get; } = typeof(Employee).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? nameof(Employee);

    public static Dictionary<string, string> PropNames => _propNames.Value;

    private readonly static Lazy<Dictionary<string, string>> _propNames = new Lazy<Dictionary<string, string>>(() =>
    {
        var props = typeof(Employee).GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public);
        //return new Dictionary<Props, string>();
        return props.ToDictionary(x => x.Name, x => x.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? x.Name);
    });
}
