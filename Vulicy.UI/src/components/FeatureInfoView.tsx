import { useState, useRef } from 'react';
import { X, Link, Check, MessageSquarePlus, MessageSquare, Loader2, Pencil } from 'lucide-react';
import Button from './Button';
import {
  FEATURE_TYPE_LABELS,
  getClassificationText,
} from '../constants/mapConstants';
import type { FeatureProperties, NamingCategory } from '../types';
import { getFeatureName } from '../types/feature';
import { api } from '../utils/api';

interface FeatureInfoViewProps {
  feature: FeatureProperties;
  namingCategories: NamingCategory[];
  isCopied: boolean;
  onCopyLink: () => void;
  onClose: () => void;
  onStartEdit: () => void;
  isAdmin: boolean;
  isAuthenticated: boolean;
  discourseBaseUrl?: string;
  onForumLinkCreated?: (featureId: number, forumLink: string) => void;
}

/**
 * Read-only view component for displaying feature information.
 * Shows feature details, forum links, and dossier record information.
 */
const FeatureInfoView = ({
  feature,
  namingCategories,
  isCopied,
  onCopyLink,
  onClose,
  onStartEdit,
  isAdmin,
  isAuthenticated,
  discourseBaseUrl,
  onForumLinkCreated,
}: FeatureInfoViewProps) => {
  const currentIdRef = useRef(feature.id);
  currentIdRef.current = feature.id;

  const [isCreatingTopic, setIsCreatingTopic] = useState(false);
  const [localForumLink, setLocalForumLink] = useState<string | null>(null);

  const effectiveForumLink = localForumLink || feature.forumRelativeLink;
  const forumFullUrl = effectiveForumLink && discourseBaseUrl
    ? `${discourseBaseUrl}${effectiveForumLink.startsWith('/') ? '' : '/'}${effectiveForumLink}`
    : null;
  const hasAdminData = isAdmin && (feature.comment || feature.dossierRecordId);

  const handleCreateDiscussion = async () => {
    if (isCreatingTopic) return;
    const targetFeatureId = feature.id;
    setIsCreatingTopic(true);

    try {
      const data = await api.post<{ forumRelativeLink: string }>('/api/forum/create-feature-topic', {
        objectId: targetFeatureId
      });

      if (data.forumRelativeLink && currentIdRef.current === targetFeatureId) {
        setLocalForumLink(data.forumRelativeLink);
        onForumLinkCreated?.(targetFeatureId, data.forumRelativeLink);
      }
    } catch (error) {
      console.error('Failed to create forum topic:', error);
    } finally {
      if (currentIdRef.current === targetFeatureId) {
        setIsCreatingTopic(false);
      }
    }
  };

  return (
    <div className="absolute right-4 top-4 h-fit max-h-panel w-96 glass z-20 overflow-y-auto p-6 flex flex-col gap-3 animate-in">
      <div className="flex justify-between items-start">
        <h2 className="text-2xl font-bold leading-tight m-0">
          {FEATURE_TYPE_LABELS[feature.type] && (
            <span className="text-black/40 font-medium">{FEATURE_TYPE_LABELS[feature.type]} </span>
          )}
          {getFeatureName(feature)}
          <button
            onClick={onCopyLink}
            className="ml-2 p-1 inline-flex items-center justify-center text-black/30 hover:text-black/60 transition-colors bg-transparent border-none cursor-pointer outline-none align-middle"
            title="Скапіяваць спасылку"
          >
            {isCopied ? <Check size={16} className="text-green-500" /> : <Link size={16} />}
          </button>
          {isAdmin && (
            <button
              onClick={onStartEdit}
              className="ml-1 p-1 inline-flex items-center justify-center text-black/30 hover:text-black/60 transition-colors bg-transparent border-none cursor-pointer outline-none align-middle"
              title="Рэдагаваць"
            >
              <Pencil size={16} />
            </button>
          )}
        </h2>
        <div className="flex items-center gap-1">
          <button
            onClick={onClose}
            className="text-black/30 hover:text-black/60 transition-colors p-0 appearance-none bg-transparent border-none cursor-pointer outline-none"
          >
            <X size={20} strokeWidth={1.2} />
          </button>
        </div>
      </div>

      <div className="flex flex-col gap-2">
        {/* Classification - only show if not 0 (None) */}
        {feature.classification !== 0 && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Клясыфікацыя: </span>
            <span className="font-medium text-black">{getClassificationText(feature.classification)}</span>
          </div>
        )}

        {feature.dossierRecordNameBeTarask && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Этымалёгія: </span>
            <span className="text-black">{feature.dossierRecordNameBeTarask}</span>
          </div>
        )}

        {feature.renamingReason && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Абгрунтаваньне: </span>
            <span className="text-black">{feature.renamingReason}</span>
          </div>
        )}

        {feature.namingCategoryId && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Катэгорыя: </span>
            <span className="text-black">
              {namingCategories.find(c => c.id === feature.namingCategoryId)?.name || '...'}
            </span>
          </div>
        )}

        {feature.historicNames && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Гістарычная(-ыя) назва(-ы): </span>
            <span className="text-black italic">{feature.historicNames}</span>
          </div>
        )}

        {feature.yearNamed && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Год назвы: </span>
            <span className="text-black">{feature.yearNamed}</span>
          </div>
        )}

        {/* Admin-only: Comment field */}
        {isAdmin && feature.comment && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Камэнтар: </span>
            <span className="text-black">{feature.comment}</span>
          </div>
        )}
      </div>

      {/* Forum link - shown before dossier record details */}
      {forumFullUrl && (
        <a
          href={forumFullUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="flex items-center gap-1.5 text-primary hover:underline text-sm w-fit transition-colors"
        >
          <MessageSquare size={16} />
          Абмеркаваць на форуме
        </a>
      )}

      {/* Create discussion button - shown when authenticated and no forum link */}
      {isAuthenticated && !forumFullUrl && (
        <Button
          variant="secondary"
          size="sm"
          onClick={handleCreateDiscussion}
          disabled={isCreatingTopic}
          icon={isCreatingTopic ? <Loader2 size={18} className="animate-spin" /> : <MessageSquarePlus size={18} />}
          className="w-fit"
        >
          {isCreatingTopic ? 'Стварэньне...' : 'Стварыць абмеркаваньне'}
        </Button>
      )}

      {/* DossierRecord section - admin only, separated by horizontal line */}
      {hasAdminData && feature.dossierRecordId && (
        <div className="flex flex-col gap-2 pt-4 border-t border-black/10">
          {feature.dossierRecordNameBeTarask && (
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Этымалёгія: </span>
              <span className="text-black">{feature.dossierRecordNameBeTarask}</span>
            </div>
          )}

          {feature.dossierRecordClassification !== undefined && feature.dossierRecordClassification !== 0 && (
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Клясыфікацыя: </span>
              <span className="font-medium text-black">{getClassificationText(feature.dossierRecordClassification)}</span>
            </div>
          )}

          {feature.dossierRecordDescriptionBe && (
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Апісаньне: </span>
              <span className="text-black">{feature.dossierRecordDescriptionBe}</span>
            </div>
          )}

          {feature.dossierRecordDescriptionRu && (
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Апісаньне КА: </span>
              <span className="text-black">{feature.dossierRecordDescriptionRu}</span>
            </div>
          )}

          {feature.dossierRecordNamingCategoryId && (
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Катэгорыя: </span>
              <span className="text-black">
                {namingCategories.find(c => c.id === feature.dossierRecordNamingCategoryId)?.name || '...'}
              </span>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default FeatureInfoView;
