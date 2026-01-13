export interface Secret {
  id: string;
  websiteUrl: string;
  username: string;
  notes?: string;
  category?: string;
  isFavorite: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface SecretListResponse {
  items: Secret[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateSecretRequest {
  websiteUrl: string;
  username: string;
  password: string;
  notes?: string;
  category?: string;
  isFavorite?: boolean;
}

export interface UpdateSecretRequest {
  websiteUrl: string;
  username: string;
  password?: string;
  notes?: string;
  category?: string;
  isFavorite: boolean;
}

export interface SecretQueryParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  category?: string;
  isFavorite?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface DecryptedPassword {
  password: string;
}
