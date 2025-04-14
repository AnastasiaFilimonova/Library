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
        }
    }
}
