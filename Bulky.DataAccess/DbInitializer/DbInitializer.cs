using Bulky.DataAccess.Data;

namespace Bulky.DataAccess.DbInitializer;

public class DbInitializer
{
    private readonly ApplicationDbContext _context;

    public DbInitializer(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Initialize()
    {
        try
        {

        } catch (Exception ex) { }

        return;
    }
}
