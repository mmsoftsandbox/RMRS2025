using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RMRS.ConsoleApp.DataModel;
using RMRS.ConsoleApp.Helpers;
using static RMRS.ConsoleApp.Helpers.UserInteractiveHelper;
using static RMRS.ConsoleApp.Helpers.StringHelper;
using System.Diagnostics;
using System.Reflection;

namespace RMRS.ConsoleApp;

public class BusinessLogic
{
    private IDataLayer _dataLayer { get; set; }
    private IConsoleScreen _console { get; set; }

    private MenuData _menu { get; set; } = new MenuData();


    public BusinessLogic(IDataLayer dataLayer, IConsoleScreen consoleScreen)
    {
        _dataLayer = dataLayer;
        _console = consoleScreen;
    }

    private void InitMenu()
    {
        _menu = new MenuData()
        {
            MenuTree = new List<string>()
                {
                    "Добавить нового сотрудника",
                    "Посмотреть всех сотрудников",
                    "Обновить информацию о сотруднике",
                    "Удалить сотрудника",
                    "Сотрудники с зарплатой выше среднего",
                    "Выйти из приложения"
                 }.Select((x, i) => new MenuData.MenuItem() { ActionId = i + 1, Prompt = x })
                .ToList(),
            MenuTitle = $"Система \"{Employee.DisplayName}\"",
            Prompt = "Выберите пункт меню: ",
            WrongPrompt = "Неправильный ввод!",
            Hint = _console.HintDefault
        };

        _menu.MenuTree[index: 0].Operation = AddEmployee;
        _menu.MenuTree[index: 1].Operation = ViewEmployees;
        _menu.MenuTree[index: 2].Operation = UpdateEmployee;
        _menu.MenuTree[index: 3].Operation = DeleteEmployee;
        _menu.MenuTree[index: 4].Operation = ShowAnalytics;

        _menu.MenuTree[index: 5].Operation = _console.DoMenuExitDefault; // Стандартный выход из меню
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        InitMenu();
        await _console.RunMenuLoop(_menu, ct);
    }

    public async Task<OperationResult?> AddEmployee()
    {
        try
        {
            Console.WriteLine($"Все записи {Employee.DisplayName}");

            var employee = new Employee();
            employee.FirstName = GetString(Employee.PropNames[nameof(employee.FirstName)], true, 50);
            employee.LastName = GetString(Employee.PropNames[nameof(employee.LastName)], true, 50);
            employee.Email = GetEmail(Employee.PropNames[nameof(employee.Email)], isRequired: false);
            employee.DateOfBirth = GetBirthDate(Employee.PropNames[nameof(employee.DateOfBirth)], isRequired: false);
            employee.Salary = GetDecimalPositive(Employee.PropNames[nameof(employee.Salary)], isRequired: false);

            await _dataLayer.AddEmployee(employee);

            Console.WriteLine($"Успешное добавлениe записи {Employee.DisplayName}, {Employee.PropNames[nameof(employee.EmployeeId)]} = {employee.EmployeeId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка добавления записи {Employee.DisplayName}: {ex.Message}");
        }

        return null;
    }

    public async Task<OperationResult?> ViewEmployees()
    {
        var employeeCount = await _dataLayer.GetEmployeesCount();

        if (employeeCount == 0)
        {
            Console.WriteLine($"Список записей {Employee.DisplayName} пуст");
            Console.WriteLine();
            PressAnyKeyOrEsc();

            return null;
        }

        var windowHeight = _console.WindowHeight;
        var pageSize = _console.MaxTableRowsCount;
        var pagesCount = (employeeCount - 1) / pageSize + 1;

        // Спецификация колонок для вывода
        var spec = Employee.ColumnsSpecification
            .Where(x => !x.Value.Hidden)
            .ToDictionary(
            x => x.Key,
            x => (Width: x.Value.Width, Title: Employee.PropNames[x.Key])
            );

        for (int i = 0; i < pagesCount; i++)
        {
            var employeeList = await _dataLayer.GetEmployees(page: i, pageSize);
            if ((employeeList?.Count ?? 0) > 0)
            {
                var rows = new List<List<object?>>();
                foreach (var emloyee in employeeList!)
                {
                    var rowData = spec
                        .Select(x => emloyee.GetPropertyValue(x.Key) ?? "N/A").ToList();
                    rows.Add(rowData);
                }

                // Отрисовка
                Console.WriteLine($"Все записи {Employee.DisplayName}, количество: {employeeList?.Count}, страница {i + 1} из {pagesCount}");

                _console.PrintTable(rows, spec);

                Console.WriteLine();

                if (i == pagesCount - 1)
                {
                    PressAnyKeyOrEsc();
                    return null;
                }
                else
                {
                    if (PressAnyKeyOrEsc("Нажмите любую клавишу для продолжения или Esc для выхода ..."))
                    {
                        return null;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Список записей {Employee.DisplayName} пуст");
                Console.WriteLine();
                PressAnyKeyOrEsc();

                return null;
            }
        }

        return null;
    }

    public async Task<OperationResult?> UpdateEmployee()
    {
        try
        {
            Console.WriteLine($"Изменение записи {Employee.DisplayName}");
            var employeeId = GetId(Employee.PropNames[nameof(Employee.Props.EmployeeId)]);

            var employee = await _dataLayer.GetEmployeeInfo(employeeId);
            if (employee == null)
            {
                Console.WriteLine($"Запись {Employee.DisplayName} не найдена!");

                return null;
            }

            Console.WriteLine();
            Console.WriteLine($"{Employee.DisplayName}:");

            //Show Employee Info
            foreach (var item in Employee.PropNames)
            {
                Console.WriteLine(value: $"{item.Value}: {employee.GetPropertyValue(item.Key) ?? "Не задано"}");
            }

            // Init menu
            var editFields = Employee.PropNames
                .Where(x => x.Key != nameof(Employee.Props.EmployeeId)).OrderBy(x => x.Key)
                .Select(x => x.Key).ToArray();
            var _submenuOptions = Employee.PropNames
                .Where(x => x.Key != nameof(Employee.Props.EmployeeId)).OrderBy(x => x.Key)
                .Select((x, i) => new MenuData.MenuItem() { ActionId = i + 1, Prompt = x.Value })
                .ToList();
            _submenuOptions.Add(new MenuData.MenuItem()
            {
                ActionId = _submenuOptions.Count + 1,
                Prompt = $"Закончить редактирование {Employee.DisplayName}, {Employee.PropNames[nameof(Employee.Props.EmployeeId)]} = {employeeId}"
            });

            var _submenu = new MenuData()
            {
                MenuTree = _submenuOptions,
                MenuTitle = $"Изменить поле:",
                Prompt = "Выберите поле для изменения: ",
                WrongPrompt = "Неправильный ввод!",
                Hint = _console.HintDefault
            };

            Func<Task<OperationResult?>> doEditField = async () =>
            {
                Console.WriteLine($"Выбрано поле {_submenu.SelectedIndex + 1}: {editFields[_submenu.SelectedIndex]}");
                await UdateEmloyeeField(employeeId, editFields[_submenu.SelectedIndex]);
                return null;
            };

            _submenu.MenuTree[index: 0].Operation = doEditField;
            _submenu.MenuTree[index: 1].Operation = doEditField;
            _submenu.MenuTree[index: 2].Operation = doEditField;
            _submenu.MenuTree[index: 3].Operation = doEditField;
            _submenu.MenuTree[index: 4].Operation = doEditField;

            _submenu.MenuTree[index: 5].Operation = _console.DoMenuExitDefault; // Стандартный выход из меню

            await _console.RunMenuLoop(_submenu);

            Console.WriteLine($"Завершение операции изменения записи {Employee.DisplayName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при изменении записи {Employee.DisplayName}: {ex.Message}");
        }

        return null;
    }

    private async Task UdateEmloyeeField(int employeeId, string fieldName)
    {
        var editField = Enum.Parse<Employee.Props>(fieldName);
        object? newValue = null;

        switch (editField)
        {
            case Employee.Props.FirstName:
                newValue = GetString(Employee.PropNames[fieldName], true, 50);
                break;
            case Employee.Props.LastName:
                newValue = GetString(Employee.PropNames[fieldName], true, 50);
                break;
            case Employee.Props.Email:
                newValue = GetEmail(Employee.PropNames[fieldName], isRequired: false);
                break;
            case Employee.Props.DateOfBirth:
                newValue = GetBirthDate(Employee.PropNames[fieldName], isRequired: false);
                break;
            case Employee.Props.Salary:
                newValue = GetDecimalPositive(Employee.PropNames[fieldName], isRequired: false);
                break;
            default:
                throw new ArgumentOutOfRangeException(fieldName);
        }

        var success = await _dataLayer.UpdateEmployeeById(employeeId, fieldName, newValue);

        if (success)
        {
            Console.WriteLine($"Успешное изменение записи {Employee.DisplayName}, {Employee.PropNames[nameof(Employee.Props.EmployeeId)]} = {employeeId}");
        }
        else
            Console.WriteLine("Ошибка при именении записи!");
    }

    public async Task<OperationResult?> DeleteEmployee()
    {
        try
        {
            Console.WriteLine($"Удаление записи {Employee.DisplayName}");
            var employeeId = GetId(Employee.PropNames[nameof(Employee.Props.EmployeeId)]);

            if (!await _dataLayer.EmployeeExists(employeeId))
            {
                Console.WriteLine($"Запись {Employee.DisplayName} не найдена!");
                return null;
            }

            if (GetConfirmation("Подтвердите удаление нажав \"Y\""))
            {
                var success = await _dataLayer.DeleteEmployeeById(employeeId);

                Console.WriteLine($"Успешное удаление записи {Employee.DisplayName}, {Employee.PropNames[nameof(Employee.Props.EmployeeId)]} = {employeeId} ");
            }

            Console.WriteLine($"Операция отменена");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении записи: {ex.Message}");
        }

        return null;
    }

    public async Task<OperationResult?> ShowAnalytics()
    {
        var salaryCount = await _dataLayer.GetAnalytics();

        Console.WriteLine($"{Employee.DisplayName}: Количество с зарплатой выше среднего: {salaryCount}");

        Console.WriteLine();
        PressAnyKeyOrEsc();

        return null;
    }
}
