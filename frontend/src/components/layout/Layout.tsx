import { Outlet } from 'react-router-dom';
import { Header } from './Header';
import { Sidebar } from './Sidebar';
import { MobileNav } from './MobileNav';

export function Layout() {
  return (
    <div className="min-h-screen bg-dark-950 grid-pattern">
      <Header />
      <Sidebar />
      <MobileNav />
      
      <main className="lg:ml-64 pt-4 pb-20 lg:pb-4">
        <div className="page-container">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
