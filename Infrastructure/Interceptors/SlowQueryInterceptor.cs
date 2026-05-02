using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Infrastructure.Interceptors;

public class SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger) : DbCommandInterceptor
{
    private static readonly TimeSpan WarningThreshold = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan ErrorThreshold = TimeSpan.FromMilliseconds(2000);

    public override DbDataReader ReaderExecuted(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        LogIfSlow(command.CommandText, eventData.Duration);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command.CommandText, eventData.Duration);
        return new ValueTask<DbDataReader>(result);
    }

    public override object? ScalarExecuted(
        DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        LogIfSlow(command.CommandText, eventData.Duration);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, object? result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command.CommandText, eventData.Duration);
        return new ValueTask<object?>(result);
    }

    public override int NonQueryExecuted(
        DbCommand command, CommandExecutedEventData eventData, int result)
    {
        LogIfSlow(command.CommandText, eventData.Duration);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command.CommandText, eventData.Duration);
        return new ValueTask<int>(result);
    }

    private void LogIfSlow(string sql, TimeSpan duration)
    {
        if (duration >= ErrorThreshold)
        {
            logger.LogError(
                "Very slow EF Core query detected ({DurationMs}ms): {Sql}",
                (int)duration.TotalMilliseconds, sql);
        }
        else if (duration >= WarningThreshold)
        {
            logger.LogWarning(
                "Slow EF Core query detected ({DurationMs}ms): {Sql}",
                (int)duration.TotalMilliseconds, sql);
        }
    }
}
