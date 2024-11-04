using AutoMapper;
using BookStoreAPI.Dtos;
using BookStoreAPI.Models;

namespace BookStoreAPI.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<BookDto, Book>();
        }
    }
}
