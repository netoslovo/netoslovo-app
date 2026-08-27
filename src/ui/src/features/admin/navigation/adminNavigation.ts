import type { AuthSession } from "../../auth/model/auth";
import { adminPermissions, hasPermission } from "../../auth/model/permissions";

export type AdminNavigationDestination = {
  name: string;
  path: string;
  label: string;
  icon: string;
  permission: string;
};

export const adminDestinations = {
  dailyOverview: {
    name: "admin-daily-overview",
    path: "daily",
    label: "Обзор",
    icon: "pi pi-chart-bar",
    permission: adminPermissions.viewSources,
  },
  dailyWords: {
    name: "admin-daily-words",
    path: "daily/words",
    label: "Слова",
    icon: "pi pi-list",
    permission: adminPermissions.viewSources,
  },
  dailyModeration: {
    name: "admin-daily-moderation",
    path: "daily/moderation",
    label: "Модерация",
    icon: "pi pi-check-circle",
    permission: adminPermissions.setSourceReview,
  },
  dailySchedule: {
    name: "admin-daily-schedule",
    path: "daily/schedule",
    label: "Расписание",
    icon: "pi pi-calendar",
    permission: adminPermissions.viewSources,
  },
  dictionaryUploads: {
    name: "admin-dictionary-uploads",
    path: "dictionary/uploads",
    label: "Загрузка слов",
    icon: "pi pi-cloud-upload",
    permission: adminPermissions.viewWordsVersionUpload,
  },
  usersNameFilter: {
    name: "admin-users-name-filter",
    path: "users/name-filter",
    label: "Фильтр UserName",
    icon: "pi pi-filter",
    permission: adminPermissions.manageUserNameFilter,
  },
} as const satisfies Record<string, AdminNavigationDestination>;

const adminNavigationSections = [
  {
    id: "daily",
    title: "Слова дня",
    icon: "pi pi-calendar-clock",
    description: "Обзор, модерация и расписание слов дня.",
    destinations: [
      adminDestinations.dailyOverview,
      adminDestinations.dailyWords,
      adminDestinations.dailyModeration,
      adminDestinations.dailySchedule,
    ],
  },
  {
    id: "dictionary",
    title: "Словарь",
    icon: "pi pi-book",
    description: "Загрузка и состояние версий словаря.",
    destinations: [adminDestinations.dictionaryUploads],
  },
  {
    id: "users",
    title: "Пользователи",
    icon: "pi pi-users",
    description: "Настройки проверки пользовательских имён.",
    destinations: [adminDestinations.usersNameFilter],
  },
] as const;

export type AdminNavigationSection = Omit<
  (typeof adminNavigationSections)[number],
  "destinations"
> & {
  destinations: AdminNavigationDestination[];
};

export function getPermittedAdminSections(
  auth: AuthSession | null,
): AdminNavigationSection[] {
  return adminNavigationSections.flatMap((section) => {
    const destinations = section.destinations.filter((destination) =>
      hasPermission(auth, destination.permission),
    );

    return destinations.length > 0 ? [{ ...section, destinations }] : [];
  });
}
