using UygApi.Models;

namespace UygApi.Repositories
{
    public class FileModalRepository : GenericRepository<FileModal>
    {
        public FileModalRepository(AppDbContext context) : base(context)
        {
        }
    }
}