export type ProblemDetails = {
  type?: string | null;
  title?: string | null;
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
  code?: string;
  extensions?: {
    code?: string;
    [key: string]: unknown;
  };
  [key: string]: unknown;
};
