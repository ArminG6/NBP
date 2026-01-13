import { NavLink } from 'react-router-dom';
import { LayoutDashboard, KeyRound, Download } from 'lucide-react';

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/secrets', label: 'Secrets', icon: KeyRound },
  { to: '/export', label: 'Export', icon: Download },
];

export function Sidebar() {
  return (
    <aside className="fixed left-0 top-16 bottom-0 w-64 bg-dark-900/50 
                      border-r border-dark-800 hidden lg:block">
      <nav className="p-4 space-y-2">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-200
               ${isActive
                 ? 'bg-gradient-to-r from-primary-500/20 to-accent-500/20 text-primary-400 border border-primary-500/30'
                 : 'text-dark-400 hover:bg-dark-800 hover:text-dark-100'
               }`
            }
          >
            <item.icon size={20} />
            <span className="font-medium">{item.label}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
