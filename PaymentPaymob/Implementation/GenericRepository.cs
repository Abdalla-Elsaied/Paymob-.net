using Microsoft.EntityFrameworkCore;
using PaymentPaymob.Data;
using PaymentPaymob.Interface;
using System.Collections.Generic;

namespace PaymentPaymob.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private protected readonly AppDbContext _dbContext;
        public GenericRepository(AppDbContext dbContext) => _dbContext = dbContext;
        public void Add(T entity) => _dbContext.Set<T>().Add(entity);
        public async Task AddAsync(T entity) => await _dbContext.Set<T>().AddAsync(entity);
        public void Update(T entity) => _dbContext.Set<T>().Attach(entity);
        public void Delete(T entity) => _dbContext.Set<T>().Remove(entity);
        public T? GetById(int id) => _dbContext.Set<T>().Find(id);
        public async Task<T?> GetByIdAsync(int id) => await _dbContext.Set<T>().FindAsync(id);

        public IEnumerable<T> GetAll() => _dbContext.Set<T>().AsNoTracking().ToList();

        //edit by Abdallah
        public async Task<IEnumerable<T>?> GetAllAsync() => await _dbContext.Set<T>().ToListAsync();



    }
}
