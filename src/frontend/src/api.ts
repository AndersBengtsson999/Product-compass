export interface WorkspaceOverview {
  organizationId: string;
  organizationName: string;
  workspaceId: string;
  workspaceName: string;
}
export async function loadWorkspace(
  organizationId: string,
  token: string,
): Promise<WorkspaceOverview> {
  const response = await fetch(
    `/api/organizations/${encodeURIComponent(organizationId)}/workspace`,
    {
      headers: { Authorization: `Bearer ${token}` },
      signal: AbortSignal.timeout(10000),
    },
  );
  if (!response.ok) {
    if (response.status === 401)
      throw new Error(
        "Your access token is invalid or expired. Create a new token and reconnect.",
      );
    if (response.status === 403)
      throw new Error(
        "This account does not have access to that organization.",
      );
    if (response.status === 400)
      throw new Error("Enter a valid organization ID.");
    throw new Error(
      "The workspace is unavailable. Check that the API and database are running.",
    );
  }
  return response.json() as Promise<WorkspaceOverview>;
}
