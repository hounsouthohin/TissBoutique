namespace ECommerce.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        ICartRepository Carts { get; }

        IGenericRepository<T> Repository<T>() where T : class;

        Task<int> SaveChangesAsync();
        Task<int> CompleteAsync(); // ✅ ALIAS pour SaveChangesAsync

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}