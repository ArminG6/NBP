import { useForm } from 'react-hook-form';
import { Globe, User, KeyRound, StickyNote, Tag, Star } from 'lucide-react';
import { Secret, CreateSecretRequest, UpdateSecretRequest } from '../../types/secret.types';
import { Input } from '../common/Input';
import { Button } from '../common/Button';

interface SecretFormProps {
  secret?: Secret;
  onSubmit: (data: CreateSecretRequest | UpdateSecretRequest) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

interface FormData {
  websiteUrl: string;
  username: string;
  password: string;
  notes: string;
  category: string;
  isFavorite: boolean;
}

export function SecretForm({ secret, onSubmit, onCancel, isLoading }: SecretFormProps) {
  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<FormData>({
    defaultValues: {
      websiteUrl: secret?.websiteUrl || '',
      username: secret?.username || '',
      password: '',
      notes: secret?.notes || '',
      category: secret?.category || '',
      isFavorite: secret?.isFavorite || false,
    },
  });

  const isFavorite = watch('isFavorite');

  const onFormSubmit = async (data: FormData) => {
    if (secret) {
      // Update - password is optional
      const updateData: UpdateSecretRequest = {
        websiteUrl: data.websiteUrl,
        username: data.username,
        password: data.password || undefined,
        notes: data.notes || undefined,
        category: data.category || undefined,
        isFavorite: data.isFavorite,
      };
      await onSubmit(updateData);
    } else {
      // Create - password is required
      const createData: CreateSecretRequest = {
        websiteUrl: data.websiteUrl,
        username: data.username,
        password: data.password,
        notes: data.notes || undefined,
        category: data.category || undefined,
        isFavorite: data.isFavorite,
      };
      await onSubmit(createData);
    }
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-5">
      <Input
        label="Website URL"
        placeholder="https://example.com"
        leftIcon={<Globe size={18} />}
        error={errors.websiteUrl?.message}
        {...register('websiteUrl', {
          required: 'Website URL is required',
          maxLength: { value: 2048, message: 'URL is too long' },
        })}
      />

      <Input
        label="Username / Email"
        placeholder="your@email.com"
        leftIcon={<User size={18} />}
        error={errors.username?.message}
        {...register('username', {
          required: 'Username is required',
          maxLength: { value: 256, message: 'Username is too long' },
        })}
      />

      <Input
        label={secret ? 'New Password (leave empty to keep current)' : 'Password'}
        type="password"
        placeholder={secret ? '••••••••••••' : 'Enter password'}
        leftIcon={<KeyRound size={18} />}
        error={errors.password?.message}
        {...register('password', {
          required: secret ? false : 'Password is required',
          maxLength: { value: 1024, message: 'Password is too long' },
        })}
      />

      <div className="grid grid-cols-2 gap-4">
        <Input
          label="Category (optional)"
          placeholder="e.g., Social, Work, Finance"
          leftIcon={<Tag size={18} />}
          error={errors.category?.message}
          {...register('category', {
            maxLength: { value: 100, message: 'Category is too long' },
          })}
        />

        <div className="flex flex-col justify-end">
          <button
            type="button"
            onClick={() => setValue('isFavorite', !isFavorite)}
            className={`flex items-center gap-2 px-4 py-3 rounded-xl border transition-colors
                       ${isFavorite
                         ? 'bg-yellow-500/10 border-yellow-500/50 text-yellow-500'
                         : 'bg-dark-800 border-dark-600 text-dark-400 hover:border-dark-500'
                       }`}
          >
            <Star className={`w-5 h-5 ${isFavorite ? 'fill-yellow-500' : ''}`} />
            <span className="font-medium">Favorite</span>
          </button>
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-dark-300 mb-2">
          Notes (optional)
        </label>
        <div className="relative">
          <div className="absolute left-4 top-3 text-dark-400">
            <StickyNote size={18} />
          </div>
          <textarea
            placeholder="Any additional notes..."
            className="w-full pl-11 pr-4 py-3 bg-dark-800 border border-dark-600 rounded-xl
                      text-dark-100 placeholder-dark-400 transition-all duration-200
                      focus:outline-none focus:border-primary-500 focus:ring-2 
                      focus:ring-primary-500/20 min-h-[100px] resize-y"
            {...register('notes', {
              maxLength: { value: 4000, message: 'Notes are too long' },
            })}
          />
        </div>
        {errors.notes && (
          <p className="mt-2 text-sm text-red-400">{errors.notes.message}</p>
        )}
      </div>

      <div className="flex justify-end gap-3 pt-4">
        <Button type="button" variant="secondary" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" isLoading={isLoading}>
          {secret ? 'Update Secret' : 'Create Secret'}
        </Button>
      </div>
    </form>
  );
}
