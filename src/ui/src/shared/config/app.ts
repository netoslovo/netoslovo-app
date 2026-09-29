export const APP_HOSTNAME = "нетослово.рф";

const encodedAppHostname = new URL(`https://${APP_HOSTNAME}`).hostname;

type AppLocation = Pick<Location, "hostname" | "origin">;

/** Returns the current origin with the canonical hostname in its user-facing form. */
export function getAppOrigin(location: AppLocation = window.location): string {
  if (location.hostname !== encodedAppHostname) return location.origin;

  return location.origin.replace(location.hostname, APP_HOSTNAME);
}
