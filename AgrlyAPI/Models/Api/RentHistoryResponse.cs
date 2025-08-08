using AgrlyAPI.Models.Apartments;

namespace AgrlyAPI.Models.Api;

public class RentHistoryResponse
{
	public string UserId { get; set; }
	public List<RentHistory> History { get; set; }
}
