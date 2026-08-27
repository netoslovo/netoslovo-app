import axios from "axios";
import type { ProblemDetails } from "./problemDetails";

export class ApiError extends Error {
  readonly status?: number;
  readonly code?: string;
  readonly problem?: ProblemDetails;

  constructor(
    message: string,
    status?: number,
    code?: string,
    problem?: ProblemDetails,
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.problem = problem;
  }
}

export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) {
    return error;
  }

  if (axios.isAxiosError(error)) {
    if (error.code === "ERR_CANCELED" || axios.isCancel(error)) {
      return new ApiError("Запрос отменен", undefined, "RequestCanceled");
    }

    const problem = readProblemDetails(error.response?.data);
    const status = error.response?.status ?? readProblemStatus(problem);
    const code = readProblemCode(problem);
    const message = error.code === "ECONNABORTED"
      ? "Превышено время ожидания, попробуйте позже"
      : problem?.detail ?? problem?.title ?? error.message;

    return new ApiError(message, status, code, problem);
  }

  if (error instanceof Error) {
    return new ApiError(error.message);
  }

  return new ApiError("Произошла ошибка, попробуйте позже");
}

export function matchesApiError(
  error: unknown,
  status: number,
  code: string,
): boolean {
  const apiError = toApiError(error);
  return apiError.status === status && apiError.code === code;
}

export function isApiRequestCanceled(error: unknown): boolean {
  return toApiError(error).code === "RequestCanceled";
}

function readProblemDetails(data: unknown): ProblemDetails | undefined {
  if (typeof data !== "object" || data === null || Array.isArray(data)) {
    return undefined;
  }

  return data as ProblemDetails;
}

function readProblemStatus(
  problem: ProblemDetails | undefined,
): number | undefined {
  if (typeof problem?.status === "number") {
    return problem.status;
  }

  if (typeof problem?.status === "string") {
    const status = Number(problem.status);
    return Number.isFinite(status) ? status : undefined;
  }

  return undefined;
}

function readProblemCode(
  problem: ProblemDetails | undefined,
): string | undefined {
  const directCode =
    typeof problem?.code === "string" ? problem.code : undefined;

  const extensionCode =
    typeof problem?.extensions?.code === "string"
      ? problem.extensions.code
      : undefined;

  return directCode ?? extensionCode;
}
