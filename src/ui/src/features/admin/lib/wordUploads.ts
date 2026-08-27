import type {
  WordsVersionUploadState,
  WordsVersionUploadStepName,
} from "../model/wordUploads";

export const uploadStepLabels: Record<WordsVersionUploadStepName, string> = {
  CreateNewWordsVersion: "Создание версии слов",
  InsertWordsIndexes: "Загрузка индексов слов",
  InsertWordDistanceMaps: "Загрузка карт расстояний",
  DownloadNewGameSources: "Получение источников игр",
  UploadNewGameSourcesVersion: "Загрузка версии игровых слов",
  ActivateNewWordsVersion: "Активация версии слов",
  WaitingForPostActivationTasks: "Ожидание задач после активации",
  UpdateSingleGames: "Обновление игр",
  UpdateSingleGameResults: "Обновление результатов",
  InvalidateWordsCache: "Сброс кеша слов",
  UnloadOldWordsVersions: "Выгрузка старых версий",
  Cancel: "Отмена",
  Timeout: "Таймаут",
};

export const uploadStateLabels: Record<WordsVersionUploadState, string> = {
  Active: "В процессе",
  Completed: "Завершено",
  Canceled: "Отменено",
  Timeout: "Таймаут",
  Error: "Ошибка",
};

export function formatUploadDateTime(value: string | null) {
  if (!value) return "не завершена";
  return new Date(value).toLocaleString("ru-RU");
}

export function shortUploadSagaId(sagaId: string) {
  return `${sagaId.slice(0, 8)}...${sagaId.slice(-6)}`;
}
