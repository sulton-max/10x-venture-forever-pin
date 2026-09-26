// Shared content-control props and select field.

import { Field, Select as SdkSelect } from "@wow-two-beta/ui/presentation/forms";

/** Defines props for a typed content control group. */
export interface ContentControlsProps<T> {
  /** The current typed content for this type. */
  readonly value: T;

  /** Emits the next typed content. */
  readonly onChange: (next: T) => void;
}

/** Defines one selectable option. */
export interface SelectOption {
  /** The option's stored value. */
  readonly value: string;

  /** The option's display label. */
  readonly label: string;
}

/** Defines props for the select field. */
interface SelectFieldProps {
  /** The field's label. */
  readonly label: string;

  /** The current value, or undefined to fall back to the first option. */
  readonly value: string | undefined;

  /** The selectable options. */
  readonly options: ReadonlyArray<SelectOption>;

  /** Emits the next value. */
  readonly onChange: (value: string) => void;
}

/** Renders a select field, defaulting to the first option when the value is unset. */
export function SelectField({ label, value, options, onChange }: SelectFieldProps) {
  return (
    <Field label={label}>
      <SdkSelect
        value={value ?? options[0]?.value}
        onValueChange={(o) => o && onChange(o.itemKey)}
        options={options.map((o) => ({ itemKey: o.value, value: o.value, label: o.label }))}
      >
        <SdkSelect.Trigger>
          <SdkSelect.Value />
        </SdkSelect.Trigger>
        <SdkSelect.Content>
          {options.map((o) => (
            <SdkSelect.Item key={o.value} itemKey={o.value} label={o.label} />
          ))}
        </SdkSelect.Content>
      </SdkSelect>
    </Field>
  );
}
