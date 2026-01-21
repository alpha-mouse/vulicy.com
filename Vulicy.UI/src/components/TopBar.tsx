import { useState, useRef, useEffect } from 'react';
import { LogIn, LogOut, Menu, FileUser } from 'lucide-react';
import Search from './Search';
import type { User, SearchResult } from '../types/feature';

interface TopBarProps {
  user: User | null;
  isLoading: boolean;
  onLogin: () => void;
  onLogout: () => Promise<void>;
  currentLat: number;
  currentLng: number;
  onResultClick: (result: SearchResult) => void;
  isAdmin?: boolean;
  onOpenDossierPanel?: () => void;
}

const TopBar = ({
  user,
  isLoading,
  onLogin,
  onLogout,
  currentLat,
  currentLng,
  onResultClick,
  isAdmin = false,
  onOpenDossierPanel,
}: TopBarProps) => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  // Close menu on click outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsMenuOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleMenuItemClick = (action: () => void) => {
    action();
    setIsMenuOpen(false);
  };

  return (
    <div className="topbar">
      {/* Left section: Admin menu + Search */}
      <div className="flex items-center flex-1 max-w-md">
        {/* Admin menu button - always in DOM to prevent layout shift */}
        <div className={`relative ${!(isAdmin && user) ? 'invisible' : ''}`} ref={menuRef}>
          <button
            onClick={() => setIsMenuOpen(!isMenuOpen)}
            className="p-2 hover:bg-black/5 rounded-lg transition-colors bg-transparent border-none cursor-pointer outline-none"
            title="Меню"
          >
            <Menu size={20} className="text-black/60" />
          </button>

          {isMenuOpen && (
            <div className="absolute top-full left-0 mt-1 bg-white dark:bg-slate-800 rounded-lg shadow-2xl border border-black/10 dark:border-white/10 overflow-hidden z-50 min-w-40">
              <button
                onClick={() => handleMenuItemClick(() => onOpenDossierPanel?.())}
                className="w-full px-4 py-2.5 text-left text-sm font-medium text-black hover:bg-black/5 transition-colors bg-transparent border-none cursor-pointer outline-none flex items-center gap-2"
              >
                <FileUser size={18} className="text-black/60" />
                <span>Імёны</span>
              </button>
            </div>
          )}
        </div>

        <Search
          currentLat={currentLat}
          currentLng={currentLng}
          onResultClick={onResultClick}
          embedded
        />
      </div>

      <div className="flex items-center gap-3">
        {isLoading ? (
          <div className="text-sm text-black/40">...</div>
        ) : user ? (
          <>
            <span className="text-sm font-medium text-black">{user.username}</span>
            <button
              onClick={onLogout}
              className="flex items-center gap-2 px-3 py-1.5 text-sm font-medium text-black/60 hover:text-black transition-colors bg-transparent border-none cursor-pointer outline-none"
              title="Выйсьці"
            >
              <LogOut size={16} />
            </button>
          </>
        ) : (
          <button
            onClick={onLogin}
            className="flex items-center gap-2 px-4 py-1.5 text-sm font-medium bg-secondary text-black rounded-lg hover:bg-secondary-hover transition-colors border border-black/10 cursor-pointer outline-none shadow-sm"
          >
            <LogIn size={16} />
            <span>Увайсьці</span>
          </button>
        )}
      </div>
    </div>
  );
};

export default TopBar;

