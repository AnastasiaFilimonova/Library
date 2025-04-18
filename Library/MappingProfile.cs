using Entities.DataTransferObject;
using Library.Models;
using AutoMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Entities.Models;

namespace Library
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Book, BookListDTO>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.AuthorName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.ReadingStatus != null && src.ReadingStatus.Status == 1 ? "Прочитана" : "Не прочитана"));

            CreateMap<Book, BookDetailsDTO>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.AuthorName))
                .ForMember(dest => dest.GenreName, opt => opt.MapFrom(src => src.Genre.GenreName))
                .ForMember(dest => dest.ReadingStatusName, opt => opt.MapFrom(src =>
                    src.ReadingStatus != null && src.ReadingStatus.Status == 1 ? "Прочитана" : "Не прочитана"))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.ReadingStatus.Rating))
                .ForMember(dest => dest.Review, opt => opt.MapFrom(src => src.ReadingStatus.Review))
                .ForMember(dest => dest.Quotes, opt => opt.MapFrom(src => src.ReadingStatus.Quotes))
                .ForMember(dest => dest.StartReadingDate, opt => opt.MapFrom(src => src.ReadingStatus.StartReadingDate))
                .ForMember(dest => dest.EndReadingDate, opt => opt.MapFrom(src => src.ReadingStatus.EndReadingDate));

            CreateMap<BookDTO, Book>();
            CreateMap<BookUpdateDTO, ReadingStatus>()
                .ForAllMembers(opt => opt.Condition((src, context, srcMember) => srcMember != null));
            CreateMap<BookUpdateDTO, ReadingStatus>()
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


        }
    }
}
