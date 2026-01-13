import { useState, useEffect, useCallback, useMemo } from 'react';
import { Plus, KeyRound, AlertTriangle } from 'lucide-react';
import { secretsApi } from '../api/secretsApi';
import { Secret, SecretListResponse, SecretQueryParams, CreateSecretRequest, UpdateSecretRequest } from '../types/secret.types';
import { SecretCard } from '../components/secrets/SecretCard';
import { SecretForm } from '../components/secrets/SecretForm';
import { FilterBar } from '../components/secrets/FilterBar';
import { Button } from '../components/common/Button';
import { Modal } from '../components/common/Modal';
import { Pagination } from '../components/common/Pagination';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import toast from 'react-hot-toast';

export function SecretsPage() {
  const [secrets, setSecrets] = useState<SecretListResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [filters, setFilters] = useState<SecretQueryParams>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdAt',
    sortDescending: true,
  });

  // Modal states
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [selectedSecret, setSelectedSecret] = useState<Secret | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Extract unique categories
  const categories = useMemo(() => {
    if (!secrets) return [];
    const cats = secrets.items
      .map((s) => s.category)
      .filter((c): c is string => !!c);
    return [...new Set(cats)].sort();
  }, [secrets]);

  const loadSecrets = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await secretsApi.getSecrets(filters);
      setSecrets(data);
    } catch (error) {
      console.error('Failed to load secrets:', error);
      toast.error('Failed to load secrets');
    } finally {
      setIsLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    loadSecrets();
  }, [loadSecrets]);

  const handleFilterChange = (newFilters: SecretQueryParams) => {
    setFilters(newFilters);
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, pageNumber: page }));
  };

  const handleOpenCreate = () => {
    setSelectedSecret(null);
    setIsFormModalOpen(true);
  };

  const handleEdit = (secret: Secret) => {
    setSelectedSecret(secret);
    setIsFormModalOpen(true);
  };

  const handleDeleteClick = (secret: Secret) => {
    setSelectedSecret(secret);
    setIsDeleteModalOpen(true);
  };

  const handleFormSubmit = async (data: CreateSecretRequest | UpdateSecretRequest) => {
    setIsSubmitting(true);
    try {
      if (selectedSecret) {
        await secretsApi.updateSecret(selectedSecret.id, data as UpdateSecretRequest);
        toast.success('Secret updated successfully');
      } else {
        await secretsApi.createSecret(data as CreateSecretRequest);
        toast.success('Secret created successfully');
      }
      setIsFormModalOpen(false);
      setSelectedSecret(null);
      loadSecrets();
    } catch (error) {
      console.error('Failed to save secret:', error);
      toast.error('Failed to save secret');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedSecret) return;

    setIsSubmitting(true);
    try {
      await secretsApi.deleteSecret(selectedSecret.id);
      toast.success('Secret deleted successfully');
      setIsDeleteModalOpen(false);
      setSelectedSecret(null);
      loadSecrets();
    } catch (error) {
      console.error('Failed to delete secret:', error);
      toast.error('Failed to delete secret');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="animate-fade-in">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h1 className="text-3xl font-bold text-dark-100 mb-2">My Secrets</h1>
          <p className="text-dark-400">
            {secrets ? `${secrets.totalCount} secrets stored securely` : 'Loading...'}
          </p>
        </div>
        <Button leftIcon={<Plus size={18} />} onClick={handleOpenCreate}>
          Add Secret
        </Button>
      </div>

      {/* Filters */}
      <FilterBar
        filters={filters}
        onFilterChange={handleFilterChange}
        categories={categories}
      />

      {/* Secrets list */}
      {isLoading ? (
        <div className="flex justify-center py-16">
          <LoadingSpinner size="lg" />
        </div>
      ) : secrets && secrets.items.length > 0 ? (
        <>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-8">
            {secrets.items.map((secret, index) => (
              <div
                key={secret.id}
                className="animate-slide-up"
                style={{ animationDelay: `${index * 50}ms` }}
              >
                <SecretCard
                  secret={secret}
                  onEdit={handleEdit}
                  onDelete={handleDeleteClick}
                />
              </div>
            ))}
          </div>

          <Pagination
            currentPage={secrets.pageNumber}
            totalPages={secrets.totalPages}
            hasNextPage={secrets.hasNextPage}
            hasPreviousPage={secrets.hasPreviousPage}
            onPageChange={handlePageChange}
          />
        </>
      ) : (
        <div className="card text-center py-16">
          <KeyRound className="w-16 h-16 text-dark-600 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-dark-200 mb-2">No secrets found</h3>
          <p className="text-dark-400 mb-6">
            {filters.searchTerm || filters.category || filters.isFavorite
              ? 'Try adjusting your filters'
              : 'Start by adding your first secret'}
          </p>
          <Button leftIcon={<Plus size={18} />} onClick={handleOpenCreate}>
            Add your first secret
          </Button>
        </div>
      )}

      {/* Create/Edit Modal */}
      <Modal
        isOpen={isFormModalOpen}
        onClose={() => {
          setIsFormModalOpen(false);
          setSelectedSecret(null);
        }}
        title={selectedSecret ? 'Edit Secret' : 'Add New Secret'}
        size="lg"
      >
        <SecretForm
          secret={selectedSecret || undefined}
          onSubmit={handleFormSubmit}
          onCancel={() => {
            setIsFormModalOpen(false);
            setSelectedSecret(null);
          }}
          isLoading={isSubmitting}
        />
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal
        isOpen={isDeleteModalOpen}
        onClose={() => {
          setIsDeleteModalOpen(false);
          setSelectedSecret(null);
        }}
        title="Delete Secret"
        size="sm"
      >
        <div className="text-center">
          <div className="w-16 h-16 rounded-full bg-red-500/10 flex items-center justify-center mx-auto mb-4">
            <AlertTriangle className="w-8 h-8 text-red-500" />
          </div>
          <p className="text-dark-200 mb-2">Are you sure you want to delete this secret?</p>
          <p className="text-dark-400 text-sm mb-6">
            This action cannot be undone.
          </p>
          <div className="flex justify-center gap-3">
            <Button
              variant="secondary"
              onClick={() => {
                setIsDeleteModalOpen(false);
                setSelectedSecret(null);
              }}
            >
              Cancel
            </Button>
            <Button variant="danger" onClick={handleDelete} isLoading={isSubmitting}>
              Delete
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
