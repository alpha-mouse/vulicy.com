import { useState } from 'react';
import { X, Loader2 } from 'lucide-react';
import FeatureInfoView from './FeatureInfoView';
import FeatureEditForm from './FeatureEditForm';
import type { FeatureProperties, NamingCategory } from '../types/feature';

interface FeatureInfoPanelProps {
  feature: FeatureProperties | null;
  namingCategories: NamingCategory[];
  isCopied: boolean;
  onCopyLink: () => void;
  onClose: () => void;
  isAdmin?: boolean;
  isAuthenticated?: boolean;
  discourseBaseUrl?: string;
  onForumLinkCreated?: (featureId: number, forumLink: string) => void;
  onFeatureUpdated?: (featureId: number, updatedData?: Partial<FeatureProperties>) => void;
  isLoading?: boolean;
}

/**
 * Main wrapper component for feature information display.
 * Handles loading state and switches between view and edit modes.
 */
const FeatureInfoPanel = ({
  feature,
  namingCategories,
  isCopied,
  onCopyLink,
  onClose,
  isAdmin = false,
  isAuthenticated = false,
  discourseBaseUrl,
  onForumLinkCreated,
  onFeatureUpdated,
  isLoading = false,
}: FeatureInfoPanelProps) => {
  const [isEditing, setIsEditing] = useState(false);

  // Loading state
  if (isLoading) {
    return (
      <div className="absolute right-4 top-4 h-32 w-96 glass z-20 p-6 flex items-center justify-center animate-in fade-in duration-200">
        <div className="flex flex-col items-center gap-2 text-black/40">
          <Loader2 size={24} className="animate-spin" />
          <span className="text-sm font-medium">Загрузка...</span>
        </div>
        <button
          onClick={onClose}
          className="absolute top-6 right-6 text-black/30 hover:text-black/60 transition-colors p-0 appearance-none bg-transparent border-none cursor-pointer outline-none"
        >
          <X size={20} strokeWidth={1.2} />
        </button>
      </div>
    );
  }

  // No feature to display
  if (!feature) return null;

  // Edit mode
  if (isEditing && isAdmin && onFeatureUpdated) {
    return (
      <FeatureEditForm
        feature={feature}
        namingCategories={namingCategories}
        onClose={() => setIsEditing(false)}
        onFeatureUpdated={onFeatureUpdated}
      />
    );
  }

  // View mode
  return (
    <FeatureInfoView
      feature={feature}
      namingCategories={namingCategories}
      isCopied={isCopied}
      onCopyLink={onCopyLink}
      onClose={onClose}
      onStartEdit={() => setIsEditing(true)}
      isAdmin={isAdmin}
      isAuthenticated={isAuthenticated}
      discourseBaseUrl={discourseBaseUrl}
      onForumLinkCreated={onForumLinkCreated}
    />
  );
};

export default FeatureInfoPanel;
