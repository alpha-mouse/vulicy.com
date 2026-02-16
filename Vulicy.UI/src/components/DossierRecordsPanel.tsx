import { useState, useEffect, useRef, useCallback } from 'react';
import { ChevronLeft, Search as SearchIcon, X, MapPin, ChevronDown, ChevronUp, Loader2, FileUser, Plus } from 'lucide-react';
import type { SearchResult, DossierRecordSearchResult, NamingCategory } from '../types';
import { api } from '../utils/api';
import FeatureListItem from './FeatureListItem';
import DossierRecordItem from './DossierRecordItem';
import DossierRecordEditForm from './DossierRecordEditForm';
import MergeDialog from './MergeDialog';
import { CLASSIFICATION_COLORS } from '../constants/mapConstants';

interface DossierRecordsPanelProps {
  isOpen: boolean;
  onClose: () => void;
  onFeatureClick: (feature: SearchResult) => void;
  namingCategories: NamingCategory[];
  isAdmin?: boolean;
}

const PAGE_SIZE = 50;

const DossierRecordsPanel = ({
  isOpen,
  onClose,
  onFeatureClick,
  namingCategories,
  isAdmin = false,
}: DossierRecordsPanelProps) => {
  const [query, setQuery] = useState('');
  const [records, setRecords] = useState<DossierRecordSearchResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMore] = useState(false);
  const [expandedId, setExpandedId] = useState<number | null>(null);
  const [features, setFeatures] = useState<Record<number, SearchResult[]>>({});
  const [loadingFeatures, setLoadingFeatures] = useState<Record<number, boolean>>({});

  // Edit/delete/create state
  const [editingRecord, setEditingRecord] = useState<DossierRecordSearchResult | null>(null);
  const [isCreatingRecord, setIsCreatingRecord] = useState(false);
  const [deletingRecord, setDeletingRecord] = useState<DossierRecordSearchResult | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [mergingRecord, setMergingRecord] = useState<DossierRecordSearchResult | null>(null);

  const scrollRef = useRef<HTMLDivElement>(null);
  const sentinelRef = useRef<HTMLDivElement>(null);
  const skipRef = useRef(0);

  // Search for records
  const searchRecords = useCallback(async (searchQuery: string, skip: number, append: boolean) => {
    setIsLoading(true);
    try {
      const data = await api.get<DossierRecordSearchResult[]>('/api/dossier-records/search', {
        query: searchQuery || undefined,
        skip,
        take: PAGE_SIZE,
      });

      if (append) {
        setRecords(prev => [...prev, ...data]);
      } else {
        setRecords(data);
      }
      setHasMore(data.length === PAGE_SIZE);
      skipRef.current = skip + data.length;
    } catch (error) {
      console.error('Failed to search dossier records:', error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Initial load and search on query change
  useEffect(() => {
    if (!isOpen) return;

    const timer = setTimeout(() => {
      skipRef.current = 0;
      searchRecords(query, 0, false);
    }, 300);

    return () => clearTimeout(timer);
  }, [query, isOpen, searchRecords]);

  // Infinite scroll with intersection observer
  useEffect(() => {
    if (!isOpen || !sentinelRef.current) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasMore && !isLoading) {
          searchRecords(query, skipRef.current, true);
        }
      },
      { threshold: 0.1 }
    );

    observer.observe(sentinelRef.current);
    return () => observer.disconnect();
  }, [isOpen, hasMore, isLoading, query, searchRecords]);

  // Load features when expanding a record
  const handleExpand = useCallback(async (recordId: number) => {
    if (expandedId === recordId) {
      setExpandedId(null);
      return;
    }

    setExpandedId(recordId);

    // Load features if not already loaded
    if (!features[recordId] && !loadingFeatures[recordId]) {
      setLoadingFeatures(prev => ({ ...prev, [recordId]: true }));
      try {
        const data = await api.get<SearchResult[]>(`/api/dossier-records/${recordId}/features`);
        setFeatures(prev => ({ ...prev, [recordId]: data }));
      } catch (error) {
        console.error('Failed to load features:', error);
      } finally {
        setLoadingFeatures(prev => ({ ...prev, [recordId]: false }));
      }
    }
  }, [expandedId, features, loadingFeatures]);

  const handleClear = () => {
    setQuery('');
  };

  const handleFeatureClick = (feature: SearchResult) => {
    onFeatureClick(feature);
  };

  // Handle record update after edit
  const handleRecordUpdated = useCallback((updatedRecord: DossierRecordSearchResult) => {
    setRecords(prev => prev.map(r => r.id === updatedRecord.id ? updatedRecord : r));
    setEditingRecord(null);
  }, []);

  // Handle newly created record
  const handleRecordCreated = useCallback((newRecord: DossierRecordSearchResult) => {
    setRecords(prev => [newRecord, ...prev]);
    setIsCreatingRecord(false);
  }, []);

  // Handle record deletion
  const handleConfirmDelete = useCallback(async () => {
    if (!deletingRecord || isDeleting) return;
    setIsDeleting(true);
    try {
      await api.delete(`/api/dossier-records/${deletingRecord.id}`);
      setRecords(prev => prev.filter(r => r.id !== deletingRecord.id));
      setDeletingRecord(null);
      if (expandedId === deletingRecord.id) {
        setExpandedId(null);
      }
    } catch (error) {
      console.error('Failed to delete dossier record:', error);
      alert(error instanceof Error ? error.message : 'Не атрымалася выдаліць запіс');
    } finally {
      setIsDeleting(false);
    }
  }, [deletingRecord, isDeleting, expandedId]);

  // Handle successful merge - refresh the records list
  const handleMerged = useCallback(() => {
    setMergingRecord(null);
    skipRef.current = 0;
    searchRecords(query, 0, false);
  }, [query, searchRecords]);

  if (!isOpen) return null;

  return (
    <div className="absolute left-0 top-0 h-full w-96 glass z-30 flex flex-col animate-slide-in-left" style={{ borderRadius: 0, borderLeft: 'none' }}>
      {/* Header */}
      <div className="flex items-center gap-3 p-4 border-b border-black/10">
        <button
          onClick={onClose}
          className="p-1.5 hover:bg-black/5 rounded-lg transition-colors bg-transparent border-none cursor-pointer outline-none"
          title="Згарнуць"
        >
          <ChevronLeft size={20} className="text-black/60" />
        </button>
        <div className="flex items-center gap-2">
          <FileUser size={20} className="text-black/60" />
          <h2 className="text-lg font-semibold m-0">Імёны</h2>
        </div>
        {isAdmin && (
          <button
            onClick={() => setIsCreatingRecord(true)}
            className="ml-auto p-1.5 hover:bg-black/5 rounded-lg transition-colors bg-transparent border-none cursor-pointer outline-none"
            title="Дадаць новае імя"
          >
            <Plus size={20} className="text-black/60" />
          </button>
        )}
      </div>

      {/* Search bar */}
      <div className="p-4 border-b border-black/10">
        <div className="bg-white/10 h-9 flex items-center gap-3 rounded-lg border border-black/10 px-3 box-border">
          <SearchIcon className="text-black/40 w-4 h-4 shrink-0" />
          <input
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Пошук імені..."
            className="bg-transparent border-none outline-none w-full text-sm text-black placeholder:text-black/30"
          />
          {query && (
            <button
              onClick={handleClear}
              className="p-1 hover:bg-black/5 rounded-full transition-colors border-none bg-transparent cursor-pointer outline-none"
            >
              <X size={14} className="text-black/40" />
            </button>
          )}
        </div>
      </div>

      {/* Records list */}
      <div className="flex-1 overflow-y-auto" ref={scrollRef}>
        {records.map((record) => (
          <div key={record.id} className="border-b border-black/5">
            {/* Record row */}
            <div
              onClick={() => handleExpand(record.id)}
              className="p-3 hover:bg-white/50 cursor-pointer transition-colors flex items-center gap-3"
            >
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium text-black truncate">
                  {record.nameBeTarask || record.nameBeNark || record.nameRu || '(без назвы)'}
                </div>
              </div>
              <div
                className="w-2.5 h-2.5 rounded-full shrink-0"
                style={{ backgroundColor: CLASSIFICATION_COLORS[record.classification] || CLASSIFICATION_COLORS[0] }}
              />
              <span className="text-xs text-black/50 bg-black/5 px-2 py-0.5 rounded-full shrink-0">
                {record.numFeatures}
              </span>
              {expandedId === record.id ? (
                <ChevronUp size={16} className="text-black/40 shrink-0" />
              ) : (
                <ChevronDown size={16} className="text-black/40 shrink-0" />
              )}
            </div>

            {expandedId === record.id && (
              <div className="px-4 pb-4 flex flex-col gap-3 bg-black/[0.02]">
                {/* Record details */}
                <DossierRecordItem
                  record={record}
                  namingCategories={namingCategories}
                  isAdmin={isAdmin}
                  onMerge={setMergingRecord}
                  onEdit={setEditingRecord}
                  onDelete={setDeletingRecord}
                />

                {/* Features */}
                {loadingFeatures[record.id] ? (
                  <div className="flex items-center gap-2 text-sm text-black/40 py-2">
                    <Loader2 size={14} className="animate-spin" />
                    <span>Загрузка аб'ектаў...</span>
                  </div>
                ) : features[record.id]?.length ? (
                  <div className="flex flex-col gap-1 pt-2 border-t border-black/10">
                    <div className="text-xs text-black/40 mb-1">Аб'екты:</div>
                    {features[record.id].map((feature) => (
                      <div
                        key={feature.id}
                        onClick={(e) => {
                          e.stopPropagation();
                          handleFeatureClick(feature);
                        }}
                        className="p-2 hover:bg-white/70 cursor-pointer transition-colors rounded-lg flex items-start gap-2 group"
                      >
                        <MapPin size={14} className="text-black/20 group-hover:text-primary transition-colors shrink-0 mt-0.5" />
                        <FeatureListItem feature={feature} truncate />
                      </div>
                    ))}
                  </div>
                ) : features[record.id] && features[record.id].length === 0 ? (
                  <div className="text-sm text-black/40 py-2">Няма аб'ектаў</div>
                ) : null}
              </div>
            )}
          </div>
        ))}

        {/* Loading indicator */}
        {isLoading && (
          <div className="p-4 flex items-center justify-center gap-2 text-sm text-black/40">
            <Loader2 size={16} className="animate-spin" />
            <span>Загрузка...</span>
          </div>
        )}

        {/* Sentinel for infinite scroll */}
        {records.length > 0 && hasMore && !isLoading && <div ref={sentinelRef} className="h-4" />}

        {/* Empty state */}
        {!isLoading && records.length === 0 && (
          <div className="p-4 text-center text-sm text-black/40">
            {query ? 'Нічога ня знойдзена' : 'Пачніце пошук'}
          </div>
        )}
      </div>

      {/* Create Form Modal */}
      {isCreatingRecord && (
        <DossierRecordEditForm
          onClose={() => setIsCreatingRecord(false)}
          onRecordUpdated={handleRecordCreated}
          namingCategories={namingCategories}
        />
      )}

      {/* Edit Form Modal */}
      {editingRecord && (
        <DossierRecordEditForm
          record={editingRecord}
          onClose={() => setEditingRecord(null)}
          onRecordUpdated={handleRecordUpdated}
          namingCategories={namingCategories}
        />
      )}

      {/* Delete Confirmation Dialog */}
      {deletingRecord && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30" style={{ animation: 'fadeIn 150ms ease-out' }}>
          <div className="w-80 glass p-6 flex flex-col gap-4 animate-in zoom-in-95 duration-200">
            <h3 className="text-lg font-bold m-0">Выдаліць запіс?</h3>
            <p className="text-sm text-black/70 m-0">
              Вы ўпэўненыя, што жадаеце выдаліць запіс "{deletingRecord.nameBeTarask || deletingRecord.nameBeNark || deletingRecord.nameRu}"?
            </p>
            <div className="flex gap-2 pt-2">
              <button
                onClick={() => setDeletingRecord(null)}
                disabled={isDeleting}
                className="flex-1 px-4 py-2 text-sm rounded-lg border border-black/20 bg-white/50 hover:bg-white/80 transition-colors cursor-pointer disabled:opacity-50"
              >
                Скасаваць
              </button>
              <button
                onClick={handleConfirmDelete}
                disabled={isDeleting}
                className="flex-1 px-4 py-2 text-sm rounded-lg border-none bg-red-500 text-white hover:bg-red-600 transition-colors cursor-pointer disabled:opacity-50"
              >
                {isDeleting ? 'Выдаленьне...' : 'Выдаліць'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Merge Dialog */}
      {mergingRecord && (
        <MergeDialog
          record={mergingRecord}
          namingCategories={namingCategories}
          onClose={() => setMergingRecord(null)}
          onMerged={handleMerged}
        />
      )}
    </div>
  );
};

export default DossierRecordsPanel;
