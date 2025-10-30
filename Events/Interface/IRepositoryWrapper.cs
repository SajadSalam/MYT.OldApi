using Events.Interface;

namespace Events.Repository
{
    public interface IRepositoryWrapper
    {
   
        IUserRepository User { get; }
        IArticleRespository Article { get; }
        IPermissionRepository Permission { get; }
        
        IRoleRepository Role { get; }
        
        
    }
}