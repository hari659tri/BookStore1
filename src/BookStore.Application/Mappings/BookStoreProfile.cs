using AutoMapper;
using BookStore.Application.DTOs;
using BookStore.Domain.Entities;

namespace BookStore.Application.Mappings;

public sealed class BookStoreProfile : Profile
{
    public BookStoreProfile()
    {
        CreateMap<Author, AuthorDto>();
        CreateMap<UpsertAuthorRequest, Author>();
        CreateMap<Category, CategoryDto>();
        CreateMap<UpsertCategoryRequest, Category>();

        CreateMap<Book, BookDto>()
            .ForMember(dest => dest.Isbn, opt => opt.MapFrom(src => src.Isbn))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.Name))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
        CreateMap<UpsertBookRequest, Book>()
            .ForMember(dest => dest.Isbn, opt => opt.MapFrom(src => src.Isbn.Trim()));
    }
}
