using Perfect.Application.Common;

namespace Perfect.Infrastructure.Security;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
