import { ApiError, parseJson } from "@wow-two-beta/ui/foundation/http";
import type { ApiResponse, ProblemDetails } from "@wow-two-beta/ui/foundation/http";

// Re-export the shared error type so consumers import it from the integration surface, not the SDK directly.
export { ApiError };

// Empty = same-origin; serves the SPA in prod. Override via VITE_API_BASE for split deployment.
export const API_BASE: string = import.meta.env.VITE_API_BASE ?? "";

export let REDIRECT_BASE: string = import.meta.env.VITE_REDIRECT_BASE ?? "http://localhost:7022";

// Public; also the backend's token audience. Empty leaves sign-in inert.
export let GOOGLE_CLIENT_ID: string = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? "";

// Load before rendering so Google sign-in and generated links use this environment.
export async function loadRuntimeConfig(): Promise<void> {
  const response = await fetch(`${API_BASE}/api/runtime-config`, { cache: "no-store" });
  if (!response.ok) throw new Error("Runtime configuration unavailable");
  const config = await response.json();
  if (typeof config.googleClientId !== "string" || typeof config.redirectBaseUrl !== "string")
    throw new Error("Invalid runtime configuration");
  GOOGLE_CLIENT_ID = config.googleClientId;
  REDIRECT_BASE = config.redirectBaseUrl;
}

// Parse Temporal values and unwrap the success envelope.
export async function readData<T>(res: Response): Promise<T> {
  const envelope = parseJson<ApiResponse<T>>(await res.text());
  return envelope.data;
}

// Prefer validation messages, then problem details, then the HTTP status.
function problemMessage(problem: ProblemDetails | null, fallback: string, status: number): string {
  const errs = problem?.errors as Array<{ message?: string }> | Record<string, string[]> | undefined;
  if (Array.isArray(errs)) {
    const msgs = errs.map((e) => e?.message).filter((m): m is string => Boolean(m));
    if (msgs.length) return msgs.join(" ");
  } else if (errs && typeof errs === "object") {
    const msgs = Object.values(errs).flat().filter((m): m is string => Boolean(m));
    if (msgs.length) return msgs.join(" ");
  }
  return problem?.detail ?? problem?.title ?? `${fallback} (HTTP ${status})`;
}

// Build an API error with status, problem details, and a display message.
export async function problemError(res: Response, fallback: string): Promise<ApiError> {
  let problem: ProblemDetails | null = null;
  try {
    problem = parseJson<ProblemDetails>(await res.text());
  } catch {
    problem = null;
  }
  return new ApiError(res.status, problem, problemMessage(problem, fallback, res.status));
}
