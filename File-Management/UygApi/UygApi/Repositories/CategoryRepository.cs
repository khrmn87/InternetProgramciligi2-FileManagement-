using UygApi.Models;

namespace UygApi.Repositories
{
    public class CategoryRepository : GenericRepository<Category>
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
