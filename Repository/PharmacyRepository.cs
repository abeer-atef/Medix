using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Models;

namespace Midix.Repositories
{
    public class PharmacyRepository : IPharmacyRepository
    {
        private readonly ApplicationDbContext _context;

        public PharmacyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pharmacy>> GetAllAsync()
        {
            return await _context.Pharmacies
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pharmacy>> SearchAsync(string? searchTerm)
        {
            var query = _context.Pharmacies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Address.Contains(searchTerm));
            }

            return await query.ToListAsync();
        }

        public async Task<Pharmacy?> GetByIdAsync(int id)
        {
            return await _context.Pharmacies.FindAsync(id);
        }

        public async Task AddAsync(Pharmacy pharmacy)
        {
            _context.Pharmacies.Add(pharmacy);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pharmacy pharmacy)
        {
            _context.Pharmacies.Update(pharmacy);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var pharmacy = await _context.Pharmacies.FindAsync(id);
            if (pharmacy != null)
            {
                _context.Pharmacies.Remove(pharmacy);
                await _context.SaveChangesAsync();
            }
        }
    }
}