import { useState, useEffect, useCallback, useRef } from 'react';
import { X, Loader2, Save, AlertCircle, Plus } from 'lucide-react';
import Button from './Button';
import DossierRecordPicker from './DossierRecordPicker';
import FeatureFormFields, { validateFeatureForm } from './FeatureFormFields';
import type {
  FeatureEditRequest,
  FeaturePreviewRequest,
  FeaturePreviewResponse,
  FeatureCreateFromSourcesRequest,
  SearchResult,
  NamingCategory,
  DossierRecordSearchResult,
} from '../types';
import type { OsmFeature, CadastreFeature } from '../types/source-feature';
import { api } from '../utils/api';

interface FeatureCreateDialogProps {
  osmFeature: OsmFeature;
  cadastreFeature: CadastreFeature;
  onClose: () => void;
  onCreated: (feature: SearchResult) => void;
}

/**
 * Dialog for creating a new Vulicy feature from OSM and Cadastre sources.
 * Fetches preview data, shows edit form, and handles creation.
 */
const FeatureCreateDialog = ({
  osmFeature,
  cadastreFeature,
  onClose,
  onCreated,
}: FeatureCreateDialogProps) => {
  const hasFetched = useRef(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isPickerOpen, setIsPickerOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});
  const [namingCategories, setNamingCategories] = useState<NamingCategory[]>([]);
  const [previewData, setPreviewData] = useState<FeaturePreviewResponse | null>(null);

  // Form state
  const [editForm, setEditForm] = useState<FeatureEditRequest>({
    nameBeTarask: '',
    nameBeNark: '',
    nameRu: '',
    classification: 0,
    type: 0,
    renamingReason: null,
    historicNames: null,
    comment: null,
    historicPossible: false,
    yearNamed: null,
    namingCategoryId: null,
    dossierRecordId: null,
  });

  const [selectedDossierName, setSelectedDossierName] = useState<string | null>(null);

  useEffect(() => {
    if (hasFetched.current) return;
    hasFetched.current = true;

    const fetchData = async () => {
      setIsLoading(true);
      setError(null);

      let preview: FeaturePreviewResponse | null = null;
      let categories: NamingCategory[] = [];
      try {
        const previewRequest: FeaturePreviewRequest = {
          osmId: osmFeature.id,
          osmType: osmFeature.type,
          cadastreId: cadastreFeature.id,
        };

        [preview, categories] = await Promise.all([
          api.post<FeaturePreviewResponse>('/api/features/preview', previewRequest),
          api.get<NamingCategory[]>('/api/map/naming-categories'),
        ]);

      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Памылка запыту даных';
        setError(errorMessage);
        console.error('Failed to fetch preview:', err);
      } finally {
        setIsLoading(false);
      }

      if (!preview || !categories) return;

      setPreviewData(preview);
      setNamingCategories(categories);

      setEditForm({
        nameBeTarask: preview.nameBeTarask || '',
        nameBeNark: preview.nameBeNark || '',
        nameRu: preview.nameRu || '',
        classification: preview.classification,
        type: preview.type,
        renamingReason: preview.renamingReason,
        historicNames: preview.historicNames,
        comment: preview.comment,
        historicPossible: preview.historicPossible,
        yearNamed: preview.yearNamed,
        namingCategoryId: preview.namingCategoryId,
        dossierRecordId: preview.dossierRecordId,
      });

      if (preview.dossierRecordNameBeTarask) {
        setSelectedDossierName(preview.dossierRecordNameBeTarask);
      }
    };

    fetchData();
  }, [osmFeature.id, osmFeature.type, cadastreFeature.id]);

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
      const createRequest: FeatureCreateFromSourcesRequest = {
        ...editForm,
        osmId: osmFeature.id,
        osmType: osmFeature.type,
        cadastreId: cadastreFeature.id,
      };

      const createdFeature = await api.post<SearchResult>('/api/features/from-sources', createRequest);
      onCreated(createdFeature);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Не атрымалася стварыць вуліцу';
      setError(errorMessage);
      console.error('Failed to create feature:', err);
    } finally {
      setIsSaving(false);
    }
  }, [editForm, osmFeature, cadastreFeature, isSaving, onCreated]);

  const handleDossierSelect = (record: DossierRecordSearchResult | null) => {
    if (record) {
      setEditForm(prev => ({ ...prev, dossierRecordId: record.id }));
      setSelectedDossierName(record.nameBeTarask || record.nameBeNark || record.nameRu || null);
    } else {
      setEditForm(prev => ({ ...prev, dossierRecordId: null, namingCategoryId: null }));
      setSelectedDossierName(null);
    }
  };

  const updateField = <K extends keyof FeatureEditRequest>(key: K, value: FeatureEditRequest[K]) => {
    setEditForm(prev => ({ ...prev, [key]: value }));
    if (validationErrors[key]) {
      setValidationErrors(prev => {
        const next = { ...prev };
        delete next[key];
        return next;
      });
    }
  };

  return (
    <div className="absolute left-1/2 -translate-x-1/2 top-4 h-fit max-h-panel w-96 glass z-20 overflow-y-auto p-6 flex flex-col gap-4 animate-in">
      {/* Header */}
      <div className="flex justify-between items-start">
        <div className="flex items-center gap-2">
          <Plus size={20} className="text-primary" />
          <h2 className="text-lg font-bold m-0">Новая вуліца</h2>
        </div>
        <button
          onClick={onClose}
          className="text-black/30 hover:text-black/60 transition-colors p-0 appearance-none bg-transparent border-none cursor-pointer outline-none"
        >
          <X size={20} strokeWidth={1.2} />
        </button>
      </div>

      {/* Loading State */}
      {isLoading && (
        <div className="flex flex-col items-center justify-center py-12 gap-3">
          <Loader2 size={32} className="animate-spin text-primary" />
          <span className="text-sm text-black/50">Загрузка...</span>
        </div>
      )}

      {/* Error State (when loading failed) */}
      {!isLoading && error && !previewData && (
        <div className="flex flex-col items-center gap-4 py-8">
          <div className="flex items-start gap-2 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
            <AlertCircle size={16} className="shrink-0 mt-0.5" />
            <span>{error}</span>
          </div>
          <Button variant="ghost" onClick={onClose}>
            Зачыніць
          </Button>
        </div>
      )}

      {/* Form */}
      {!isLoading && previewData && (
        <>
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
              {isSaving ? 'Стварэньне...' : 'Стварыць'}
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
        </>
      )}
    </div>
  );
};

export default FeatureCreateDialog;
