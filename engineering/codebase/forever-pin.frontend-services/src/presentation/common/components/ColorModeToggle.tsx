import { Moon, Sun } from "lucide-react";

import { useColorMode } from "@wow-two-beta/ui/foundation/primitives";
import { ColorTone } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant } from "@wow-two-beta/ui/presentation/actions";

/** Renders the light and dark mode switch. */
export function ColorModeToggle() {
  const { mode, toggle } = useColorMode();
  return (
    <Button
      variant={ButtonVariant.Ghost}
      tone={ColorTone.Neutral}
      shape="square"
      aria-label={mode === "dark" ? "Switch to light mode" : "Switch to dark mode"}
      onClick={toggle}
    >
      {mode === "dark" ? <Sun size={18} /> : <Moon size={18} />}
    </Button>
  );
}
