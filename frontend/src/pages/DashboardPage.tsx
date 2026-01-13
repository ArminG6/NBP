import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { KeyRound, Star, Shield, Plus, ArrowRight } from 'lucide-react';
import { useAuth } from '../auth/useAuth';
import { secretsApi } from '../api/secretsApi';
import { SecretListResponse } from '../types/secret.types';
import { Button } from '../components/common/Button';
import { LoadingSpinner } from '../components/common/LoadingSpinner';

export function DashboardPage() {
  const { user } = useAuth();
  const [stats, setStats] = useState<{
    total: number;
    favorites: number;
    recent: SecretListResponse | null;
  }>({
    total: 0,
    favorites: 0,
    recent: null,
  });
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadStats = async () => {
      try {
        const [allSecrets, favoriteSecrets, recentSecrets] = await Promise.all([
          secretsApi.getSecrets({ pageSize: 1 }),
          secretsApi.getSecrets({ pageSize: 1, isFavorite: true }),
          secretsApi.getSecrets({ pageSize: 5, sortBy: 'createdAt', sortDescending: true }),
        ]);

        setStats({
          total: allSecrets.totalCount,
          favorites: favoriteSecrets.totalCount,
          recent: recentSecrets,
        });
      } catch (error) {
        console.error('Failed to load stats:', error);
      } finally {
        setIsLoading(false);
      }
    };

    loadStats();
  }, []);

  const getDomain = (url: string) => {
    try {
      const domain = new URL(url.startsWith('http') ? url : `https://${url}`).hostname;
      return domain.replace('www.', '');
    } catch {
      return url;
    }
  };

  return (
    <div className="animate-fade-in">
      {/* Welcome section */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-dark-100 mb-2">
          Welcome back, <span className="gradient-text">{user?.firstName}</span>
        </h1>
        <p className="text-dark-400">
          Your secure vault is ready. Manage your passwords safely.
        </p>
      </div>

      {/* Stats cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <div className="card animate-slide-up">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-primary-500 to-primary-600 
                           flex items-center justify-center shadow-lg shadow-primary-500/25">
              <KeyRound className="w-6 h-6 text-white" />
            </div>
            <div>
              <p className="text-sm text-dark-400">Total Secrets</p>
              {isLoading ? (
                <LoadingSpinner size="sm" />
              ) : (
                <p className="text-2xl font-bold text-dark-100">{stats.total}</p>
              )}
            </div>
          </div>
        </div>

        <div className="card animate-slide-up animation-delay-100">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-yellow-500 to-orange-500 
                           flex items-center justify-center shadow-lg shadow-yellow-500/25">
              <Star className="w-6 h-6 text-white" />
            </div>
            <div>
              <p className="text-sm text-dark-400">Favorites</p>
              {isLoading ? (
                <LoadingSpinner size="sm" />
              ) : (
                <p className="text-2xl font-bold text-dark-100">{stats.favorites}</p>
              )}
            </div>
          </div>
        </div>

        <div className="card animate-slide-up animation-delay-200">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-green-500 to-emerald-500 
                           flex items-center justify-center shadow-lg shadow-green-500/25">
              <Shield className="w-6 h-6 text-white" />
            </div>
            <div>
              <p className="text-sm text-dark-400">Security</p>
              <p className="text-2xl font-bold text-dark-100">AES-256</p>
            </div>
          </div>
        </div>
      </div>

      {/* Quick actions */}
      <div className="flex flex-wrap gap-4 mb-8">
        <Link to="/secrets">
          <Button leftIcon={<Plus size={18} />}>
            Add New Secret
          </Button>
        </Link>
        <Link to="/export">
          <Button variant="secondary">
            Export Secrets
          </Button>
        </Link>
      </div>

      {/* Recent secrets */}
      <div className="card">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-semibold text-dark-100">Recent Secrets</h2>
          <Link
            to="/secrets"
            className="flex items-center gap-1 text-sm text-primary-400 hover:text-primary-300"
          >
            View all
            <ArrowRight size={16} />
          </Link>
        </div>

        {isLoading ? (
          <div className="flex justify-center py-8">
            <LoadingSpinner size="lg" />
          </div>
        ) : stats.recent && stats.recent.items.length > 0 ? (
          <div className="space-y-3">
            {stats.recent.items.map((secret) => (
              <Link
                key={secret.id}
                to="/secrets"
                className="flex items-center gap-4 p-4 bg-dark-800/50 rounded-xl 
                          hover:bg-dark-800 transition-colors group"
              >
                <div className="w-10 h-10 rounded-lg bg-dark-700 flex items-center justify-center">
                  <img
                    src={`https://www.google.com/s2/favicons?domain=${getDomain(secret.websiteUrl)}&sz=24`}
                    alt=""
                    className="w-5 h-5"
                    onError={(e) => {
                      (e.target as HTMLImageElement).src = 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" fill="%2394a3b8" viewBox="0 0 24 24"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2z"/></svg>';
                    }}
                  />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-dark-100 truncate">
                    {getDomain(secret.websiteUrl)}
                  </p>
                  <p className="text-sm text-dark-400 truncate">{secret.username}</p>
                </div>
                {secret.isFavorite && (
                  <Star className="w-4 h-4 text-yellow-500 fill-yellow-500" />
                )}
                <ArrowRight className="w-4 h-4 text-dark-500 group-hover:text-dark-300 transition-colors" />
              </Link>
            ))}
          </div>
        ) : (
          <div className="text-center py-8">
            <KeyRound className="w-12 h-12 text-dark-600 mx-auto mb-4" />
            <p className="text-dark-400 mb-4">No secrets yet</p>
            <Link to="/secrets">
              <Button size="sm" leftIcon={<Plus size={16} />}>
                Add your first secret
              </Button>
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}
