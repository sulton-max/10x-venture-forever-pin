import { afterEach, describe, expect, it, vi } from "vitest";

import { apiFetch } from "@/integration/common";

// The API issues a readable XSRF-TOKEN cookie on every GET; cookie-carrying writes echo it in a header.
function stubBrowser(cookie: string, responses: Response[]) {
  const document = { cookie };
  const fetch = vi.fn(async (_url: string, _init?: RequestInit) => responses.shift() ?? new Response("{}"));
  vi.stubGlobal("document", document);
  vi.stubGlobal("fetch", fetch);
  return { document, fetch };
}

function headerOf(init: RequestInit | undefined, name: string): string | null {
  return new Headers(init?.headers).get(name);
}

describe("apiFetch", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("echoes the decoded token on writes and never on reads", async () => {
    const { fetch } = stubBrowser("theme=dark; XSRF-TOKEN=abc%2Bdef%3D", []);

    await apiFetch("/api/codes", { method: "POST", body: "{}" });
    await apiFetch("/api/codes");

    expect(headerOf(fetch.mock.calls[0][1], "X-XSRF-TOKEN")).toBe("abc+def=");
    expect(fetch.mock.calls[0][1]?.credentials).toBe("include");
    expect(headerOf(fetch.mock.calls[1][1], "X-XSRF-TOKEN")).toBeNull();
  });

  it("refreshes a stale token once through a GET and retries the write", async () => {
    const { document, fetch } = stubBrowser("XSRF-TOKEN=before-sign-in", []);
    fetch
      .mockImplementationOnce(async () =>
        new Response(JSON.stringify({ code: "AntiforgeryValidationFailed" }), { status: 400 }))
      .mockImplementationOnce(async () => {
        document.cookie = "XSRF-TOKEN=after-sign-in";
        return new Response("{}");
      });

    const response = await apiFetch("/api/codes", { method: "PUT", body: "{}" });

    expect(response.status).toBe(200);
    expect(fetch).toHaveBeenCalledTimes(3);
    expect(fetch.mock.calls[1][0]).toBe("/api/runtime-config");
    expect(headerOf(fetch.mock.calls[2][1], "X-XSRF-TOKEN")).toBe("after-sign-in");
  });

  it("returns other failures unchanged and readable", async () => {
    const problem = new Response(JSON.stringify({ title: "Name is required." }), { status: 400 });
    const { fetch } = stubBrowser("XSRF-TOKEN=token", [problem]);

    const response = await apiFetch("/api/codes", { method: "POST", body: "{}" });

    expect(fetch).toHaveBeenCalledTimes(1);
    expect(await response.json()).toEqual({ title: "Name is required." });
  });
});
