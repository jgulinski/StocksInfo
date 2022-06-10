namespace Shared.Models;

public class Stock
{
    public int IdStock { get; set; }
    public string TickerSymbol { get; set; }
    public string Name { get; set; }
    public string? PrimaryExchange { get; set; }
    public string? Description { get; set; }
    public string? IndustrialClassification { get; set; }
    public string? HomepageUrl { get; set; }
    public DateTime? ListDate { get; set; }
    public string? ImgUrl { get; set; }

    public virtual ICollection<User> Users { get; set; }

    public Stock()
    {
        Users = new HashSet<User>();
    }
}