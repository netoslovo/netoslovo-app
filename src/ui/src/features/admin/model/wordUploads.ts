export type WordsVersionUploadState =
  | "Active"
  | "Completed"
  | "Canceled"
  | "Timeout"
  | "Error";

export type WordsVersionUploadStepName =
  | "CreateNewWordsVersion"
  | "InsertWordsIndexes"
  | "InsertWordDistanceMaps"
  | "DownloadNewGameSources"
  | "UploadNewGameSourcesVersion"
  | "ActivateNewWordsVersion"
  | "WaitingForPostActivationTasks"
  | "UpdateSingleGames"
  | "UpdateSingleGameResults"
  | "InvalidateWordsCache"
  | "UnloadOldWordsVersions"
  | "Cancel"
  | "Timeout";

export type WordsVersionUploadCompletedStep = {
  name: WordsVersionUploadStepName;
  isSuccess: boolean;
  completedAt: string;
};

export type WordsVersionUpload = {
  sagaId: string;
  wordsVersion: number;
  state: WordsVersionUploadState;
  currentStep: WordsVersionUploadStepName;
  createdAt: string;
  completedAt: string | null;
  completedSteps: WordsVersionUploadCompletedStep[];
  version: number;
};

export type WordsVersionUploadPage = {
  uploads: WordsVersionUpload[];
  hasMore: boolean;
};

export type WordsVersionUploadHistorySortField =
  | "CreatedAt"
  | "CompletedAt"
  | "WordsVersion"
  | "State";

export type WordsVersionUploadSortDirection = "Asc" | "Desc";
