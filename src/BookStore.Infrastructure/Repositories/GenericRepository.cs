using System.Linq.Expressions;
using BookStore.Application.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories;

public sealed class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly BookStoreDbContext _dbContext;

    public GenericRepository(BookStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<T> Query() => _dbContext.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<T>().FindAsync([id], cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);

    public void Remove(T entity) => _dbContext.Set<T>().Remove(entity);
}
