import { useState } from 'react';
import { Globe, User, Eye, EyeOff, Star, Edit2, Trash2, Copy, Check } from 'lucide-react';
import { Secret } from '../../types/secret.types';
import { secretsApi } from '../../api/secretsApi';
import { LoadingSpinner } from '../common/LoadingSpinner';
import toast from 'react-hot-toast';

interface SecretCardProps {
  secret: Secret;
  onEdit: (secret: Secret) => void;
  onDelete: (secret: Secret) => void;
}

export function SecretCard({ secret, onEdit, onDelete }: SecretCardProps) {
  const [isRevealing, setIsRevealing] = useState(false);
  const [revealedPassword, setRevealedPassword] = useState<string | null>(null);
  const [copied, setCopied] = useState<'password' | 'username' | null>(null);

  const handleRevealPassword = async () => {
    if (revealedPassword) {
      setRevealedPassword(null);
      return;
    }

    setIsRevealing(true);
    try {
      const password = await secretsApi.decryptPassword(secret.id);
      setRevealedPassword(password);
      
      // Auto-hide after 30 seconds
      setTimeout(() => {
        setRevealedPassword(null);
      }, 30000);
    } catch (error) {
      console.error('Failed to decrypt password:', error);
      toast.error('Failed to reveal password');
    } finally {
      setIsRevealing(false);
    }
  };

  const handleCopy = async (text: string, type: 'password' | 'username') => {
    try {
      await navigator.clipboard.writeText(text);
      setCopied(type);
      toast.success(`${type === 'password' ? 'Password' : 'Username'} copied!`);
      setTimeout(() => setCopied(null), 2000);
    } catch {
      toast.error('Failed to copy');
    }
  };

  const handleCopyPassword = async () => {
    if (revealedPassword) {
      await handleCopy(revealedPassword, 'password');
    } else {
      try {
        setIsRevealing(true);
        const password = await secretsApi.decryptPassword(secret.id);
        await handleCopy(password, 'password');
      } catch (error) {
        console.error('Failed to copy password:', error);
        toast.error('Failed to copy password');
      } finally {
        setIsRevealing(false);
      }
    }
  };

  const getDomain = (url: string) => {
    try {
      const domain = new URL(url.startsWith('http') ? url : `https://${url}`).hostname;
      return domain.replace('www.', '');
    } catch {
      return url;
    }
  };

  return (
    <div className="card-hover group">
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-start gap-4 flex-1 min-w-0">
          {/* Favicon */}
          <div className="w-12 h-12 rounded-xl bg-dark-800 flex items-center justify-center 
                         flex-shrink-0 border border-dark-700">
            <img
              src={`https://www.google.com/s2/favicons?domain=${getDomain(secret.websiteUrl)}&sz=32`}
              alt=""
              className="w-6 h-6"
              onError={(e) => {
                (e.target as HTMLImageElement).style.display = 'none';
                (e.target as HTMLImageElement).parentElement!.innerHTML = 
                  '<svg class="w-6 h-6 text-dark-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9"/></svg>';
              }}
            />
          </div>

          {/* Info */}
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              <h3 className="font-semibold text-dark-100 truncate">
                {getDomain(secret.websiteUrl)}
              </h3>
              {secret.isFavorite && (
                <Star className="w-4 h-4 text-yellow-500 fill-yellow-500 flex-shrink-0" />
              )}
            </div>
            
            <a
              href={secret.websiteUrl.startsWith('http') ? secret.websiteUrl : `https://${secret.websiteUrl}`}
              target="_blank"
              rel="noopener noreferrer"
              className="text-sm text-dark-400 hover:text-primary-400 flex items-center gap-1 truncate"
            >
              <Globe className="w-3 h-3 flex-shrink-0" />
              {secret.websiteUrl}
            </a>

            <div className="flex items-center gap-2 mt-2">
              <div className="flex items-center gap-1.5 px-2 py-1 bg-dark-800 rounded-lg">
                <User className="w-3 h-3 text-dark-400" />
                <span className="text-sm text-dark-300">{secret.username}</span>
                <button
                  onClick={() => handleCopy(secret.username, 'username')}
                  className="ml-1 p-0.5 text-dark-400 hover:text-dark-100 transition-colors"
                >
                  {copied === 'username' ? <Check className="w-3 h-3 text-green-500" /> : <Copy className="w-3 h-3" />}
                </button>
              </div>
              
              {secret.category && (
                <span className="px-2 py-1 text-xs font-medium bg-dark-800 text-dark-300 rounded-lg">
                  {secret.category}
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
          <button
            onClick={() => onEdit(secret)}
            className="p-2 text-dark-400 hover:text-dark-100 hover:bg-dark-800 rounded-lg transition-colors"
            title="Edit"
          >
            <Edit2 size={16} />
          </button>
          <button
            onClick={() => onDelete(secret)}
            className="p-2 text-dark-400 hover:text-red-400 hover:bg-dark-800 rounded-lg transition-colors"
            title="Delete"
          >
            <Trash2 size={16} />
          </button>
        </div>
      </div>

      {/* Password section */}
      <div className="mt-4 pt-4 border-t border-dark-800">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="text-sm text-dark-400">Password:</span>
            {revealedPassword ? (
              <code className="px-2 py-1 bg-dark-800 rounded font-mono text-sm text-primary-400">
                {revealedPassword}
              </code>
            ) : (
              <code className="px-2 py-1 bg-dark-800 rounded font-mono text-sm text-dark-500">
                ••••••••••••
              </code>
            )}
          </div>

          <div className="flex items-center gap-1">
            <button
              onClick={handleCopyPassword}
              disabled={isRevealing}
              className="p-2 text-dark-400 hover:text-dark-100 hover:bg-dark-800 
                        rounded-lg transition-colors disabled:opacity-50"
              title="Copy password"
            >
              {isRevealing ? (
                <LoadingSpinner size="sm" />
              ) : copied === 'password' ? (
                <Check className="w-4 h-4 text-green-500" />
              ) : (
                <Copy className="w-4 h-4" />
              )}
            </button>
            <button
              onClick={handleRevealPassword}
              disabled={isRevealing}
              className="p-2 text-dark-400 hover:text-dark-100 hover:bg-dark-800 
                        rounded-lg transition-colors disabled:opacity-50"
              title={revealedPassword ? 'Hide password' : 'Reveal password'}
            >
              {isRevealing ? (
                <LoadingSpinner size="sm" />
              ) : revealedPassword ? (
                <EyeOff className="w-4 h-4" />
              ) : (
                <Eye className="w-4 h-4" />
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
