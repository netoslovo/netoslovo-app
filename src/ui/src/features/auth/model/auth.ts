export type AuthSession = {
  isAuthenticated: boolean;
  id: string;
  userName: string | null;
  email: string | null;
  roles: readonly string[];
  permissions: readonly string[];
};

export type Profile = {
  email: string;
  userName: string;
  userNameChangedAt: string | null;
  createdAt: string;
};
