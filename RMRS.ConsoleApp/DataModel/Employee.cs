using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RMRS.ConsoleApp.DataModel;

[DisplayName("Сотрудник")]
public partial class Employee
{
    [DisplayName("Id Сотрудника")]
    public int EmployeeId { get; set; }

    [DisplayName("Имя")]
    public string? FirstName { get; set; }

    [DisplayName("Фамилия")]
    public string? LastName { get; set; }

    [DisplayName("Эл. адрес")]
    public string? Email { get; set; }

    /// <summary>
    /// В общем случае, может быть не определена
    /// </summary>
    [DisplayName("Дата рождения")]
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// В общем случае, может быть не определена. И это - не == 0
    /// В аналитике используем как Salary ?? 0
    /// </summary>
    [DisplayName("Зарплата")]
    public decimal? Salary { get; set; } 
}
