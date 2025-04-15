using Entities.DataTransferObject;
using Library.Models;
using AutoMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Library
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Book, BookDTO>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.AuthorName))
            .ForMember(dest => dest.GenreName, opt => opt.MapFrom(src => src.Genre.GenreName));

            CreateMap<BookDTO, Book>()
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.Genre, opt => opt.Ignore());

        }
    }
}
