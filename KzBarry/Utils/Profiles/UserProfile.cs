using AutoMapper;
using KzBarry.Models.DTOs.Auth;
using KzBarry.Models.DTOs.Users;
using KzBarry.Models.Entities;

namespace KzBarry.Utils.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile() {
            CreateMap<UserCreateDto, User>();

            CreateMap<UserUpdateDto, User>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); // Ignore null fields

            CreateMap<User, UserDto>()
                .ReverseMap()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<RegisterRequest, UserCreateDto>();
        }

    }
}
