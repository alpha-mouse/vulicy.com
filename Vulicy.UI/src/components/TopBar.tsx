import { LogIn, LogOut } from 'lucide-react';
import Button from './Button';
import './TopBar.css';
import { useAuth } from '../hooks/useAuth';

interface TopBarProps {
  leftContent?: React.ReactNode;
  centerContent?: React.ReactNode;
}

const TopBar = ({ leftContent, centerContent }: TopBarProps) => {
  const { user, isLoading, login, logout } = useAuth();

  const handleLogin = () => login(window.location.href);

  return (
    <div className="topbar">
      {/* Left section: passed content */}
      <div className="topbar-left">
        {leftContent}
      </div>

      {/* Center section: flexible space for search controls */}
      {centerContent && (
        <div className="topbar-center">
          {centerContent}
        </div>
      )}

      {/* Right section: Login/Logout */}
      <div className="flex items-center gap-3">
        {isLoading ? (
          <div className="text-sm text-black/40">...</div>
        ) : user ? (
          <>
            <span className="text-sm font-medium text-black">{user.username}</span>
            <Button
              variant="ghost"
              size="sm"
              onClick={logout}
              icon={<LogOut size={16} />}
              title="Выйсьці"
              className="px-3"
            />
          </>
        ) : (
          <Button
            variant="secondary"
            size="sm"
            onClick={handleLogin}
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
