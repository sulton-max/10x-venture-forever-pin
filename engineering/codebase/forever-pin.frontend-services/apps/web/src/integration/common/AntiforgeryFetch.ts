/** Holds the readable cookie the API issues on every read, and the header a cookie-carrying write echoes it in. */
const TokenCookie = 'XSRF-TOKEN';
const TokenHeader = 'X-XSRF-TOKEN';
const UnsafeMethods = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);

/** Reads the current antiforgery token, or `null` when the API has not issued one yet. */
function readToken(): string | null {
  const match = document.cookie.match(new RegExp(`(?:^|;\\s*)${TokenCookie}=([^;]*)`));
  return match?.[1] ? decodeURIComponent(match[1]) : null;
}

/** Reports whether a response is the API's antiforgery rejection; reads a clone so the body stays readable. */
async function isAntiforgeryRejection(response: Response): Promise<boolean> {
  if (response.status !== 400) return false;
  try {
    return (await response.clone().text()).includes('AntiforgeryValidationFailed');
  } catch {
    return false;
  }
}

/**
 * Wraps `fetch` so cookie-carrying writes echo the antiforgery token.
 * A token issued before sign-in or sign-out is refreshed once through a read, then the write retries.
 * Product bridge until `@wow-two-beta/ui-vue`'s client ships the same behavior as an option.
 */
export function createAntiforgeryFetch(
  refreshPath: string,
  base: typeof fetch = (...args) => fetch(...args),
): typeof fetch {
  return async (input, init) => {
    const method = (init?.method ?? (input instanceof Request ? input.method : 'GET')).toUpperCase();
    if (!UnsafeMethods.has(method)) return base(input, init);

    const send = () => {
      const headers = new Headers(init?.headers);
      const token = readToken();
      if (token) headers.set(TokenHeader, token);
      return base(input, { ...init, headers });
    };

    const response = await send();
    if (!(await isAntiforgeryRejection(response))) return response;
    await base(refreshPath, { cache: 'no-store', credentials: init?.credentials, signal: init?.signal });
    return send();
  };
}
