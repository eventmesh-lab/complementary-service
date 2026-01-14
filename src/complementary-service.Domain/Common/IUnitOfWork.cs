using System.Threading;
using System.Threading.Tasks;

namespace ComplementaryServices.Domain.Common
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
