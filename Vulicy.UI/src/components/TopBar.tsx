import { useState, useRef } from 'react';
import { LogIn, LogOut, Menu, FileUser } from 'lucide-react';
import { useClickOutside } from '../hooks/useClickOutside';
import Button from './Button';
import Search from './Search';
import type { SearchResult, User } from '../types';

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
  useClickOutside(menuRef, () => setIsMenuOpen(false));

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
            <Button
              variant="ghost"
              size="sm"
              onClick={onLogout}
              icon={<LogOut size={16} />}
              title="Выйсьці"
              className="px-3"
            />
          </>
        ) : (
          <Button
            variant="secondary"
            size="sm"
            onClick={onLogin}
            icon={<LogIn size={16} />}
          >
            Увайсьці
          </Button>
        )}
      </div>
    </div>
  );
};

export default TopBar;

