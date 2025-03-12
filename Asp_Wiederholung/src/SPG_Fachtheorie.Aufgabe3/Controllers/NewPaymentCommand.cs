using System;

public class NewPaymentCommand
{
    public int CashDeskNumber { get; set; }
    public DateTime PaymentDateTime { get; set; }
    public string PaymentType { get; set; }
    public int EmployeeRegistrationNumber { get; set; }
}
