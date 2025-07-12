using AgrlyAPI.Models.Apartments;

namespace AgrlyAPI.Models.Api
{
	public class AvailableApartmentsResponse
	{
		public int StatusCode { get; set; }
		public int CurrentPage { get; set; }
		public List<Apartment> Apartments { get; set; } = new List<Apartment>();
	}
}
