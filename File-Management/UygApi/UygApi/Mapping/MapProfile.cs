using AutoMapper;
using UygApi.DTOs; 
using UygApi.Models;


namespace UygApi.Mapping
{
    public class MapProfile: Profile
    {
        public MapProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<FileModal, FileModalDto>().ReverseMap();
            CreateMap<CategoryDto, Category>().ReverseMap();
            CreateMap<FileModalDto, FileModal>().ReverseMap();
            CreateMap<AppUser, UserDto>().ReverseMap();
            CreateMap<Starred, StarredDto>()
    .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.File.FileName))
    .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.UserName));
        }
    }
}
