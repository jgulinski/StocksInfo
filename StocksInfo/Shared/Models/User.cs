namespace Shared.Models;

public class User
{
    public int IdUser { get; set; }
    public string EmailAddress { get; set; }
    
    public string Password { get; set; }
    public virtual ICollection<Stock> Stocks { get; set; }
    public User()
    {
        Stocks = new HashSet<Stock>();
    }
}