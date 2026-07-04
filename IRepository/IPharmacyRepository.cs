using Midix.Models;

namespace Midix.Repositories
{
    public interface IPharmacyRepository
    {
        Task<IEnumerable<Pharmacy>> GetAllAsync();

        /// <summary>
        /// Returns all pharmacies when searchTerm is null/empty, otherwise filters
        /// by name or address (case-insensitive, contains match).
        /// </summary>
        Task<IEnumerable<Pharmacy>> SearchAsync(string? searchTerm);

        Task<Pharmacy?> GetByIdAsync(int id);

        Task AddAsync(Pharmacy pharmacy);

        Task UpdateAsync(Pharmacy pharmacy);

        Task DeleteAsync(int id);
    }
}
