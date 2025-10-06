using System;
using System.Collections.Generic;
using RMRS.ConsoleApp.DataModel;

namespace RMRS.ConsoleApp
{
    public interface IDataLayer
    {
        Task EnsureConnection();

        Task<bool> EmployeeExists(int employeeId);

        Task<Employee?> GetEmployeeInfo(int employeeId);

        Task<bool> AddEmployee(Employee employee);

        Task<int> GetEmployeesCount();

        /// <summary>
        /// Список сотрудников.
        /// </summary>
        /// <param name="page">Номер страницы, начиная с 0</param>
        /// <param name="pageSize">Количество строк</param>
        /// <returns></returns>
        Task<List<Employee>> GetEmployees(int page, int pageSize);

        Task<bool> UpdateEmployeeById(int employeeId, string fieldName, object? newValue);

        Task<bool> DeleteEmployeeById(int employeeId);

        Task<int> GetAnalytics();
    }
}