using Perfect.Domain.Enums;

namespace Perfect.Application.Services;

public interface IInvoiceStatusCalculator
{
    DateTime ResolveDueDate(DateTime invoiceDate, PaymentType paymentType, int creditDaysApplied);
    InvoiceStatus ResolveStatus(decimal balance, DateTime dueDate, DateTime now, int thresholdDays);
}

public class InvoiceStatusCalculator : IInvoiceStatusCalculator
{
    public DateTime ResolveDueDate(DateTime invoiceDate, PaymentType paymentType, int creditDaysApplied)
    {
        if (paymentType == PaymentType.Cash)
        {
            return invoiceDate;
        }

        return invoiceDate.AddDays(creditDaysApplied);
    }

    public InvoiceStatus ResolveStatus(decimal balance, DateTime dueDate, DateTime now, int thresholdDays)
    {
        if (balance <= 0)
        {
            return InvoiceStatus.Paid;
        }

        if (now.Date > dueDate.Date)
        {
            return InvoiceStatus.Overdue;
        }

        var daysToDue = (dueDate.Date - now.Date).Days;
        if (daysToDue <= thresholdDays)
        {
            return InvoiceStatus.DueSoon;
        }

        return InvoiceStatus.Pending;
    }
}
