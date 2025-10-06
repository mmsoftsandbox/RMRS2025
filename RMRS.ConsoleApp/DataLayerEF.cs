using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RMRS.ConsoleApp.Context;
using RMRS.ConsoleApp.DataModel;

namespace RMRS.ConsoleApp
{
    public class DataLayerEF : IDataLayer
    {
        private EmployeeDBContext _dbContext;

        public DataLayerEF(EmployeeDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task EnsureConnection()
        {
            await _dbContext.Database.EnsureCreatedAsync();

            var connection = _dbContext.Database.GetDbConnection();
            
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
        }

        public async Task<bool> EmployeeExists(int employeeId)
        {
            return await _dbContext.Employees.AnyAsync(x => x.EmployeeId == employeeId);
        }

        public async Task<Employee?> GetEmployeeInfo(int employeeId)
        {
            return await _dbContext.Employees
                .AsNoTracking() // Отсоединяем
                .SingleOrDefaultAsync(x => x.EmployeeId == employeeId);
        }

        public async Task<bool> AddEmployee(Employee employee)
        {
            await _dbContext.Employees.AddAsync(employee);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<int> GetEmployeesCount()
        {
            return await _dbContext.Employees.CountAsync();
        }

        /// <summary>
        /// Список сотрудников.
        /// </summary>
        /// <param name="page">Номер страницы, начиная с 0</param>
        /// <param name="pageSize">Количество строк</param>
        /// <returns></returns>
        public async Task<List<Employee>> GetEmployees(int page, int pageSize)
        {
            return await _dbContext.Employees
                .OrderBy(x => x.EmployeeId)
                .Skip(page * pageSize).Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdateEmployeeById(int employeeId, string fieldName, object? newValue)
        {
            var employee = await _dbContext.Employees.SingleOrDefaultAsync(x => x.EmployeeId == employeeId);
            if (employee == null)
            {
                return false;
            }

            if (!Enum.TryParse(typeof(Employee.Props), fieldName, out var field))
            {
                throw new ArgumentOutOfRangeException(fieldName);
            }

            switch ((Employee.Props)field)
            {
                case Employee.Props.FirstName: employee.FirstName = (string?)newValue; break;
                case Employee.Props.LastName: employee.LastName = (string?)newValue; break;
                case Employee.Props.Email: employee.Email = (string?)newValue; break;
                case Employee.Props.DateOfBirth: employee.DateOfBirth = (DateOnly?)newValue; break;
                case Employee.Props.Salary: employee.Salary = (decimal?)newValue; break;
            }

            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteEmployeeById(int employeeId)
        {
            var employee = await _dbContext.Employees.SingleOrDefaultAsync(x => x.EmployeeId == employeeId);
            if (employee == null)
            {
                return false;
            }

            _dbContext.Employees.Remove(employee);

            return await _dbContext.SaveChangesAsync() > 0;

        }

        public async Task<int> GetAnalytics()
        {
            var count = await _dbContext.Employees
                .Where(x =>
                    x.Salary > _dbContext.Employees.Average(x => x.Salary ?? 0))
                .CountAsync();

            return count;
        }
    }
}