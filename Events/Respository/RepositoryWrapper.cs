
using AutoMapper;
using Events.DATA;
using Events.Interface;

namespace Events.Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        private IUserRepository _user;  
        private IArticleRespository _articles;
        private IPermissionRepository _permission;
        private IRoleRepository _role;
        
        public IRoleRepository Role {  get {
            if(_role == null)
            {
                _role = new RoleRepository(_context,_mapper);
            }
            return _role;
        } }
        
        public IPermissionRepository Permission {  get {
            if(_permission == null)
            {
                _permission = new PermissionRepository(_context,_mapper);
            }
            return _permission;
        } }


        public IArticleRespository Article {  get {
            if(_articles == null)
            {
                _articles = new ArticleRepository(_context,_mapper);
            }
            return _articles;
        } }

        
        public IUserRepository User {  get {
            if(_user == null)
            {
                _user = new UserRepository(_context,_mapper);
            }
            return _user;
        } }

       

        public RepositoryWrapper(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;

        }
    }
}