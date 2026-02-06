import { useState, useCallback } from 'react';
import { X, Loader2, Save, AlertCircle } from 'lucide-react';
import Button from './Button';
import DossierRecordPicker from './DossierRecordPicker';
import FeatureFormFields, { validateFeatureForm } from './FeatureFormFields';
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
    nameBeTarask: feature.nameBeTarask || '',
    nameBeNark: feature.nameBeNark || '',
    nameRu: feature.nameRu || '',
    classification: feature.classification,
    type: feature.type,
    renamingReason: feature.renamingReason || null,
    historicNames: feature.historicNames || null,
    comment: feature.comment || null,
    historicPossible: feature.historicPossible,
    yearNamed: feature.yearNamed || null,
    namingCategoryId: feature.namingCategoryId || null,
    dossierRecordId: feature.dossierRecordId || null,
  });

  // Track selected dossier record name for display
  const [selectedDossierName, setSelectedDossierName] = useState<string | null>(
    feature.dossierRecordNameBeTarask || null
  );
  const [selectedDossierRecord, setSelectedDossierRecord] = useState<DossierRecordSearchResult | null>(null);

  const handleSave = useCallback(async () => {
    if (isSaving) return;

    const errors = validateFeatureForm(editForm);
    if (Object.keys(errors).length > 0) {
      setValidationErrors(errors);
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      await api.put(`/api/features/${feature.id}`, editForm);

      const updatedProps: Partial<FeatureProperties> = {
        nameBeTarask: editForm.nameBeTarask || undefined,
        nameBeNark: editForm.nameBeNark || undefined,
        nameRu: editForm.nameRu || undefined,
        classification: editForm.classification,
        type: editForm.type,
        renamingReason: editForm.renamingReason ?? undefined,
        historicNames: editForm.historicNames ?? undefined,
        historicPossible: editForm.historicPossible,
        comment: editForm.comment ?? undefined,
        yearNamed: editForm.yearNamed ?? undefined,
        namingCategoryId: editForm.namingCategoryId ?? undefined,
        dossierRecordId: editForm.dossierRecordId ?? undefined,
        dossierRecordNameBeTarask: selectedDossierName ?? undefined
      };

      // If dossier record changed, update related fields
      if (editForm.dossierRecordId !== feature.dossierRecordId) {
        if (!editForm.dossierRecordId) {
          // Unlinked - clear fields
          Object.assign(updatedProps, {
            dossierRecordDescriptionBe: undefined,
            dossierRecordDescriptionRu: undefined,
            dossierRecordClassification: undefined,
            dossierRecordNamingCategoryId: undefined
          });
        } else if (selectedDossierRecord) {
          // Linked new - update fields
          Object.assign(updatedProps, {
            dossierRecordDescriptionBe: selectedDossierRecord.descriptionBe,
            dossierRecordDescriptionRu: selectedDossierRecord.descriptionRu,
            dossierRecordClassification: selectedDossierRecord.classification,
            dossierRecordNamingCategoryId: selectedDossierRecord.namingCategoryId
          });
        }
      }

      onFeatureUpdated(feature.id, updatedProps);
      onClose();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Не атрымалася захаваць змены';
      setError(errorMessage);
      console.error('Failed to save feature:', err);
    } finally {
      setIsSaving(false);
    }
  }, [editForm, feature, isSaving, onClose, onFeatureUpdated, selectedDossierName, selectedDossierRecord]);

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
      <FeatureFormFields
        editForm={editForm}
        onFieldChange={updateField}
        validationErrors={validationErrors}
        namingCategories={namingCategories}
        selectedDossierName={selectedDossierName}
        onOpenDossierPicker={() => setIsPickerOpen(true)}
      />

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
