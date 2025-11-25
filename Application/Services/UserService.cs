using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Create;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;
using IceBreakerApp.Application.IServices;
using IceBreakerApp.Domain.IRepositories;
using IceBreakerApp.Domain.Models;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace IceBreakerApp.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger, IMapper mapper)
    {
        _userRepository = userRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<UserResponseDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user != null ? _mapper.Map<UserResponseDTO>(user) : null;
    }

    public async Task<PaginatedResult<UserResponseDTO>> GetAllAsync(
        int pageNumber, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetPageAsync(pageNumber, pageSize, cancellationToken);
        var userDTOs = _mapper.Map<List<UserResponseDTO>>(users);
        
        return new PaginatedResult<UserResponseDTO>(userDTOs, userDTOs.Count, pageNumber, pageSize);
    }

    public async Task<UserResponseDTO> CreateAsync(CreateUserDTO createDto, CancellationToken cancellationToken = default)
    {
        var user = _mapper.Map<User>(createDto);

        await _userRepository.AddAsync(user, cancellationToken);
        
        _logger.LogInformation("User created: {UserId}", user.Id);
        
        return _mapper.Map<UserResponseDTO>(user);
    }

    public async Task UpdateAsync(Guid id, UpdateUserDTO updateDto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException();

        if (!string.IsNullOrWhiteSpace(updateDto.DisplayName))
            user.DisplayName = updateDto.DisplayName;
            
        if (!string.IsNullOrWhiteSpace(updateDto.Bio))
            user.Bio = updateDto.Bio;


        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _userRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("User deleted: {UserId}", id);
    }

    public async Task<UserResponseDTO?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
        return user != null ? _mapper.Map<UserResponseDTO>(user) : null;
    }

    public async Task<UserResponseDTO?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByUsernameAsync(username, cancellationToken);
        return user != null ? _mapper.Map<UserResponseDTO>(user) : null;
    }
}