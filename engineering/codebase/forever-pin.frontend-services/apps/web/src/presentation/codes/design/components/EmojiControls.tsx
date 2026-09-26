import { useCallback, useMemo } from "react";

import { EmojiCatalog, type EmojiCatalogEntry } from "@wow-two-beta/ui/domain/emoji";
import { localStorageStorageBroker, type StorageBroker } from "@wow-two-beta/ui/foundation/storage";
import {
  type CategoryNavVariant,
  DefaultEmojiSize,
  EmojiPicker,
  type EmojiPickerSizeInput,
  EmojiSizeControl,
  type EmojiTileShape,
} from "@wow-two-beta/ui/presentation/forms";
import { Stack } from "@wow-two-beta/ui/presentation/layout";

import { type CodeEmojiDto } from "@/domain/codes/style";

/** Defines props for the center-emoji picker. */
export interface EmojiControlsProps {
  /** The current center emoji, or `null` for none. */
  readonly emoji: CodeEmojiDto | null;

  /** Emits the next center emoji, or `null` to clear it. */
  readonly onChange: (emoji: CodeEmojiDto | null) => void;

  /** The category-navigation affordance. Default `strip`. */
  readonly categoryNavVariant?: CategoryNavVariant;

  /** The element scale — one value for every element, or a per-element `{ search, nav, tile }`. Default `md`. */
  readonly size?: EmojiPickerSizeInput;

  /** The emoji-tile frame — rounded chip or circle. Default `rounded`. */
  readonly tileShape?: EmojiTileShape;

  /** The scrollable tile viewport's height, in tile rows. Default `6`. */
  readonly rowsCount?: number;

  /** The heading rendered above the picker. Default `Center emoji`. */
  readonly label?: string;

  /** Open on the first category when there are no recents yet. Default `true`. */
  readonly showFirstCategoryWhenRecentsEmpty?: boolean;

  /** The recent-emoji storage broker; defaults to local storage. */
  readonly recentsStorageBroker?: StorageBroker;
}

/** @internal The largest preview glyph, in px, that still fits an `OptionTile` without clipping its frame. */
const MaxPreviewGlyph = 24;

/** Renders the center-emoji picker and size control. */
export function EmojiControls({
  emoji,
  onChange,
  categoryNavVariant,
  size,
  tileShape,
  rowsCount,
  label = "Center emoji",
  showFirstCategoryWhenRecentsEmpty = true,
  recentsStorageBroker = localStorageStorageBroker,
}: EmojiControlsProps) {
  const selectedEntry = useMemo<EmojiCatalogEntry | null>(
    () => (emoji === null ? null : EmojiCatalog.all.find((entry) => entry.glyph === emoji.glyph) ?? null),
    [emoji],
  );

  const pickEmoji = useCallback(
    (entry: EmojiCatalogEntry | null) => {
      if (entry === null) {
        onChange(null);
        return;
      }
      onChange({ glyph: entry.glyph, sizeRatio: emoji?.sizeRatio ?? DefaultEmojiSize });
    },
    [emoji?.sizeRatio, onChange],
  );

  return (
    <Stack gap="3">
      <EmojiPicker
        value={selectedEntry}
        onChange={pickEmoji}
        storage={recentsStorageBroker}
        label={label}
        showFirstCategoryWhenRecentsEmpty={showFirstCategoryWhenRecentsEmpty}
        categoryNavVariant={categoryNavVariant}
        size={size}
        tileShape={tileShape}
        rowsCount={rowsCount}
      />

      {emoji === null ? null : (
        <EmojiSizeControl
          glyph={emoji.glyph}
          sizeRatio={emoji.sizeRatio}
          maxPreviewGlyph={MaxPreviewGlyph}
          onChange={(ratio) => onChange({ ...emoji, sizeRatio: ratio })}
        />
      )}
    </Stack>
  );
}
