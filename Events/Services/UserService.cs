using AutoMapper;
using AutoMapper.QueryableExtensions;
using e_parliament.Interface;
using Events.DATA;
using Events.DATA.DTOs.User;
using Events.Entities;
using Events.Repository;
using Microsoft.EntityFrameworkCore;

namespace Events.Services
{
    public interface IUserService
    {
        Task<(UserDto? user, string? error)> Login(LoginForm loginForm);
        Task<(AppUser? user, string? error)> DeleteUser(Guid id);
        Task<(UserDto? UserDto, string? error)> Register(RegisterForm registerForm);

        // get all
        Task<(List<UserWithoutTokenDto>? users, int? totalCount, string? error)> GetAllUsers(UserFilter filter);

        // create point of sail 
        Task<(AppUser? user, string? error)> UpdateUser(UpdateUserForm updateUserForm, Guid id);

        Task<(UserDto? user, string? error)> GetUserById(Guid id);
        Task<(bool? state, string? error)> LinkUserToWorkspace(Guid userId);
    }

    public class UserService : IUserService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly ISeatIoService _seatIoService;
        private readonly DataContext _context;

        public UserService(IRepositoryWrapper repositoryWrapper, IMapper mapper, ITokenService tokenService,
            ISeatIoService seatIoService, DataContext context)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _tokenService = tokenService;
            _seatIoService = seatIoService;
            _context = context;
        }

        public async Task<(UserDto? user, string? error)> Login(LoginForm loginForm)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PhoneNumber == loginForm.PhoneNumber && u.Deleted != true);
            if (user == null) return (null, "المستخدم غير موجود");
            if (!BCrypt.Net.BCrypt.Verify(loginForm.Password, user.Password)) return (null, "المستخدم غير موجود");
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Token = _tokenService.CreateToken(userDto, (UserRole)user.Role);
            return (userDto, null);
        }

        public async Task<(AppUser? user, string? error)> DeleteUser(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id && x.Deleted != true);
            if (user == null) return (null, "User not found");
            user.Deleted = true;

            if (user.Role == UserRole.Provider)
            {
                try
                {
                    await _seatIoService.DeleteWorkspaceAsync(user.WorkspacePublicKey!);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return (user, null);
        }

        public async Task<(UserDto? UserDto, string? error)> Register(RegisterForm registerForm)
        {
            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.PhoneNumber == registerForm.PhoneNumber && u.Deleted != true);
                if (user != null) return (null, "User already exists");

                var newUser = new AppUser
                {
                    PhoneNumber = registerForm.PhoneNumber,
                    FullName = registerForm.FullName,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerForm.Password),
                    Role = registerForm.Role
                };


                if (registerForm.Role == UserRole.Provider)
                {
                    var (workspace, error) = await _seatIoService.CreateWorkspaceAsync(newUser.PhoneNumber!);
                    if (error != null) return (null, error);
                    newUser.WorkspacePublicKey = workspace.Key;
                    newUser.WorkspaceSecretKey = workspace.SecretKey;
                }


                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
                // var (workspace , error) = await _seatIoService.CreateWorkspaceAsync(user.Username!);
                // if (error != null) return (null, error);
                // user.WorkspacePublicKey = workspace.Key;
                // user.WorkspaceSecretKey = workspace.SecretKey;
                // await _repositoryWrapper.User.UpdateUser(user);
                // await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                var userDto = _mapper.Map<UserDto>(newUser);
                userDto.Token = _tokenService.CreateToken(userDto, registerForm.Role);
                return (userDto, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await transaction.RollbackAsync();
                return (null, ex.Message);
            }
        }

        public async Task<(List<UserWithoutTokenDto>? users, int? totalCount, string? error)> GetAllUsers(
            UserFilter filter)
        {
            var users = _context.Users.AsNoTracking()
                    .Where(x => filter.Role == null || x.Role == filter.Role)
                    .Where(x => filter.FullName == null || x.FullName.Contains(filter.FullName))
                    .Where(x => filter.PhoneNumber == null || x.PhoneNumber.Contains(filter.PhoneNumber))
                    .Where(x => x.Deleted != true)
                ;

            var totalCount = await users.CountAsync();

            var usersList = await users
                .OrderByDescending(x => x.CreationDate)
                .Skip(filter.PageSize * (filter.PageNumber - 1))
                .Take(filter.PageSize)
                .ProjectTo<UserWithoutTokenDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (usersList, totalCount, null);
        }

        public async Task<(AppUser? user, string? error)> UpdateUser(UpdateUserForm updateUserForm, Guid id)
        {
            var user = await _repositoryWrapper.User.Get(u => u.Id == id && u.Deleted != true);
            if (user == null) return (null, "User not found");
            if (user.PhoneNumber.ToLower() != updateUserForm.PhoneNumber)
            {
                user.PhoneNumber = updateUserForm.PhoneNumber;
                try
                {
                    await _seatIoService.EditWorkspaceAsync(updateUserForm.PhoneNumber, user.WorkspacePublicKey);

                }
                catch (Exception e)
                {
                    // ignored
                }
            }
            if (user.FullName != updateUserForm.FullName) user.FullName = updateUserForm.FullName;
            
            
            await _repositoryWrapper.User.UpdateUser(user);
            return (user, null);
        }

        public async Task<(UserDto? user, string? error)> GetUserById(Guid id)
        {
            var user = await _repositoryWrapper.User.Get(u => u.Id == id && u.Deleted != true);
            if (user == null) return (null, "User not found");
            var userDto = _mapper.Map<UserDto>(user);
            return (userDto, null);
        }

        public async Task<(bool? state, string? error)> LinkUserToWorkspace(Guid userId)
        {
            var user = await _repositoryWrapper.User.Get(u => u.Id == userId && u.Deleted != true);
            if (user == null) return (null, "User not found");
            if (user.WorkspacePublicKey != null) return (false, "User already linked to workspace");

            var (workspace, error) = await _seatIoService.CreateWorkspaceAsync(user.PhoneNumber!);
            if (error != null) return (null, error);
            user.WorkspacePublicKey = workspace.Key;
            user.WorkspaceSecretKey = workspace.SecretKey;
            await _repositoryWrapper.User.UpdateUser(user);
            return (true, null);
        }
    }
}