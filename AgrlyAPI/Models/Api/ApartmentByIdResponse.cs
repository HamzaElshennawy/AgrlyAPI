using AgrlyAPI.Models.Apartments;

namespace AgrlyAPI.Models.Api;

public class ApartmentByIdResponse
{
	public Apartment? Apartment;
	public List<Reviews>? Reviews = [];
}
