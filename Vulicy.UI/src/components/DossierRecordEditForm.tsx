import { useState } from 'react';
import { X, Loader2, Save, AlertCircle } from 'lucide-react';
import { TextField, SelectField } from './FormFields';
import Button from './Button';
import type { DossierRecordSearchResult, DossierRecordEditRequest } from '../types';
import { api } from '../utils/api';

// Classification options for dossier records (exclude 0 - records need explicit classification)
const DOSSIER_CLASSIFICATION_OPTIONS = [
  { value: 1, label: 'Перайменаваньне неабходнае ў прыярытэтным парадку' },
  { value: 2, label: 'Перайменаваньне неабходнае' },
  { value: 3, label: 'Перайменаваньне пажаданае' },
  { value: 4, label: 'Перайменаваньне магчымае' },
  { value: 5, label: 'Перайменаваньне не патрэбнае' },
];

interface DossierRecordEditFormProps {
  record?: DossierRecordSearchResult;
  onClose: () => void;
  onRecordUpdated: (record: DossierRecordSearchResult) => void;
}

/**
 * Form component for editing dossier record properties.
 * Shows all editable fields and handles save/cancel actions.
 */
const DossierRecordEditForm = ({
  record,
  onClose,
  onRecordUpdated,
}: DossierRecordEditFormProps) => {
  const isCreateMode = !record;
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

  // Edit form state
  const [editForm, setEditForm] = useState<DossierRecordEditRequest>({
    nameBeTarask: record?.nameBeTarask || '',
    nameBeNark: record?.nameBeNark || '',
    nameRu: record?.nameRu || null,
    descriptionBe: record?.descriptionBe || null,
    descriptionRu: record?.descriptionRu || null,
    classification: record?.classification || 1,
  });

  // Validation matching backend EditDossierRecordRequestValidator
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
      if (isCreateMode) {
        // Create mode: POST to create new record
        const response = await api.post<{ id: number }>('/api/dossier-records', editForm);

        // Create a new record object with the returned ID
        const newRecord: DossierRecordSearchResult = {
          id: response.id,
          nameBeTarask: editForm.nameBeTarask || undefined,
          nameBeNark: editForm.nameBeNark || undefined,
          nameRu: editForm.nameRu ?? undefined,
          descriptionBe: editForm.descriptionBe ?? undefined,
          descriptionRu: editForm.descriptionRu ?? undefined,
          classification: editForm.classification,
          numFeatures: 0,
        };
        onRecordUpdated(newRecord);
      } else {
        // Edit mode: PUT to update existing record
        await api.put(`/api/dossier-records/${record.id}`, editForm);

        // Update local record with new values
        const updatedRecord: DossierRecordSearchResult = {
          ...record,
          nameBeTarask: editForm.nameBeTarask || undefined,
          nameBeNark: editForm.nameBeNark || undefined,
          nameRu: editForm.nameRu ?? undefined,
          descriptionBe: editForm.descriptionBe ?? undefined,
          descriptionRu: editForm.descriptionRu ?? undefined,
          classification: editForm.classification,
        };
        onRecordUpdated(updatedRecord);
      }
      onClose();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Не атрымалася захаваць змены';
      setError(errorMessage);
      console.error('Failed to save dossier record:', err);
    } finally {
      setIsSaving(false);
    }
  };

  const updateField = <K extends keyof DossierRecordEditRequest>(key: K, value: DossierRecordEditRequest[K]) => {
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
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30" style={{ animation: 'fadeIn 150ms ease-out' }}>
      <div className="w-96 max-h-[80vh] glass overflow-y-auto p-6 flex flex-col gap-4 animate-in zoom-in-95 duration-200">
        {/* Header */}
        <div className="flex justify-between items-start">
          <h2 className="text-lg font-bold m-0">{isCreateMode ? 'Новае імя' : 'Рэдагаваньне імені'}</h2>
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
            value={editForm.nameRu || ''}
            onChange={(v) => updateField('nameRu', v || null)}
            maxLength={128}
            error={validationErrors.nameRu}
          />

          <TextField
            label="Апісаньне"
            value={editForm.descriptionBe || ''}
            onChange={(v) => updateField('descriptionBe', v || null)}
            maxLength={1024}
            multiline
          />

          <TextField
            label="Апісаньне КА"
            value={editForm.descriptionRu || ''}
            onChange={(v) => updateField('descriptionRu', v || null)}
            maxLength={1024}
            multiline
          />

          <SelectField
            label="Клясыфікацыя"
            value={editForm.classification}
            onChange={(v) => updateField('classification', v ?? 1)}
            options={DOSSIER_CLASSIFICATION_OPTIONS}
          />
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
            {isSaving ? 'Захаваньне...' : (isCreateMode ? 'Стварыць' : 'Захаваць')}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default DossierRecordEditForm;
