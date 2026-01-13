import { NavLink } from 'react-router-dom';
import { LayoutDashboard, KeyRound, Download } from 'lucide-react';

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/secrets', label: 'Secrets', icon: KeyRound },
  { to: '/export', label: 'Export', icon: Download },
];

export function MobileNav() {
  return (
    <nav className="fixed bottom-0 left-0 right-0 z-40 glass border-t border-dark-700/50 lg:hidden">
      <div className="flex items-center justify-around h-16">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `flex flex-col items-center gap-1 px-4 py-2 rounded-lg transition-colors
               ${isActive
                 ? 'text-primary-400'
                 : 'text-dark-400 hover:text-dark-100'
               }`
            }
          >
            <item.icon size={20} />
            <span className="text-xs font-medium">{item.label}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  );
}
