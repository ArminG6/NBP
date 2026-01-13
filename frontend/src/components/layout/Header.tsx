import { Link } from 'react-router-dom';
import { LogOut, KeyRound, User } from 'lucide-react';
import { useAuth } from '../../auth/useAuth';

export function Header() {
  const { user, logout } = useAuth();

  return (
    <header className="sticky top-0 z-40 glass border-b border-dark-700/50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <Link to="/dashboard" className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary-500 to-accent-500 
                           flex items-center justify-center shadow-lg shadow-primary-500/25">
              <KeyRound className="w-5 h-5 text-white" />
            </div>
            <span className="text-xl font-semibold gradient-text">
              My Secrets
            </span>
          </Link>

          {/* User menu */}
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-3 px-4 py-2 bg-dark-800/50 rounded-xl">
              <div className="w-8 h-8 rounded-full bg-gradient-to-br from-primary-500 to-accent-500 
                             flex items-center justify-center">
                <User className="w-4 h-4 text-white" />
              </div>
              <div className="hidden sm:block">
                <p className="text-sm font-medium text-dark-100">
                  {user?.firstName} {user?.lastName}
                </p>
                <p className="text-xs text-dark-400">{user?.email}</p>
              </div>
            </div>

            <button
              onClick={logout}
              className="p-2 text-dark-400 hover:text-dark-100 hover:bg-dark-800 
                        rounded-lg transition-colors"
              title="Logout"
            >
              <LogOut size={20} />
            </button>
          </div>
        </div>
      </div>
    </header>
  );
}
