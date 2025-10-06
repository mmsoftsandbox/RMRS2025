using System;
using System.Collections.Generic;
using RMRS.ConsoleApp.DataModel;

namespace RMRS.ConsoleApp
{
    /// <summary>
    /// Интерфейс работы в данными для уровня бинес-логики.
    /// Может быть реализован испольованием различных подходов.
    /// Например, на EF, с помошью иной ORM, непосредственными запросами к БД
    /// или же вызовами хранимых процедур
    /// </summary>
    public interface IDataLayer
    {
        /// <summary>
        /// Проверить соединение
        /// </summary>
        /// <returns></returns>
        Task EnsureConnection();

        /// <summary>
        /// Проверка существования пользователя
        /// </summary>
        /// <param name="employeeId">Id сотрудника</param>
        /// <returns></returns>
        Task<bool> EmployeeExists(int employeeId);

        /// <summary>
        /// Информация о сотруднике
        /// </summary>
        /// <param name="employeeId">Id сотрудника</param>
        /// <returns></returns>
        Task<Employee?> GetEmployeeInfo(int employeeId);

        /// <summary>
        /// Добавить сотрудника
        /// </summary>
        /// <param name="employee">сотрудник</param>
        /// <returns></returns>
        Task<bool> AddEmployee(Employee employee);

        /// <summary>
        /// Получить список сотрудников
        /// </summary>
        /// <param name="employee">сотрудник</param>
        /// <returns></returns>
        Task<int> GetEmployeesCount();

        /// <summary>
        /// Список сотрудников
        /// </summary>
        /// <param name="page">Номер страницы, начиная с 0</param>
        /// <param name="pageSize">Количество строк</param>
        /// <returns></returns>
        Task<List<Employee>> GetEmployees(int page, int pageSize);

        /// <summary>
        /// Обновить данные сотрудника по его Id
        /// </summary>
        /// <param name="employeeId">Id сотрудника</param>
        /// <param name="fieldName">Поле для изменения</param>
        /// <param name="newValue">Новое значение</param>
        /// <returns></returns>
        Task<bool> UpdateEmployeeById(int employeeId, string fieldName, object? newValue);

        /// <summary>
        /// Удалить сотрудника по его Id
        /// </summary>
        /// <param name="employeeId">Id сотрудника</param>
        /// <returns></returns>
        Task<bool> DeleteEmployeeById(int employeeId);

        /// <summary>
        /// Получить аналитическую информацию по сотрудникам
        /// </summary>
        /// <returns>начение в соответствии с целевой логикой</returns>
        Task<int> GetAnalytics();
    }
}