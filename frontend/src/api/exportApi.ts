import axiosInstance from './axiosInstance';

export const exportApi = {
  exportCsv: async (): Promise<Blob> => {
    const response = await axiosInstance.get('/export/csv', {
      responseType: 'blob',
    });
    return response.data;
  },

  exportTxt: async (): Promise<Blob> => {
    const response = await axiosInstance.get('/export/txt', {
      responseType: 'blob',
    });
    return response.data;
  },
};

export const downloadBlob = (blob: Blob, filename: string) => {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  window.URL.revokeObjectURL(url);
};
