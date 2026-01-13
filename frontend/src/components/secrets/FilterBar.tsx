import { Search, Star, ArrowUpDown, X } from 'lucide-react';
import { SecretQueryParams } from '../../types/secret.types';

interface FilterBarProps {
  filters: SecretQueryParams;
  onFilterChange: (filters: SecretQueryParams) => void;
  categories: string[];
}

export function FilterBar({ filters, onFilterChange, categories }: FilterBarProps) {
  const sortOptions = [
    { value: 'createdAt', label: 'Date Created' },
    { value: 'updatedAt', label: 'Date Updated' },
    { value: 'websiteUrl', label: 'Website' },
    { value: 'username', label: 'Username' },
    { value: 'category', label: 'Category' },
  ];

  const hasActiveFilters = 
    filters.searchTerm || 
    filters.category || 
    filters.isFavorite !== undefined;

  const clearFilters = () => {
    onFilterChange({
      ...filters,
      searchTerm: undefined,
      category: undefined,
      isFavorite: undefined,
    });
  };

  return (
    <div className="flex flex-wrap items-center gap-3 mb-6">
      {/* Search */}
      <div className="relative flex-1 min-w-[200px] max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-dark-400" />
        <input
          type="text"
          placeholder="Search secrets..."
          value={filters.searchTerm || ''}
          onChange={(e) => onFilterChange({ ...filters, searchTerm: e.target.value || undefined, pageNumber: 1 })}
          className="w-full pl-10 pr-4 py-2.5 bg-dark-800 border border-dark-600 rounded-xl
                    text-dark-100 placeholder-dark-400 transition-all duration-200
                    focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20"
        />
      </div>

      {/* Category filter */}
      {categories.length > 0 && (
        <select
          value={filters.category || ''}
          onChange={(e) => onFilterChange({ ...filters, category: e.target.value || undefined, pageNumber: 1 })}
          className="px-4 py-2.5 bg-dark-800 border border-dark-600 rounded-xl
                    text-dark-100 transition-all duration-200 cursor-pointer
                    focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20"
        >
          <option value="">All Categories</option>
          {categories.map((cat) => (
            <option key={cat} value={cat}>{cat}</option>
          ))}
        </select>
      )}

      {/* Favorite filter */}
      <button
        onClick={() => onFilterChange({ 
          ...filters, 
          isFavorite: filters.isFavorite === true ? undefined : true,
          pageNumber: 1 
        })}
        className={`flex items-center gap-2 px-4 py-2.5 rounded-xl border transition-colors
                   ${filters.isFavorite === true
                     ? 'bg-yellow-500/10 border-yellow-500/50 text-yellow-500'
                     : 'bg-dark-800 border-dark-600 text-dark-400 hover:border-dark-500'
                   }`}
      >
        <Star className={`w-4 h-4 ${filters.isFavorite === true ? 'fill-yellow-500' : ''}`} />
        <span className="font-medium">Favorites</span>
      </button>

      {/* Sort */}
      <div className="flex items-center gap-2">
        <select
          value={filters.sortBy || 'createdAt'}
          onChange={(e) => onFilterChange({ ...filters, sortBy: e.target.value })}
          className="px-4 py-2.5 bg-dark-800 border border-dark-600 rounded-xl
                    text-dark-100 transition-all duration-200 cursor-pointer
                    focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20"
        >
          {sortOptions.map((opt) => (
            <option key={opt.value} value={opt.value}>{opt.label}</option>
          ))}
        </select>

        <button
          onClick={() => onFilterChange({ ...filters, sortDescending: !filters.sortDescending })}
          className={`p-2.5 rounded-xl border transition-colors
                     ${filters.sortDescending
                       ? 'bg-primary-500/10 border-primary-500/50 text-primary-400'
                       : 'bg-dark-800 border-dark-600 text-dark-400 hover:border-dark-500'
                     }`}
          title={filters.sortDescending ? 'Descending' : 'Ascending'}
        >
          <ArrowUpDown className={`w-4 h-4 ${filters.sortDescending ? 'rotate-180' : ''} transition-transform`} />
        </button>
      </div>

      {/* Clear filters */}
      {hasActiveFilters && (
        <button
          onClick={clearFilters}
          className="flex items-center gap-1.5 px-3 py-2 text-sm text-dark-400 
                    hover:text-dark-100 transition-colors"
        >
          <X className="w-4 h-4" />
          Clear filters
        </button>
      )}
    </div>
  );
}
