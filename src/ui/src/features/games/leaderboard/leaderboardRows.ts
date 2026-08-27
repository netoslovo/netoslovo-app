export type LeaderboardPlayerRow = {
  id: string;
  kind: "player";
  place: number | null;
  playerName: string;
  metric: string | number;
  current: boolean;
  placeEmphasized: boolean;
  playerNameEmphasized: boolean;
  emptyPlaceInfo?: string;
};

export type LeaderboardGapRow = {
  id: string;
  kind: "gap";
};

export type LeaderboardRow = LeaderboardPlayerRow | LeaderboardGapRow;

export function createLeaderboardRows(
  topRows: LeaderboardPlayerRow[],
  currentPlayerRow: LeaderboardPlayerRow,
  topN: number,
): LeaderboardRow[] {
  if (topRows.some((row) => row.current)) return topRows;

  const rows: LeaderboardRow[] = [...topRows];
  if (topRows.length > 0 && currentPlayerRow.place !== null) {
    rows.push({ id: "gap-before-current", kind: "gap" });
  }

  rows.push(currentPlayerRow);

  if (currentPlayerRow.place !== null && currentPlayerRow.place > topN) {
    rows.push({ id: "gap-after-current", kind: "gap" });
  }

  return rows;
}
