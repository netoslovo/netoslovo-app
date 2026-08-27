export function formatLocalDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

export function addDays(day: string, amount: number): string {
  const date = parseLocalDate(day);
  date.setDate(date.getDate() + amount);
  return formatLocalDate(date);
}

export function parseLocalDate(day: string): Date {
  const [year, month, date] = day.split("-").map(Number);
  return new Date(year, month - 1, date);
}

const fullDateFormatter = new Intl.DateTimeFormat("ru-RU", {
  day: "numeric",
  month: "long",
  year: "numeric",
});

const shortDateFormatter = new Intl.DateTimeFormat("ru-RU", {
  day: "numeric",
  month: "short",
  year: "numeric",
});

const weekdayFormatter = new Intl.DateTimeFormat("ru-RU", {
  weekday: "short",
});

export function formatAdminDate(day: string, full = false): string {
  return (full ? fullDateFormatter : shortDateFormatter).format(
    parseLocalDate(day),
  );
}

export function formatWeekday(day: string): string {
  return weekdayFormatter.format(parseLocalDate(day));
}

export function countDays(from: string, to: string): number {
  const milliseconds = parseLocalDate(to).getTime() - parseLocalDate(from).getTime();
  return Math.floor(milliseconds / 86_400_000) + 1;
}
