import type { DailyGame, GameState } from "../model/game";

export type DailyGameStatus = GameState | "notStarted" | "disabled";

export type DailyGameStatusConfig = {
  icon: string;
  label: string;
  action: string;
  status: DailyGameStatus;
};

const statusConfigs: Record<GameState | "notStarted", DailyGameStatusConfig> = {
  notStarted: {
    icon: "pi pi-play-circle",
    label: "Не начато",
    action: "Начать",
    status: "notStarted",
  },
  active: {
    icon: "pi pi-spinner-dotted",
    label: "В процессе",
    action: "Продолжить",
    status: "active",
  },
  guessed: {
    icon: "pi pi-check-circle",
    label: "Отгадано",
    action: "Открыть",
    status: "guessed",
  },
  surrendered: {
    icon: "pi pi-times-circle",
    label: "Вы сдались",
    action: "Открыть",
    status: "surrendered",
  },
  cancelled: {
    icon: "pi pi-ban",
    label: "Игра отменена",
    action: "Открыть",
    status: "cancelled",
  },
};

export function getDailyGameStatus(dailyGame: DailyGame): DailyGameStatusConfig {
  const config = statusConfigs[dailyGame.state ?? "notStarted"];

  return !dailyGame.isToday && dailyGame.guessedAtGameDay === true && config.status === "guessed"
    ? { ...config, label: "Отгадано в день выхода" }
    : config;
}

export function getDisabledDailyGameStatus(label: string): DailyGameStatusConfig {
  return {
    icon: "pi pi-ban",
    label,
    action: "Недоступно",
    status: "disabled",
  };
}

export function formatDailyGameDate(day: string) {
  return new Intl.DateTimeFormat("ru-RU", {
    day: "numeric",
    month: "short",
  }).format(new Date(`${day}T00:00:00`));
}

export function wasGuessedOnReleaseDay(dailyGame: DailyGame) {
  return dailyGame.state === "guessed" && dailyGame.guessedAtGameDay === true;
}
