export const userNoticeCodes = {
  gameGuide: "game-guide",
  defaultUserName: "default-user-name",
  newArcadeGameWhileActive: "new-arcade-game-while-active",
  guestLoginAfterFinishedGame: "guest-login-after-finished-game",
  screenKeyboardRestoreHint: "screen-keyboard-restore-hint",
} as const;

export type UserNoticeCode = typeof userNoticeCodes[keyof typeof userNoticeCodes];
