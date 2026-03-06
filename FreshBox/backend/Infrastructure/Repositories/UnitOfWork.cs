using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Backend.Application.Interfaces;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repos = new();

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repos.ContainsKey(type))
            {
                var repo = new GenericRepository<T>(_context);
                _repos[type] = repo;
            }
            return (IGenericRepository<T>)_repos[type]!;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
