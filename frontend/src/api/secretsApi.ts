import axiosInstance from './axiosInstance';
import {
  Secret,
  SecretListResponse,
  CreateSecretRequest,
  UpdateSecretRequest,
  SecretQueryParams,
  DecryptedPassword,
} from '../types/secret.types';

export const secretsApi = {
  getSecrets: async (params: SecretQueryParams = {}): Promise<SecretListResponse> => {
    const response = await axiosInstance.get<SecretListResponse>('/secrets', { params });
    return response.data;
  },

  getSecret: async (id: string): Promise<Secret> => {
    const response = await axiosInstance.get<Secret>(`/secrets/${id}`);
    return response.data;
  },

  createSecret: async (data: CreateSecretRequest): Promise<Secret> => {
    const response = await axiosInstance.post<Secret>('/secrets', data);
    return response.data;
  },

  updateSecret: async (id: string, data: UpdateSecretRequest): Promise<Secret> => {
    const response = await axiosInstance.put<Secret>(`/secrets/${id}`, data);
    return response.data;
  },

  deleteSecret: async (id: string): Promise<void> => {
    await axiosInstance.delete(`/secrets/${id}`);
  },

  decryptPassword: async (id: string): Promise<string> => {
    const response = await axiosInstance.get<DecryptedPassword>(`/secrets/${id}/decrypt`);
    return response.data.password;
  },
};
