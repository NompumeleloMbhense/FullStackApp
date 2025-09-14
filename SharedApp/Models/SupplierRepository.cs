
using Microsoft.EntityFrameworkCore;
using SharedApp.Data;

namespace SharedApp.Models
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers.Include(s => s.Products).ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Suppliers.Include(s => s.Products)
                            .FirstOrDefaultAsync(s => s.SupplierId == id);
        }

        public async Task AddAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.Include(s => s.Products)
                                             .FirstOrDefaultAsync(s => s.SupplierId == id);

            if (supplier is not null)
            {
                foreach (var product in supplier.Products)
                {
                    product.SupplierId = 0;
                }

                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
            }
        }

    }
}