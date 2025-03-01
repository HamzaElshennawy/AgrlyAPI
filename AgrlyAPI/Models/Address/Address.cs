namespace AgrlyAPI.Models.Addresss;

public class Address
{
	public int Id { get; set; }
	public string Street { get; set; }
	public string City { get; set; }
	public string State { get; set; }
	public string Zip { get; set; }
	public string Country { get; set; }

	public Address()
	{
		Id = 0;
		Street = "";
		City = "";
		State = "";
		Zip = "";
		Country = "";
	}
}
