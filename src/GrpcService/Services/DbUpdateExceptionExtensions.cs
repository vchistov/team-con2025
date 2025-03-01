using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GrpcService.Services;

public static class DbUpdateExceptionExtensions
{
    public static bool IsUniqueIndexViolation(this DbUpdateException ex)
    {
        return ex.InnerException != null && ex.InnerException is SqliteException sql && sql.SqliteErrorCode == 19;
    }
}
