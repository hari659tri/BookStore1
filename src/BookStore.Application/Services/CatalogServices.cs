using AutoMapper;
using AutoMapper.QueryableExtensions;
using BookStore.Application.Common;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Services;

public sealed class BookService : IBookService
{
    private readonly IRepository<Book> _books;
    private readonly IRepository<Author> _authors;
    private readonly IRepository<Category> _categories;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public BookService(IRepository<Book> books, IRepository<Author> authors, IRepository<Category> categories, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _books = books;
        _authors = authors;
        _categories = categories;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<BookDto>> GetAsync(BookQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _books.Query().AsNoTracking().Where(book => book.IsActive);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(book =>
                book.Title.Contains(search) ||
                book.Isbn.Contains(search) ||
                book.Author.Name.Contains(search) ||
                book.Category.Name.Contains(search));
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(book => book.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.AuthorId.HasValue)
        {
            query = query.Where(book => book.AuthorId == parameters.AuthorId.Value);
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(book => book.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(book => book.Price <= parameters.MaxPrice.Value);
        }

        query = (parameters.SortBy?.ToLowerInvariant(), parameters.Desc) switch
        {
            ("price", true) => query.OrderByDescending(book => book.Price),
            ("price", false) => query.OrderBy(book => book.Price),
            ("title", true) => query.OrderByDescending(book => book.Title),
            ("title", false) => query.OrderBy(book => book.Title),
            ("createdat", true) => query.OrderByDescending(book => book.CreatedAt),
            _ => query.OrderBy(book => book.Title)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ProjectTo<BookDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<BookDto>(items, parameters.PageNumber, parameters.PageSize, total);
    }

    public async Task<BookDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var dto = await _books.Query().AsNoTracking()
            .Where(book => book.Id == id && book.IsActive)
            .ProjectTo<BookDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new AppException("Book not found.", 404);
    }

    public async Task<BookDto> CreateAsync(UpsertBookRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCatalogReferencesAsync(request.AuthorId, request.CategoryId, cancellationToken);

        if (await _books.Query().AnyAsync(book => book.Isbn == request.Isbn.Trim(), cancellationToken))
        {
            throw new AppException("A book with this ISBN already exists.", 409);
        }

        var book = _mapper.Map<Book>(request);
        book.CreatedAt = DateTime.UtcNow;
        await _books.AddAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(book.Id, cancellationToken);
    }

    public async Task<BookDto> UpdateAsync(int id, UpsertBookRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCatalogReferencesAsync(request.AuthorId, request.CategoryId, cancellationToken);

        var book = await _books.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Book not found.", 404);

        var normalizedIsbn = request.Isbn.Trim();
        if (await _books.Query().AnyAsync(existing => existing.Id != id && existing.Isbn == normalizedIsbn, cancellationToken))
        {
            throw new AppException("A book with this ISBN already exists.", 409);
        }

        _mapper.Map(request, book);
        book.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(book.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _books.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Book not found.", 404);

        book.IsActive = false;
        book.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCatalogReferencesAsync(int authorId, int categoryId, CancellationToken cancellationToken)
    {
        if (!await _authors.Query().AnyAsync(author => author.Id == authorId && author.IsActive, cancellationToken))
        {
            throw new AppException("Author not found.", 404);
        }

        if (!await _categories.Query().AnyAsync(category => category.Id == categoryId && category.IsActive, cancellationToken))
        {
            throw new AppException("Category not found.", 404);
        }
    }
}

public sealed class AuthorService : IAuthorService
{
    private readonly IRepository<Author> _authors;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public AuthorService(IRepository<Author> authors, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _authors = authors;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AuthorDto>> GetAsync(CancellationToken cancellationToken = default) =>
        await _authors.Query().AsNoTracking().OrderBy(author => author.Name)
            .ProjectTo<AuthorDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

    public async Task<AuthorDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var author = await _authors.Query().AsNoTracking().Where(x => x.Id == id)
            .ProjectTo<AuthorDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        return author ?? throw new AppException("Author not found.", 404);
    }

    public async Task<AuthorDto> CreateAsync(UpsertAuthorRequest request, CancellationToken cancellationToken = default)
    {
        var author = _mapper.Map<Author>(request);
        author.CreatedAt = DateTime.UtcNow;
        await _authors.AddAsync(author, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(author.Id, cancellationToken);
    }

    public async Task<AuthorDto> UpdateAsync(int id, UpsertAuthorRequest request, CancellationToken cancellationToken = default)
    {
        var author = await _authors.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Author not found.", 404);
        _mapper.Map(request, author);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(author.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var author = await _authors.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Author not found.", 404);
        author.IsActive = false;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categories;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IRepository<Category> categories, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _categories = categories;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAsync(CancellationToken cancellationToken = default) =>
        await _categories.Query().AsNoTracking().OrderBy(category => category.Name)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

    public async Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categories.Query().AsNoTracking().Where(x => x.Id == id)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        return category ?? throw new AppException("Category not found.", 404);
    }

    public async Task<CategoryDto> CreateAsync(UpsertCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (await _categories.Query().AnyAsync(category => category.Name == request.Name.Trim(), cancellationToken))
        {
            throw new AppException("A category with this name already exists.", 409);
        }

        var category = _mapper.Map<Category>(request);
        await _categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(category.Id, cancellationToken);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpsertCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Category not found.", 404);

        if (await _categories.Query().AnyAsync(existing => existing.Id != id && existing.Name == request.Name.Trim(), cancellationToken))
        {
            throw new AppException("A category with this name already exists.", 409);
        }

        _mapper.Map(request, category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(category.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Category not found.", 404);
        category.IsActive = false;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
