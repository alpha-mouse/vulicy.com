import { useState, useRef } from 'react';
import { X, Link, Check, MessageSquarePlus, MessageSquare, Loader2, Pencil, Save } from 'lucide-react';
import {
  FEATURE_TYPE_LABELS,
  getClassificationText,
  CLASSIFICATION_OPTIONS,
  FEATURE_TYPE_OPTIONS,
} from '../constants/mapConstants';
import type { FeatureProperties, NamingCategory, FeatureEditRequest, DossierRecordSearchResult } from '../types/feature';
import { api } from '../utils/api';
import DossierRecordPicker from './DossierRecordPicker';

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

interface FeatureInfoContentProps {
  feature: FeatureProperties;
  namingCategories: NamingCategory[];
  isCopied: boolean;
  onCopyLink: () => void;
  onClose: () => void;
  isAdmin: boolean;
  isAuthenticated: boolean;
  discourseBaseUrl?: string;
  onForumLinkCreated?: (featureId: number, forumLink: string) => void;
  onFeatureUpdated?: (featureId: number, updatedData?: Partial<FeatureProperties>) => void;
}

// Text input field component
const TextField = ({
  label,
  value,
  onChange,
  maxLength,
  multiline = false,
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
  maxLength: number;
  multiline?: boolean;
}) => (
  <div className="flex flex-col gap-1">
    <label className="text-xs text-black/50">{label}</label>
    {multiline ? (
      <textarea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        maxLength={maxLength}
        rows={3}
        className="text-sm p-2 border border-black/20 rounded-lg bg-white/50 outline-none focus:border-primary resize-none"
      />
    ) : (
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        maxLength={maxLength}
        className="text-sm p-2 border border-black/20 rounded-lg bg-white/50 outline-none focus:border-primary"
      />
    )}
  </div>
);

// Select dropdown component
const SelectField = ({
  label,
  value,
  onChange,
  options,
}: {
  label: string;
  value: number | null;
  onChange: (v: number | null) => void;
  options: { value: number; label: string }[];
}) => (
  <div className="flex flex-col gap-1">
    <label className="text-xs text-black/50">{label}</label>
    <select
      value={value ?? ''}
      onChange={(e) => onChange(e.target.value === '' ? null : Number(e.target.value))}
      className="text-sm p-2 border border-black/20 rounded-lg bg-white/50 outline-none focus:border-primary cursor-pointer"
    >
      {options.map((opt) => (
        <option key={opt.value} value={opt.value}>
          {opt.label}
        </option>
      ))}
    </select>
  </div>
);

// Inner component that handles actual feature display - receives non-null feature
const FeatureInfoContent = ({
  feature,
  namingCategories,
  isCopied,
  onCopyLink,
  onClose,
  isAdmin,
  isAuthenticated,
  discourseBaseUrl,
  onForumLinkCreated,
  onFeatureUpdated,
}: FeatureInfoContentProps) => {
  const currentIdRef = useRef(feature.Id);
  currentIdRef.current = feature.Id;

  const [isCreatingTopic, setIsCreatingTopic] = useState(false);
  const [localForumLink, setLocalForumLink] = useState<string | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isPickerOpen, setIsPickerOpen] = useState(false);

  // Edit form state
  const [editForm, setEditForm] = useState<FeatureEditRequest>(() => ({
    nameBeTarask: feature.NameBeTarask || '',
    nameBeNark: feature.NameBeNark || '',
    nameRu: feature.NameRu || '',
    classification: feature.Classification,
    type: feature.Type,
    renamingReason: feature.RenamingReason || null,
    historicNames: feature.HistoricNames || null,
    comment: feature.Comment || null,
    historicPossible: feature.HistoricPossible,
    yearNamed: feature.YearNamed?.toString() || null,
    namingCategoryId: feature.NamingCategoryId || null,
    dossierRecordId: feature.DossierRecordId || null,
  }));

  // Track selected dossier record name for display
  const [selectedDossierName, setSelectedDossierName] = useState<string | null>(
    feature.DossierRecordNameBeTarask || null
  );
  const [selectedDossierRecord, setSelectedDossierRecord] = useState<DossierRecordSearchResult | null>(null);

  // Reset local state when switching features without re-mounting
  const [prevFeatureId, setPrevFeatureId] = useState(feature.Id);
  if (feature.Id !== prevFeatureId) {
    setPrevFeatureId(feature.Id);
    setLocalForumLink(null);
    setIsCreatingTopic(false);
    setIsEditing(false);
    setIsSaving(false);
    // Reset edit form to new feature
    setEditForm({
      nameBeTarask: feature.NameBeTarask || '',
      nameBeNark: feature.NameBeNark || '',
      nameRu: feature.NameRu || '',
      classification: feature.Classification,
      type: feature.Type,
      renamingReason: feature.RenamingReason || null,
      historicNames: feature.HistoricNames || null,
      comment: feature.Comment || null,
      historicPossible: feature.HistoricPossible,
      yearNamed: feature.YearNamed?.toString() || null,
      namingCategoryId: feature.NamingCategoryId || null,
      dossierRecordId: feature.DossierRecordId || null,
    });
    setSelectedDossierName(feature.DossierRecordNameBeTarask || null);
    setSelectedDossierRecord(null);
  }

  const effectiveForumLink = localForumLink || feature.ForumRelativeLink;
  const forumFullUrl = effectiveForumLink && discourseBaseUrl
    ? `${discourseBaseUrl}${effectiveForumLink.startsWith('/') ? '' : '/'}${effectiveForumLink}`
    : null;
  const hasAdminData = isAdmin && (feature.Comment || feature.DossierRecordId);

  const handleCreateDiscussion = async () => {
    if (isCreatingTopic) return;
    const targetFeatureId = feature.Id;
    setIsCreatingTopic(true);

    try {
      const data = await api.post<{ forumRelativeLink: string }>('/api/forum/create-topic', {
        featureId: targetFeatureId
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

  const handleStartEdit = () => {
    setEditForm({
      nameBeTarask: feature.NameBeTarask || '',
      nameBeNark: feature.NameBeNark || '',
      nameRu: feature.NameRu || '',
      classification: feature.Classification,
      type: feature.Type,
      renamingReason: feature.RenamingReason || null,
      historicNames: feature.HistoricNames || null,
      comment: feature.Comment || null,
      historicPossible: feature.HistoricPossible,
      yearNamed: feature.YearNamed?.toString() || null,
      namingCategoryId: feature.NamingCategoryId || null,
      dossierRecordId: feature.DossierRecordId || null,
    });
    setSelectedDossierName(feature.DossierRecordNameBeTarask || null);
    setIsEditing(true);
  };

  const handleCancelEdit = () => {
    setIsEditing(false);
  };

  const handleSave = async () => {
    if (isSaving) return;
    setIsSaving(true);

    try {
      await api.put(`/api/features/${feature.Id}`, editForm);

      const updatedProps: Partial<FeatureProperties> = {
        NameBeTarask: editForm.nameBeTarask || undefined,
        NameBeNark: editForm.nameBeNark || undefined,
        NameRu: editForm.nameRu || undefined,
        Classification: editForm.classification,
        Type: editForm.type,
        RenamingReason: editForm.renamingReason ?? undefined,
        HistoricNames: editForm.historicNames ?? undefined,
        HistoricPossible: editForm.historicPossible,
        Comment: editForm.comment ?? undefined,
        YearNamed: editForm.yearNamed ? parseInt(editForm.yearNamed) : undefined,
        NamingCategoryId: editForm.namingCategoryId ?? undefined,
        DossierRecordId: editForm.dossierRecordId ?? undefined,
        DossierRecordNameBeTarask: selectedDossierName ?? undefined
      };

      // If dossier record changed, update related fields
      if (editForm.dossierRecordId !== feature.DossierRecordId) {
        if (!editForm.dossierRecordId) {
          // Unlinked - clear fields
          Object.assign(updatedProps, {
            DossierRecordDescriptionBe: undefined,
            DossierRecordDescriptionRu: undefined,
            DossierRecordClassification: undefined,
            DossierRecordNamingCategoryId: undefined
          });
        } else if (selectedDossierRecord) {
          // Linked new - update fields
          Object.assign(updatedProps, {
            DossierRecordDescriptionBe: selectedDossierRecord.descriptionBe,
            DossierRecordDescriptionRu: selectedDossierRecord.descriptionRu,
            DossierRecordClassification: selectedDossierRecord.classification,
            DossierRecordNamingCategoryId: selectedDossierRecord.namingCategoryId
          });
        }
      }

      setIsEditing(false);
      onFeatureUpdated?.(feature.Id, updatedProps);
    } catch (error) {
      console.error('Failed to save feature:', error);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDossierSelect = (record: DossierRecordSearchResult | null) => {
    if (record) {
      setEditForm(prev => ({ ...prev, dossierRecordId: record.id }));
      setSelectedDossierName(record.nameBeTarask || record.nameBeNark || record.nameRu || null);
      setSelectedDossierRecord(record);
    } else {
      setEditForm(prev => ({ ...prev, dossierRecordId: null, namingCategoryId: null }));
      setSelectedDossierName(null);
      setSelectedDossierRecord(null);
    }
  };

  const updateField = <K extends keyof FeatureEditRequest>(key: K, value: FeatureEditRequest[K]) => {
    setEditForm(prev => ({ ...prev, [key]: value }));
  };

  // Naming category options with "з прывязанага імені" for null
  const namingCategoryOptions = [
    { value: -1, label: 'з прывязанага імені' },
    ...namingCategories.map(c => ({ value: c.id, label: c.name })),
  ];

  // Render edit mode
  if (isEditing) {
    return (
      <div className="absolute right-4 top-4 h-fit max-h-panel w-96 glass z-20 overflow-y-auto p-6 flex flex-col gap-4 animate-in">
        {/* Header */}
        <div className="flex justify-between items-start">
          <h2 className="text-lg font-bold m-0">Рэдагаваньне</h2>
          <button
            onClick={handleCancelEdit}
            className="text-black/30 hover:text-black/60 transition-colors p-0 appearance-none bg-transparent border-none cursor-pointer outline-none"
          >
            <X size={20} strokeWidth={1.2} />
          </button>
        </div>

        {/* Form Fields */}
        <div className="flex flex-col gap-3">
          <TextField
            label="Назва (клясычны)"
            value={editForm.nameBeTarask}
            onChange={(v) => updateField('nameBeTarask', v)}
            maxLength={128}
          />
          <TextField
            label="Назва (акадэмічны)"
            value={editForm.nameBeNark}
            onChange={(v) => updateField('nameBeNark', v)}
            maxLength={128}
          />
          <TextField
            label="Назва (расейская)"
            value={editForm.nameRu}
            onChange={(v) => updateField('nameRu', v)}
            maxLength={128}
          />

          <SelectField
            label="Тып"
            value={editForm.type}
            onChange={(v) => updateField('type', v ?? 0)}
            options={FEATURE_TYPE_OPTIONS}
          />

          <SelectField
            label="Клясыфікацыя"
            value={editForm.classification}
            onChange={(v) => updateField('classification', v ?? 0)}
            options={CLASSIFICATION_OPTIONS}
          />

          <TextField
            label="Абгрунтаваньне"
            value={editForm.renamingReason || ''}
            onChange={(v) => updateField('renamingReason', v || null)}
            maxLength={1024}
            multiline
          />

          <TextField
            label="Гістарычныя назвы"
            value={editForm.historicNames || ''}
            onChange={(v) => updateField('historicNames', v || null)}
            maxLength={256}
          />

          <div className="flex items-center gap-2">
            <input
              type="checkbox"
              id="historicPossible"
              checked={editForm.historicPossible}
              onChange={(e) => updateField('historicPossible', e.target.checked)}
              className="w-4 h-4 cursor-pointer"
            />
            <label htmlFor="historicPossible" className="text-sm text-black cursor-pointer">
              Магчымае вяртаньне гістарычнай
            </label>
          </div>

          <TextField
            label="Год назвы"
            value={editForm.yearNamed || ''}
            onChange={(v) => updateField('yearNamed', v || null)}
            maxLength={64}
          />

          <TextField
            label="Камэнтар"
            value={editForm.comment || ''}
            onChange={(v) => updateField('comment', v || null)}
            maxLength={512}
            multiline
          />

          <SelectField
            label="Катэгорыя"
            value={editForm.namingCategoryId === null ? -1 : editForm.namingCategoryId}
            onChange={(v) => updateField('namingCategoryId', v === -1 ? null : v)}
            options={namingCategoryOptions}
          />

          {/* Dossier Record Picker */}
          <div className="flex flex-col gap-1">
            <label className="text-xs text-black/50">Імя (дасье)</label>
            <button
              onClick={() => setIsPickerOpen(true)}
              className="text-sm p-2 border border-black/20 rounded-lg bg-white/50 text-left cursor-pointer hover:border-primary transition-colors"
            >
              {selectedDossierName || '(не прывязана)'}
            </button>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-2 pt-2">
          <button
            onClick={handleCancelEdit}
            disabled={isSaving}
            className="px-4 py-2 text-sm font-medium bg-black/5 text-black rounded-lg hover:bg-black/10 transition-colors border-none cursor-pointer outline-none disabled:opacity-50"
          >
            Скасаваць
          </button>
          <button
            onClick={handleSave}
            disabled={isSaving}
            className="flex-1 flex items-center justify-center gap-2 px-4 py-2 text-sm font-medium bg-primary text-white rounded-lg hover:bg-primary/90 transition-colors border-none cursor-pointer outline-none disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSaving ? (
              <>
                <Loader2 size={16} className="animate-spin" />
                Захаваньне...
              </>
            ) : (
              <>
                <Save size={16} />
                Захаваць
              </>
            )}
          </button>
        </div>

        {/* Dossier Record Picker Modal */}
        <DossierRecordPicker
          isOpen={isPickerOpen}
          onClose={() => setIsPickerOpen(false)}
          onSelect={handleDossierSelect}
          namingCategories={namingCategories}
          currentRecordId={editForm.dossierRecordId}
          initialQuery={selectedDossierName || editForm.nameBeTarask}
        />
      </div>
    );
  }

  // Render view mode
  return (
    <div className="absolute right-4 top-4 h-fit max-h-panel w-96 glass z-20 overflow-y-auto p-6 flex flex-col gap-3 animate-in">
      <div className="flex justify-between items-start">
        <h2 className="text-2xl font-bold leading-tight m-0">
          {FEATURE_TYPE_LABELS[feature.Type] && (
            <span className="text-black/40 font-medium">{FEATURE_TYPE_LABELS[feature.Type]} </span>
          )}
          {feature.NameBeTarask || feature.NameBeNark || feature.NameRu}
          <button
            onClick={onCopyLink}
            className="ml-2 p-1 inline-flex items-center justify-center text-black/30 hover:text-black/60 transition-colors bg-transparent border-none cursor-pointer outline-none align-middle"
            title="Скапіяваць спасылку"
          >
            {isCopied ? <Check size={16} className="text-green-500" /> : <Link size={16} />}
          </button>
          {isAdmin && (
            <button
              onClick={handleStartEdit}
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
        {feature.Classification !== 0 && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Клясыфікацыя: </span>
            <span className="font-medium text-black">{getClassificationText(feature.Classification)}</span>
          </div>
        )}

        {feature.EtymologyBeTarask && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Этымалёгія: </span>
            <span className="text-black">{feature.EtymologyBeTarask}</span>
          </div>
        )}

        {feature.RenamingReason && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Абгрунтаваньне: </span>
            <span className="text-black">{feature.RenamingReason}</span>
          </div>
        )}

        {feature.NamingCategoryId && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Катэгорыя: </span>
            <span className="text-black">
              {namingCategories.find(c => c.id === feature.NamingCategoryId)?.name || '...'}
            </span>
          </div>
        )}

        {feature.HistoricNames && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Гістарычная(-ыя) назва(-ы): </span>
            <span className="text-black italic">{feature.HistoricNames}</span>
          </div>
        )}

        {feature.YearNamed && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Год назвы: </span>
            <span className="text-black">{feature.YearNamed}</span>
          </div>
        )}

        {/* Admin-only: Comment field */}
        {isAdmin && feature.Comment && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Камэнтар: </span>
            <span className="text-black">{feature.Comment}</span>
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
        <button
          onClick={handleCreateDiscussion}
          disabled={isCreatingTopic}
          className="flex items-center justify-center gap-2 px-4 py-1.5 text-sm font-medium bg-secondary text-black rounded-lg hover:bg-secondary-hover transition-colors border border-black/10 cursor-pointer outline-none shadow-sm disabled:opacity-50 disabled:cursor-not-allowed w-fit"
        >
          {isCreatingTopic ? (
            <>
              <Loader2 size={18} className="animate-spin" />
              Стварэньне...
            </>
          ) : (
            <>
              <MessageSquarePlus size={18} />
              Стварыць абмеркаваньне
            </>
          )}
        </button>
      )}

      {/* DossierRecord section - admin only, separated by horizontal line */}
      {hasAdminData && feature.DossierRecordId && (
        <div className="flex flex-col gap-2 pt-4 border-t border-black/10">
          {feature.DossierRecordNameBeTarask && (
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Этымалёгія: </span>
              <span className="text-black">{feature.DossierRecordNameBeTarask}</span>
            </div>
          )}

          {feature.DossierRecordClassification !== undefined && feature.DossierRecordClassification !== 0 && (
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Клясыфікацыя: </span>
              <span className="font-medium text-black">{getClassificationText(feature.DossierRecordClassification)}</span>
            </div>
          )}

          {feature.DossierRecordDescriptionBe && (
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Апісаньне: </span>
              <span className="text-black">{feature.DossierRecordDescriptionBe}</span>
            </div>
          )}

          {feature.DossierRecordDescriptionRu && (
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Апісаньне КА: </span>
              <span className="text-black">{feature.DossierRecordDescriptionRu}</span>
            </div>
          )}

          {feature.DossierRecordNamingCategoryId && (
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Катэгорыя: </span>
              <span className="text-black">
                {namingCategories.find(c => c.id === feature.DossierRecordNamingCategoryId)?.name || '...'}
              </span>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

// Main wrapper component - handles loading state
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

  // Render feature content
  return (
    <FeatureInfoContent
      feature={feature}
      namingCategories={namingCategories}
      isCopied={isCopied}
      onCopyLink={onCopyLink}
      onClose={onClose}
      isAdmin={isAdmin}
      isAuthenticated={isAuthenticated}
      discourseBaseUrl={discourseBaseUrl}
      onForumLinkCreated={onForumLinkCreated}
      onFeatureUpdated={onFeatureUpdated}
    />
  );
};

export default FeatureInfoPanel;
