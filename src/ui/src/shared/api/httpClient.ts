import axios, {
  type AxiosRequestConfig,
  type AxiosError,
  type GenericAbortSignal,
  type InternalAxiosRequestConfig,
} from "axios";
import { toApiError } from "./apiError";

const defaultRequestPolicy: ApiRequestPolicy = {
  timeoutMs: 15_000,
  retries: 0,
};
const defaultRetryDelayMs = 400;
const maxRetryDelayMs = 10_000;

type RetryRequestConfig = InternalAxiosRequestConfig & {
  retryCount?: number;
  requestPolicy?: ApiRequestPolicy;
  actorStampRequestSequence?: number;
};

type ApiAxiosRequestConfig = AxiosRequestConfig & {
  requestPolicy: ApiRequestPolicy;
};

type UnauthorizedResponseHandler = () => void;
export type ActorStampChange = {
  previous: string;
  current: string;
};

type ActorStampChangeHandler = (change: ActorStampChange) => void;

let unauthorizedResponseHandler: UnauthorizedResponseHandler | null = null;
let actorStampChangeHandler: ActorStampChangeHandler | null = null;
let currentActorStamp: string | null = null;
let nextActorStampRequestSequence = 0;
let latestActorStampResponseSequence = 0;

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "",
  timeout: defaultRequestPolicy.timeoutMs,
  headers: {
    "Content-Type": "application/json"
  },
  withCredentials: true
});

export type ApiRequestOptions = {
  signal?: AbortSignal;
};

export type ApiRequestPolicy = Readonly<{
  timeoutMs: number;
  retries: number;
}>;

export function withRequestPolicy(
  policy: ApiRequestPolicy,
  options?: ApiRequestOptions,
): ApiAxiosRequestConfig {
  return {
    signal: options?.signal,
    timeout: policy.timeoutMs,
    requestPolicy: policy,
  };
}

export function setUnauthorizedResponseHandler(
  handler: UnauthorizedResponseHandler,
) {
  unauthorizedResponseHandler = handler;
}

export function setActorStampChangeHandler(
  handler: ActorStampChangeHandler,
) {
  actorStampChangeHandler = handler;
}

apiClient.interceptors.request.use((config: RetryRequestConfig) => {
  config.actorStampRequestSequence = ++nextActorStampRequestSequence;
  return config;
});

apiClient.interceptors.response.use(
  (response) => {
    updateActorStamp(
      response.headers["x-actor-stamp"],
      response.config as RetryRequestConfig,
    );
    return response;
  },
  async (error: unknown) => {
    if (shouldRetry(error)) {
      const axiosError = error as AxiosError;
      const config = axiosError.config as RetryRequestConfig;
      config.retryCount = (config.retryCount ?? 0) + 1;
      await waitForRetry(
        getRetryDelay(axiosError, config.retryCount),
        config.signal,
      );
      return apiClient.request(config);
    }

    if (axios.isAxiosError(error)) {
      updateActorStamp(
        error.response?.headers["x-actor-stamp"],
        error.config as RetryRequestConfig | undefined,
      );
    }

    const apiError = toApiError(error);
    if (apiError.status === 401) {
      unauthorizedResponseHandler?.();
    }

    return Promise.reject(apiError);
  },
);

function updateActorStamp(
  value: unknown,
  config: RetryRequestConfig | undefined,
) {
  if (typeof value !== "string" || value.length === 0) {
    return;
  }

  const responseSequence = config?.actorStampRequestSequence;
  if (
    responseSequence === undefined ||
    responseSequence < latestActorStampResponseSequence
  ) {
    return;
  }

  latestActorStampResponseSequence = responseSequence;
  const previous = currentActorStamp;
  currentActorStamp = value;
  if (previous !== null && previous !== value) {
    actorStampChangeHandler?.({ previous, current: value });
  }
}

function shouldRetry(error: unknown) {
  if (!axios.isAxiosError(error) || error.code === "ERR_CANCELED") {
    return false;
  }

  const config = error.config as RetryRequestConfig | undefined;
  if (
    !config ||
    (config.retryCount ?? 0) >=
      (config.requestPolicy ?? defaultRequestPolicy).retries
  ) {
    return false;
  }

  const status = error.response?.status;
  return status === undefined || status === 408 || status === 429 || status >= 500;
}

function getRetryDelay(
  error: AxiosError,
  attempt: number,
) {
  const retryAfterMs = parseRetryAfter(error.response?.headers["retry-after"]);
  if (retryAfterMs !== null) {
    return Math.min(retryAfterMs, maxRetryDelayMs);
  }

  const exponentialDelayMs = defaultRetryDelayMs * 2 ** (attempt - 1);
  const jitterMs = Math.random() * defaultRetryDelayMs;
  return Math.min(exponentialDelayMs + jitterMs, maxRetryDelayMs);
}

function parseRetryAfter(value: unknown): number | null {
  if (typeof value !== "string") {
    return null;
  }

  const seconds = Number(value);
  if (Number.isFinite(seconds) && seconds >= 0) {
    return seconds * 1_000;
  }

  const date = Date.parse(value);
  return Number.isNaN(date) ? null : Math.max(0, date - Date.now());
}

function waitForRetry(delayMs: number, signal?: GenericAbortSignal) {
  return new Promise<void>((resolve, reject) => {
    if (signal?.aborted) {
      reject(new axios.CanceledError());
      return;
    }

    const timeoutId = window.setTimeout(() => {
      signal?.removeEventListener?.("abort", handleAbort);
      resolve();
    }, delayMs);
    const handleAbort = () => {
      window.clearTimeout(timeoutId);
      reject(new axios.CanceledError());
    };
    signal?.addEventListener?.("abort", handleAbort, { once: true });
  });
}
