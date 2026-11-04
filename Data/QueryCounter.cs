using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfCorePerformanceDemo.Data;

public class QueryCounter : DbCommandInterceptor
{
    public static readonly QueryCounter Instance = new();

    public int Count { get; private set; }

    public void Reset() => Count = 0;

    public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        Count++;
        return result;
    }
}
