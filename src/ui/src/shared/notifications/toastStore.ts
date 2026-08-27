import type { ToastMessageOptions } from "primevue/toast";

export type ToastStatus = "success" | "error" | "info";

export type Toast = {
  id: number;
  status: ToastStatus;
  title: string;
  message: string;
  actionLabel?: string;
  onAction?: () => void;
  durationMs: number;
};

type ToastInput = Omit<Toast, "id" | "durationMs"> & { durationMs?: number };

export type AppToastMessage = ToastMessageOptions & {
  id: number;
  severity: ToastStatus;
  detail: string;
  actionLabel?: string;
  onAction?: () => void;
};

type ToastPublisher = (message: AppToastMessage) => void;

const defaultToastDurationMs = 5000;
const pendingMessages: AppToastMessage[] = [];
let nextToastId = 1;
let publishToast: ToastPublisher | null = null;

export function registerToastPublisher(publisher: ToastPublisher) {
  publishToast = publisher;
  pendingMessages.splice(0).forEach(publisher);

  return () => {
    if (publishToast === publisher) {
      publishToast = null;
    }
  };
}

export function showToast(toast: ToastInput) {
  const message: AppToastMessage = {
    group: "app",
    id: nextToastId++,
    severity: toast.status,
    summary: toast.title,
    detail: toast.message,
    actionLabel: toast.actionLabel,
    onAction: toast.onAction,
    life: toast.durationMs ?? defaultToastDurationMs,
    closable: true,
  };

  if (publishToast === null) {
    pendingMessages.push(message);
    return;
  }

  publishToast(message);
}
