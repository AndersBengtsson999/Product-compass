import { afterEach, describe, expect, it, vi } from "vitest";
import { loadWorkspace } from "./api";
afterEach(() => vi.unstubAllGlobals());
describe("workspace connection", () => {
  it.each([
    [401, "invalid or expired"],
    [403, "does not have access"],
    [400, "valid organization ID"],
    [500, "unavailable"],
  ])("explains API error %s", async (status, message) => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false, status }));
    await expect(loadWorkspace("org", "token")).rejects.toThrow(
      String(message),
    );
  });
  it("passes authorization only in the request header", async () => {
    const workspace = {
      organizationId: "org",
      organizationName: "Acme",
      workspaceId: "id",
      workspaceName: "Product",
    };
    const fetchMock = vi
      .fn()
      .mockResolvedValue({ ok: true, json: async () => workspace });
    vi.stubGlobal("fetch", fetchMock);
    expect(await loadWorkspace("org", "secret-token")).toEqual(workspace);
    expect(fetchMock).toHaveBeenCalledWith(
      "/api/organizations/org/workspace",
      expect.objectContaining({
        headers: { Authorization: "Bearer secret-token" },
      }),
    );
  });
});
