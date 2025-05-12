using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace AgrlyAPI.Models.User;

[Table( "transactions" )]
public class Transactions : BaseModel
{
	[PrimaryKey( "id", false )]
	public long Id { get; set; }
	[Column( "transactionid" )]
	public Guid TransactionId { get; set; }
	[Column( "senderid" )]
	public long senderID { get; set; }
	[Column( "receiverid" )]
	public long receiverID { get; set; }
	[Column( "amount" )]
	public float Amount { get; set; }
	[Column( "currency" )]
	public string? Currency { get; set; }
	[Column( "status" )]
	public TransactionStatus? Status { get; set; }
	[Column( "method" )]
	public TransactionMethod? Method { get; set; }
	[Column( "billingID" )]
	public long? BillingID { get; set; }
	[Column( "createdAt" )]
	public DateTime CreatedAt { get; set; }
}
public enum TransactionStatus
{
	Pending,
	Completed,
	Failed
}
public enum TransactionMethod
{
	CreditCard,
	DebitCard,
	BankTransfer,
	Cash,
	VodafoneCash,
	OrangeMoney,
	EtisalatCash
}
