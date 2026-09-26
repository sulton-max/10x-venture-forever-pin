import { Field, UrlInput } from "@wow-two-beta/ui/presentation/forms";
import type { UrlContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** Renders the destination URL field. */
export function UrlControls({ value, onChange }: ContentControlsProps<UrlContent>) {
  return (
    <Field label="Destination URL">
      <UrlInput
        ring="sm"
        value={value.url}
        placeholder="https://example.com"
        onChange={(e) => onChange({ ...value, url: e.target.value })}
      />
    </Field>
  );
}
