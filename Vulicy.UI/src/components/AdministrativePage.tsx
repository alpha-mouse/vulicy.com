import { useState, useEffect, useCallback } from 'react';
import { ArrowLeft, ChevronRight, Download } from 'lucide-react';
import TopBar from './TopBar';
import { api } from '../utils/api';
import { ADMINISTRATIVE_PREFIX, type Administrative } from '../types/administrative';
import './AdministrativePage.css';

interface TreeNodeProps {
  node: Administrative;
  level: number;
  expandedIds: Set<number>;
  onToggle: (id: number) => void;
  onExport: (id: number) => void;
}

const TreeNode = ({ node, level, expandedIds, onToggle, onExport }: TreeNodeProps) => {
  const hasChildren = node.childAdministratives && node.childAdministratives.length > 0;
  const isExpanded = expandedIds.has(node.id);
  const prefix = ADMINISTRATIVE_PREFIX[node.type] ?? '';

  return (
    <div className="tree-node">
      <div
        className={`tree-node-row tree-node-level-${level}${hasChildren ? ' expandable' : ''}`}
        onClick={hasChildren ? () => onToggle(node.id) : undefined}
      >
        {hasChildren ? (
          <span className={`tree-chevron${isExpanded ? ' expanded' : ''}`}>
            <ChevronRight size={14} />
          </span>
        ) : (
          <span className="tree-chevron-placeholder" />
        )}
        {prefix && <span className="tree-prefix">{prefix}</span>}
        <span className="tree-name">{node.nameBeTarask}</span>
        <button
          className="tree-export-btn"
          title="Экспартаваць"
          onClick={e => { e.stopPropagation(); onExport(node.id); }}
        >
          <Download size={12} />
          Экспартаваць
        </button>
      </div>
      {hasChildren && isExpanded && (
        <div className="tree-children">
          {node.childAdministratives!.map(child => (
            <TreeNode
              key={child.id}
              node={child}
              level={level + 1}
              expandedIds={expandedIds}
              onToggle={onToggle}
              onExport={onExport}
            />
          ))}
        </div>
      )}
    </div>
  );
};

/** Collect all IDs in the tree that have children */
function collectExpandableIds(nodes: Administrative[]): number[] {
  const ids: number[] = [];
  for (const node of nodes) {
    if (node.childAdministratives && node.childAdministratives.length > 0) {
      ids.push(node.id);
      ids.push(...collectExpandableIds(node.childAdministratives));
    }
  }
  return ids;
}

const BackButton = () => (
  <a
    href="/"
    className="p-2 hover:bg-black/5 rounded-lg transition-colors bg-transparent border-none cursor-pointer outline-none inline-flex"
    title="Вярнуцца да мапы"
  >
    <ArrowLeft size={20} className="text-black/60" />
  </a>
);

const AdministrativePage = () => {
  const [data, setData] = useState<Administrative[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedIds, setExpandedIds] = useState<Set<number>>(new Set());

  useEffect(() => {
    const fetchData = async () => {
      try {
        const result = await api.get<Administrative[]>('/api/administratives');
        setData(result);
      } catch (err) {
        setError('Не ўдалося загрузіць дадзеныя');
        console.error('Failed to fetch administratives:', err);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const handleToggle = useCallback((id: number) => {
    setExpandedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }, []);

  const expandAll = useCallback(() => {
    setExpandedIds(new Set(collectExpandableIds(data)));
  }, [data]);

  const collapseAll = useCallback(() => {
    setExpandedIds(new Set());
  }, []);

  const handleExport = useCallback((id: number) => {
    window.location.href = `/api/features/export/by-administrative/${id}`;
  }, []);

  const handleExportAll = useCallback(() => {
    window.location.href = '/api/features/export';
  }, []);

  return (
    <div className="admin-page">
      <TopBar leftContent={<BackButton />} />

      <div className="admin-content">
        {loading ? (
          <div className="admin-state">
            <div className="admin-state-text">Загрузка...</div>
          </div>
        ) : error ? (
          <div className="admin-state">
            <div className="admin-state-text">{error}</div>
          </div>
        ) : (
          <>
            <div className="admin-header">
              <div className="admin-header-left">
                <h1 className="admin-title">Адміністрацыйны падзел</h1>
                <button className="tree-export-btn" onClick={handleExportAll}>
                  <Download size={13} />
                  Экспартаваць усё
                </button>
              </div>
              <div className="admin-controls">
                <button onClick={expandAll}>Разгарнуць усе</button>
                <button onClick={collapseAll}>Згарнуць усе</button>
              </div>
            </div>
            <div className="admin-tree">
              {data.map(node => (
                <TreeNode
                  key={node.id}
                  node={node}
                  level={0}
                  expandedIds={expandedIds}
                  onToggle={handleToggle}
                  onExport={handleExport}
                />
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default AdministrativePage;
