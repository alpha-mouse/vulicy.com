import { Layers } from 'lucide-react';
import { CLASSIFICATION_COLORS, getClassificationText } from '../constants/mapConstants';

const Legend = () => {
  return (
    <div className="absolute left-4 bottom-10 glass p-4 rounded-xl z-10 w-64 space-y-2">
      <div className="flex items-center gap-2">
        <Layers size={16} className="text-black/60" />
        <span className="text-xs font-bold uppercase tracking-widest text-black/60">Легенда</span>
      </div>
      {Object.entries(CLASSIFICATION_COLORS).map(([lvl, color]) => (
        lvl !== '0' && (
          <div key={lvl} className="flex items-center gap-3">
            <div className="w-3 h-3 rounded-full shrink-0" style={{ backgroundColor: color }} />
            <span className="text-xs font-medium text-black/80">{getClassificationText(lvl)}</span>
          </div>
        )
      ))}
    </div>
  );
};

export default Legend;
