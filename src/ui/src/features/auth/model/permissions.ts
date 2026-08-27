import type { AuthSession } from "./auth";

export const adminRoles = {
  admin: "Admin",
} as const;

export const adminPermissions = {
  viewSources: "Game.ViewSources",
  setSourceReview: "Game.Daily.SetSourceReview",
  assignSource: "Game.Daily.AssignSource",
  unassignSource: "Game.Daily.UnassignSource",
  viewWordsVersionUpload: "Sagas.ViewWordsVersionUpload",
  uploadWordsVersion: "Sagas.UploadWordsVersion",
  cancelWordsVersionUpload: "Sagas.CancelWordsVersionUpload",
  manageUserNameFilter: "Auth.ManageUserNameFilter",
} as const;

export function hasPermission(
  auth: AuthSession | null,
  permission: string,
): boolean {
  return auth?.permissions.includes(permission) === true;
}

export function hasRole(auth: AuthSession | null, role: string): boolean {
  return auth?.roles.includes(role) === true;
}
