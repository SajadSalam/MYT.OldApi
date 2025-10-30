using System.Linq.Expressions;
using Events.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace Events.Interface
{
    public interface IUserRepository : IGenericRepository<AppUser,Guid>
    {
        Task<AppUser> CreateUser(AppUser user);
        
        Task<(List<AppUser> data, int totalCount)> GetUsers(
        int pageNumber = 0
        );

     
        Task<(List<AppUser> data, int totalCount)> GetUsers(Expression<Func<AppUser, bool>> predicate,
        Func<IQueryable<AppUser>, IIncludableQueryable<AppUser, object>> include,
        int pageNumber = 0
        );
        
        Task<AppUser> UpdateUser(AppUser user);
        Task<AppUser> DeleteUser(AppUser user);
    }
}