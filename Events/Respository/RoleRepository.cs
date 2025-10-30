using AutoMapper;
using Events.DATA;
using Events.Interface;
using Role = Events.Entities.Role;

namespace Events.Repository
{
    public class RoleRepository : GenericRepository<Entities.Role,int>, IRoleRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public RoleRepository(DataContext context, IMapper mapper) : base(context,mapper)
        {
            _context = context;
            _mapper = mapper;
        }
    }
}