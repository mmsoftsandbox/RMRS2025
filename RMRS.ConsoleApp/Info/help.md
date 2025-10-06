# Generate DB Context
## Пароли не храним в репозитории

Scaffold-DbContext "Data Source=localhost;Initial Catalog=EmployeeDB;Integrated Security=True;Trust Server Certificate=True" Microsoft.EntityFrameworkCore.SqlServer  -StartupProject RMRS.Console -OutputDir DataModel -Context EmployeeDBContext -ContextDir Context -Namespace RMRS.Console.DataModel -ContextNamespace RMRS.Console.Context -Force -Verbose