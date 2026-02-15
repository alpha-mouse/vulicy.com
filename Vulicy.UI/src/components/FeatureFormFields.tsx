import { TextField, SelectField } from './FormFields';
import {
  CLASSIFICATION_OPTIONS,
  FEATURE_TYPE_OPTIONS,
} from '../constants/mapConstants';
import type { FeatureEditRequest, NamingCategory } from '../types';

interface FeatureFormFieldsProps {
  editForm: FeatureEditRequest;
  onFieldChange: <K extends keyof FeatureEditRequest>(key: K, value: FeatureEditRequest[K]) => void;
  validationErrors: Record<string, string>;
  namingCategories: NamingCategory[];
  selectedDossierName: string | null;
  onOpenDossierPicker: () => void;
}

/**
 * Reusable form fields for creating/editing a feature.
 * Used by both FeatureCreateDialog and FeatureEditForm.
 */
const FeatureFormFields = ({
  editForm,
  onFieldChange,
  validationErrors,
  namingCategories,
  selectedDossierName,
  onOpenDossierPicker,
}: FeatureFormFieldsProps) => {
  // Naming category options with "з прывязанага імені" for null
  const namingCategoryOptions = [
    { value: -1, label: 'з прывязанага імені' },
    ...namingCategories.map(c => ({ value: c.id, label: c.name })),
  ];

  return (
    <div className="flex flex-col gap-3">
      <TextField
        label="Назва (клясычны)"
        value={editForm.nameBeTarask}
        onChange={(v) => onFieldChange('nameBeTarask', v)}
        maxLength={128}
        error={validationErrors.nameBeTarask}
        required
      />
      <TextField
        label="Назва (акадэмічны)"
        value={editForm.nameBeNark}
        onChange={(v) => onFieldChange('nameBeNark', v)}
        maxLength={128}
        error={validationErrors.nameBeNark}
        required
      />
      <TextField
        label="Назва (расейская)"
        value={editForm.nameRu}
        onChange={(v) => onFieldChange('nameRu', v)}
        maxLength={128}
        error={validationErrors.nameRu}
      />

      <SelectField
        label="Тып"
        value={editForm.type}
        onChange={(v) => onFieldChange('type', v ?? 0)}
        options={FEATURE_TYPE_OPTIONS}
      />

      <SelectField
        label="Клясыфікацыя"
        value={editForm.classification}
        onChange={(v) => onFieldChange('classification', v ?? 0)}
        options={CLASSIFICATION_OPTIONS}
      />

      <TextField
        label="Абгрунтаваньне"
        value={editForm.renamingReason || ''}
        onChange={(v) => onFieldChange('renamingReason', v || null)}
        maxLength={1024}
        multiline
        error={validationErrors.renamingReason}
      />

      <TextField
        label="Гістарычныя назвы"
        value={editForm.historicNames || ''}
        onChange={(v) => onFieldChange('historicNames', v || null)}
        maxLength={256}
        error={validationErrors.historicNames}
      />

      <div className="flex items-center gap-2">
        <input
          type="checkbox"
          id="historicPossible"
          checked={editForm.historicPossible}
          onChange={(e) => onFieldChange('historicPossible', e.target.checked)}
          className="w-4 h-4 cursor-pointer"
        />
        <label htmlFor="historicPossible" className="text-sm text-black cursor-pointer">
          Магчымае вяртаньне гістарычнай
        </label>
      </div>

      <TextField
        label="Год назвы"
        value={editForm.yearNamed || ''}
        onChange={(v) => onFieldChange('yearNamed', v || null)}
        maxLength={64}
        error={validationErrors.yearNamed}
      />

      <TextField
        label="Камэнтар"
        value={editForm.comment || ''}
        onChange={(v) => onFieldChange('comment', v || null)}
        maxLength={512}
        multiline
        error={validationErrors.comment}
      />

      <SelectField
        label="Катэгорыя"
        value={editForm.namingCategoryId === null ? -1 : editForm.namingCategoryId}
        onChange={(v) => onFieldChange('namingCategoryId', v === -1 ? null : v)}
        options={namingCategoryOptions}
      />

      {/* Dossier Record Picker Trigger */}
      <div className="flex flex-col gap-1">
        <label className="text-xs text-black/50">Імя (дасье)</label>
        <button
          onClick={onOpenDossierPicker}
          className="text-sm p-2 border border-black/20 dark:border-white/10 rounded-lg bg-white/50 dark:bg-black/20 text-left cursor-pointer hover:border-primary transition-colors"
        >
          {selectedDossierName || (editForm.dossierRecordId ? `#${editForm.dossierRecordId}` : '(не прывязана)')}
        </button>
      </div>
    </div>
  );
};

/**
 * Validation logic for FeatureEditRequest, matching backend FeatureEditRequestValidator.
 * Returns a Record of field names to error messages.
 */
export function validateFeatureForm(editForm: FeatureEditRequest): Record<string, string> {
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

  return errors;
}

export default FeatureFormFields;
