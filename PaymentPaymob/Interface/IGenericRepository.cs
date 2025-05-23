using System.Collections.Generic;

namespace PaymentPaymob.Interface
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        Task<IEnumerable<T>?> GetAllAsync();
        T? GetById(int id);
        Task<T?> GetByIdAsync(int id);
        // return int >> return number of row effected
        void Add(T entity);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

     
    }
}
