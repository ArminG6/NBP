import { useState } from 'react';
import { Download, FileText, FileSpreadsheet, Shield, AlertTriangle } from 'lucide-react';
import { exportApi, downloadBlob } from '../api/exportApi';
import { Button } from '../components/common/Button';
import toast from 'react-hot-toast';

export function ExportPage() {
  const [isExporting, setIsExporting] = useState<'csv' | 'txt' | null>(null);

  const handleExport = async (format: 'csv' | 'txt') => {
    setIsExporting(format);
    try {
      const blob = format === 'csv' 
        ? await exportApi.exportCsv() 
        : await exportApi.exportTxt();
      
      const timestamp = new Date().toISOString().slice(0, 19).replace(/[:-]/g, '');
      const filename = `my_secrets_export_${timestamp}.${format}`;
      
      downloadBlob(blob, filename);
      toast.success(`Secrets exported to ${format.toUpperCase()}`);
    } catch (error) {
      console.error('Export failed:', error);
      toast.error('Failed to export secrets');
    } finally {
      setIsExporting(null);
    }
  };

  return (
    <div className="animate-fade-in max-w-3xl">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-dark-100 mb-2">Export Secrets</h1>
        <p className="text-dark-400">
          Download your secrets in CSV or TXT format for backup purposes.
        </p>
      </div>

      {/* Security notice */}
      <div className="card mb-8 border-yellow-500/30 bg-yellow-500/5">
        <div className="flex gap-4">
          <div className="w-12 h-12 rounded-xl bg-yellow-500/10 flex items-center justify-center flex-shrink-0">
            <Shield className="w-6 h-6 text-yellow-500" />
          </div>
          <div>
            <h3 className="font-semibold text-yellow-200 mb-1">Passwords remain encrypted</h3>
            <p className="text-dark-400 text-sm">
              For your security, exported files contain your passwords in <strong>encrypted form only</strong>. 
              Passwords are never exported in plain text. This ensures your data remains safe even if 
              the export file is compromised.
            </p>
          </div>
        </div>
      </div>

      {/* Export options */}
      <div className="grid md:grid-cols-2 gap-6">
        {/* CSV Export */}
        <div className="card-hover">
          <div className="flex items-start gap-4 mb-4">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-green-500 to-emerald-600 
                           flex items-center justify-center shadow-lg shadow-green-500/25">
              <FileSpreadsheet className="w-6 h-6 text-white" />
            </div>
            <div>
              <h3 className="text-lg font-semibold text-dark-100">CSV Format</h3>
              <p className="text-sm text-dark-400">Comma-separated values</p>
            </div>
          </div>
          
          <p className="text-dark-300 text-sm mb-6">
            Export your secrets as a CSV file. Ideal for importing into spreadsheet 
            applications or other password managers.
          </p>

          <div className="bg-dark-800/50 rounded-lg p-3 mb-6">
            <p className="text-xs text-dark-400 font-mono">
              Website URL, Username, Encrypted Password, Notes, Category
            </p>
          </div>

          <Button
            onClick={() => handleExport('csv')}
            isLoading={isExporting === 'csv'}
            disabled={isExporting !== null}
            leftIcon={<Download size={18} />}
            className="w-full"
          >
            Export as CSV
          </Button>
        </div>

        {/* TXT Export */}
        <div className="card-hover">
          <div className="flex items-start gap-4 mb-4">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-blue-500 to-indigo-600 
                           flex items-center justify-center shadow-lg shadow-blue-500/25">
              <FileText className="w-6 h-6 text-white" />
            </div>
            <div>
              <h3 className="text-lg font-semibold text-dark-100">TXT Format</h3>
              <p className="text-sm text-dark-400">Human-readable text file</p>
            </div>
          </div>
          
          <p className="text-dark-300 text-sm mb-6">
            Export your secrets as a formatted text file. Easy to read and print 
            for offline backup storage.
          </p>

          <div className="bg-dark-800/50 rounded-lg p-3 mb-6">
            <p className="text-xs text-dark-400 font-mono whitespace-pre">
{`═══════════════════════
[1] example.com
  Username: user@email.com
  Password (encrypted): ...`}
            </p>
          </div>

          <Button
            onClick={() => handleExport('txt')}
            isLoading={isExporting === 'txt'}
            disabled={isExporting !== null}
            leftIcon={<Download size={18} />}
            className="w-full"
            variant="secondary"
          >
            Export as TXT
          </Button>
        </div>
      </div>

      {/* Warning */}
      <div className="mt-8 p-4 rounded-xl border border-dark-700 bg-dark-900/50">
        <div className="flex gap-3">
          <AlertTriangle className="w-5 h-5 text-dark-400 flex-shrink-0 mt-0.5" />
          <div className="text-sm text-dark-400">
            <p className="font-medium text-dark-300 mb-1">Storage recommendation</p>
            <p>
              Store exported files in a secure location. While passwords are encrypted, 
              the file still contains your usernames and website URLs. Consider using 
              encrypted storage or a secure backup solution.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
