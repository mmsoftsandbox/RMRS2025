using Microsoft.Data.SqlClient;
using RMRS.ConsoleApp.DataModel;

namespace RMRS.ConsoleApp
{
    public class DataLayerSql : IDataLayer
    {
        private SqlConnection _connection { get; set; }

        public DataLayerSql(SqlConnection connection)
        {
            _connection = connection;
        }

        public async Task EnsureConnection()
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        private object GetValueOrDBNull(object? val)
        {
            return val ?? DBNull.Value;
        }

        /// <summary>
        /// Считать Nullable значение поля таблицы
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fieldName">Поле таблицы</param>
        /// <returns>Nullable значение</returns>
        private object? GetValue(SqlDataReader reader, string fieldName)
        {
            // @@@ Кешировать Ordinals
            return reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader[fieldName];
        }

        /// <summary>
        /// Конвертировать Nullable DateTime значение поля таблицы в DateOnly?
        /// </summary>
        /// <param name="dateTime">Nullable DateTime значение поля таблицы</param>
        /// <returns>DateOnly?</returns>
        private DateOnly? GetDateOnly(object? dateTime)
        {
            return dateTime == null ? null : DateOnly.FromDateTime((DateTime)dateTime);
        }

        public async Task<bool> EmployeeExists(int employeeId)
        {
            await EnsureConnection();

            var query = "SELECT COUNT(*) FROM Employees WHERE EmployeeID = @EmployeeID";

            using (var command = new SqlCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@EmployeeID", employeeId);

                return (int)(await command.ExecuteScalarAsync() ?? 0) > 0;
            }
        }

        public async Task<Employee?> GetEmployeeInfo(int employeeId)
        {
            await EnsureConnection();

            var query = "SELECT * FROM Employees WHERE EmployeeID = @EmployeeID";

            using (var command = new SqlCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@EmployeeID", employeeId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var employees = new List<Employee>();
                    if (!reader.HasRows)
                    {
                        return null;
                    }

                    reader.Read();
                    var employee = new Employee()
                    {
                        EmployeeId = (int)reader["EmployeeID"],
                        FirstName = (string?)GetValue(reader, "FirstName"),
                        LastName = (string?)GetValue(reader, "LastName"),
                        DateOfBirth = GetDateOnly(GetValue(reader, "DateOfBirth")),
                        Email = (string?)GetValue(reader, "Email"),
                        Salary = (decimal?)GetValue(reader, "Salary")

                    };

                    return employee;
                }
            }
        }

        public async Task<bool> AddEmployee(Employee employee)
        {
            await EnsureConnection();

            var query = @"INSERT INTO Employees 
                                (FirstName, LastName, Email, DateOfBirth, Salary) 
                                VALUES (@FirstName, @LastName, @Email, @DateOfBirth, @Salary)";

            using (var command = new SqlCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@FirstName", GetValueOrDBNull(employee.FirstName));
                command.Parameters.AddWithValue("@LastName", GetValueOrDBNull(employee.LastName));
                command.Parameters.AddWithValue("@Email", GetValueOrDBNull(employee.Email));
                command.Parameters.AddWithValue("@DateOfBirth", GetValueOrDBNull(employee.DateOfBirth));
                command.Parameters.AddWithValue("@Salary", GetValueOrDBNull(employee.Salary));

                return await command.ExecuteNonQueryAsync() > 0;
            }
        }
        public async Task<int> GetEmployeesCount()
        {
            await EnsureConnection();

            var query = "SELECT COUNT(*) FROM Employees";

            using (var command = new SqlCommand(query, _connection))
            {
                return (int)(await command.ExecuteScalarAsync() ?? 0); // @@@
            }
        }

        /// <summary>
        /// Список сотрудников.
        /// </summary>
        /// <param name="page">Номер страницы, начиная с 0</param>
        /// <param name="pageSize">Количество строк</param>
        /// <returns></returns>
        public async Task<List<Employee>> GetEmployees(int page, int pageSize)
        {
            await EnsureConnection();

            // NOTE:
            // Starting with SQL SERVER 2012, you can use the OFFSET FETCH Clause
            // OFFSET can only be used with or in tandem to ORDER BY.
            var query = $"SELECT * FROM Employees ORDER BY EmployeeID OFFSET {page * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            using (var command = new SqlCommand(query, _connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                var employees = new List<Employee>();
                if (!reader.HasRows)
                {
                    return employees;
                }

                while (reader.Read())
                {
                    var employee = new Employee()
                    {
                        EmployeeId = (int)reader["EmployeeID"],
                        FirstName = (string?)GetValue(reader, "FirstName"),
                        LastName = (string?)GetValue(reader, "LastName"),
                        DateOfBirth = GetDateOnly(GetValue(reader, "DateOfBirth")),
                        Email = (string?)GetValue(reader, "Email"),
                        Salary = (decimal?)GetValue(reader,"Salary")
                    };

                    employees.Add(employee);
                }

                return employees;
            }
        }

        public async Task<bool> UpdateEmployeeById(int employeeId, string fieldName, object? newValue)
        {
            await EnsureConnection();

            var query = $"UPDATE Employees SET {fieldName} = @Value WHERE EmployeeID = @EmployeeID";

            using (var command = new SqlCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@Value", GetValueOrDBNull(newValue));
                command.Parameters.AddWithValue("@EmployeeID", employeeId);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteEmployeeById(int employeeId)
        {
            await EnsureConnection();

            var query = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";

            using (var command = new SqlCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@EmployeeID", employeeId);
                int rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }

        public async Task<int> GetAnalytics()
        {
            await EnsureConnection();

            var query = @"
                        SELECT COUNT(*) as Count 
                        FROM Employees 
                        WHERE Salary > (SELECT AVG(COALESCE(Salary, 0)) FROM Employees)";

            using (var command = new SqlCommand(query, _connection))
            {
                var count = await command.ExecuteScalarAsync();
                return (int)(count ?? 0);
            }
        }
    }

}

