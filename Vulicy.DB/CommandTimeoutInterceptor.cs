using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Vulicy.Domain;

namespace Vulicy.DB;

/// <summary>
/// Removes the command timeout for queries issued inside a <see cref="DbCommandTimeout.Unlimited"/>
/// scope (import pipeline). Outside such a scope the configured default timeout is left untouched.
/// </summary>
public class CommandTimeoutInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        ApplyTimeout(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        ApplyTimeout(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        ApplyTimeout(command);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyTimeout(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        ApplyTimeout(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        ApplyTimeout(command);
        return ValueTask.FromResult(result);
    }

    private static void ApplyTimeout(DbCommand command)
    {
        if (DbCommandTimeout.IsUnlimited)
            command.CommandTimeout = 0;
    }
}
