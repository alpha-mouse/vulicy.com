import { useState } from 'react';
import { X, Loader2, Save, AlertCircle } from 'lucide-react';
import { TextField, SelectField } from './FormFields';
import Button from './Button';
import DossierRecordPicker from './DossierRecordPicker';
import {
  CLASSIFICATION_OPTIONS,
  FEATURE_TYPE_OPTIONS,
} from '../constants/mapConstants';
import type { FeatureProperties, FeatureEditRequest, NamingCategory, DossierRecordSearchResult } from '../types';
import { api } from '../utils/api';

interface FeatureEditFormProps {
  feature: FeatureProperties;
  namingCategories: NamingCategory[];
  onClose: () => void;
  onFeatureUpdated: (featureId: number, updatedData?: Partial<FeatureProperties>) => void;
}

/**
 * Form component for editing feature properties.
 * Shows all editable fields and handles save/cancel actions.
 */
const FeatureEditForm = ({
  feature,
  namingCategories,
  onClose,
  onFeatureUpdated,
}: FeatureEditFormProps) => {
  const [isSaving, setIsSaving] = useState(false);
  const [isPickerOpen, setIsPickerOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

  // Edit form state
  const [editForm, setEditForm] = useState<FeatureEditRequest>({
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

  // Track selected dossier record name for display
  const [selectedDossierName, setSelectedDossierName] = useState<string | null>(
    feature.DossierRecordNameBeTarask || null
  );
  const [selectedDossierRecord, setSelectedDossierRecord] = useState<DossierRecordSearchResult | null>(null);

  // Validation matching backend FeatureEditRequestValidator
  const validate = (): boolean => {
    const errors: Record<string, string> = {};

    if (!editForm.nameBeTarask.trim()) {
      errors.nameBeTarask = 'Поле абавязковае';
    } else if (editForm.nameBeTarask.length > 128) {
      errors.nameBeTarask = 'Максымум 128 сымбаляў';
    }

    if (!editForm.nameBeNark.trim()) {
      errors.nameBeNark = 'Поле абавязковае';
    } else if (editForm.nameBeNark.length > 128) {
      errors.nameBeNark = 'Максымум 128 сымбаляў';
    }

    if (editForm.nameRu && editForm.nameRu.length > 128) {
      errors.nameRu = 'Максымум 128 сымбаляў';
    }

    if (editForm.renamingReason && editForm.renamingReason.length > 1024) {
      errors.renamingReason = 'Максымум 1024 сымбалі';
    }

    if (editForm.historicNames && editForm.historicNames.length > 256) {
      errors.historicNames = 'Максымум 256 сымбаляў';
    }

    if (editForm.comment && editForm.comment.length > 512) {
      errors.comment = 'Максымум 512 сымбаляў';
    }

    if (editForm.yearNamed && editForm.yearNamed.length > 64) {
      errors.yearNamed = 'Максымум 64 сымбалі';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSave = async () => {
    if (isSaving) return;

    if (!validate()) {
      return;
    }

    setIsSaving(true);
    setError(null);

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

      onFeatureUpdated(feature.Id, updatedProps);
      onClose();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Не атрымалася захаваць змены';
      setError(errorMessage);
      console.error('Failed to save feature:', err);
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
    // Clear validation error for this field when user starts typing
    if (validationErrors[key]) {
      setValidationErrors(prev => {
        const next = { ...prev };
        delete next[key];
        return next;
      });
    }
  };

  // Naming category options with "з прывязанага імені" for null
  const namingCategoryOptions = [
    { value: -1, label: 'з прывязанага імені' },
    ...namingCategories.map(c => ({ value: c.id, label: c.name })),
  ];

  return (
    <div className="absolute right-4 top-4 h-fit max-h-panel w-96 glass z-20 overflow-y-auto p-6 flex flex-col gap-4 animate-in">
      {/* Header */}
      <div className="flex justify-between items-start">
        <h2 className="text-lg font-bold m-0">Рэдагаваньне</h2>
        <button
          onClick={onClose}
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
          error={validationErrors.nameBeTarask}
        />
        <TextField
          label="Назва (акадэмічны)"
          value={editForm.nameBeNark}
          onChange={(v) => updateField('nameBeNark', v)}
          maxLength={128}
          error={validationErrors.nameBeNark}
        />
        <TextField
          label="Назва (расейская)"
          value={editForm.nameRu}
          onChange={(v) => updateField('nameRu', v)}
          maxLength={128}
          error={validationErrors.nameRu}
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
          error={validationErrors.renamingReason}
        />

        <TextField
          label="Гістарычныя назвы"
          value={editForm.historicNames || ''}
          onChange={(v) => updateField('historicNames', v || null)}
          maxLength={256}
          error={validationErrors.historicNames}
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
          error={validationErrors.yearNamed}
        />

        <TextField
          label="Камэнтар"
          value={editForm.comment || ''}
          onChange={(v) => updateField('comment', v || null)}
          maxLength={512}
          multiline
          error={validationErrors.comment}
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

      {/* Error Message */}
      {error && (
        <div className="flex items-start gap-2 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
          <AlertCircle size={16} className="shrink-0 mt-0.5" />
          <span>{error}</span>
        </div>
      )}

      {/* Action Buttons */}
      <div className="flex gap-2 pt-2">
        <Button
          variant="ghost"
          onClick={onClose}
          disabled={isSaving}
        >
          Скасаваць
        </Button>
        <Button
          variant="primary"
          onClick={handleSave}
          disabled={isSaving}
          icon={isSaving ? <Loader2 size={16} className="animate-spin" /> : <Save size={16} />}
          className="flex-1"
        >
          {isSaving ? 'Захаваньне...' : 'Захаваць'}
        </Button>
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
};

export default FeatureEditForm;
