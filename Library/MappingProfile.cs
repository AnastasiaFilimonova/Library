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
            CreateMap<Book, BookDTO>();
            CreateMap<BookDTO, Book>()
                .ForMember(dest => dest.AuthorID, opt => opt.Ignore())
                .ForMember(dest => dest.GenreID, opt => opt.Ignore());
        }
    }
}
