import { LogIn, LogOut } from 'lucide-react';
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
}

const TopBar = ({
  user,
  isLoading,
  onLogin,
  onLogout,
  currentLat,
  currentLng,
  onResultClick,
}: TopBarProps) => {
  return (
    <div className="topbar">
      <Search
        currentLat={currentLat}
        currentLng={currentLng}
        onResultClick={onResultClick}
        embedded
      />

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
