import { LogOut, ArrowLeft } from 'lucide-react';
import Button from './Button';
import type { User } from '../types';

interface MergeTopBarProps {
  user: User | null;
  isLoading: boolean;
  onLogout: () => Promise<void>;
  onBack: () => void;
}

const MergeTopBar = ({
  user,
  isLoading,
  onLogout,
  onBack,
}: MergeTopBarProps) => {
  return (
    <div className="topbar">
      {/* Left section: Back button */}
      <div className="flex items-center flex-1">
        <button
          onClick={onBack}
          className="p-2 hover:bg-black/5 rounded-lg transition-colors bg-transparent border-none cursor-pointer outline-none flex items-center gap-2"
          title="Вярнуцца да мапы"
        >
          <ArrowLeft size={20} className="text-black/60" />
          <span className="text-sm font-medium text-black/80">На мапу</span>
        </button>
      </div>

      {/* Right section: User info */}
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
        ) : null}
      </div>
    </div>
  );
};

export default MergeTopBar;
