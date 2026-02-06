interface TextFieldProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  maxLength: number;
  multiline?: boolean;
  error?: string;
  required?: boolean;
}

/**
 * Reusable text input field component for forms.
 */
export const TextField = ({
  label,
  value,
  onChange,
  maxLength,
  multiline = false,
  error,
  required,
}: TextFieldProps) => (
  <div className="flex flex-col gap-1">
    <label className="text-xs text-black/50">
      {label}
      {required && <span className="text-red-500 ml-1">*</span>}
    </label>
    {multiline ? (
      <textarea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        maxLength={maxLength}
        rows={3}
        className={`text-sm p-2 border rounded-lg bg-white/50 outline-none focus:border-primary resize-none ${error ? 'border-red-400' : 'border-black/20'}`}
      />
    ) : (
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        maxLength={maxLength}
        className={`text-sm p-2 border rounded-lg bg-white/50 outline-none focus:border-primary ${error ? 'border-red-400' : 'border-black/20'}`}
      />
    )}
    {error && <span className="text-xs text-red-500">{error}</span>}
  </div>
);

interface SelectFieldProps {
  label: string;
  value: number | null;
  onChange: (value: number | null) => void;
  options: { value: number; label: string }[];
  required?: boolean;
}

/**
 * Reusable select dropdown field component for forms.
 */
export const SelectField = ({
  label,
  value,
  onChange,
  options,
  required,
}: SelectFieldProps) => (
  <div className="flex flex-col gap-1">
    <label className="text-xs text-black/50">
      {label}
      {required && <span className="text-red-500 ml-1">*</span>}
    </label>
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
