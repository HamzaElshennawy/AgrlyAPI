using AgrlyAPI.Models.Addresss;

namespace AgrlyAPI.Models.Billings;

public class Billing
{
	public int Id { get; set; }
	public string CardHolderName { get; set; }
	public string CardNumber { get; set; }
	public string ExpirationDate { get; set; }
	public string CVV { get; set; }
	public Address BillingAddress { get; set; }
	public string CardType { get; set; }

	public Billing()
	{
		Id = 0;
		CardHolderName = "";
		CardNumber = "";
		ExpirationDate = "";
		CVV = "";
		BillingAddress = new Address();
		CardType = "";
	}
}
