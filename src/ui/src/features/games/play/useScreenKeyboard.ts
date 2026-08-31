import { computed, ref, type Ref } from "vue";

const preferenceKey = "netoslovo:screenKeyboardEnabled:v1";

function getInitialPreference() {
  if (typeof window === "undefined") return false;

  const storedPreference = localStorage.getItem(preferenceKey);
  return storedPreference === null
    ? window.matchMedia("(max-width: 1024px)").matches
    : storedPreference === "true";
}

export function useScreenKeyboard(word: Ref<string>, isLoading: () => boolean) {
  const initialPreference = getInitialPreference();
  const preferred = ref(initialPreference);
  const visible = ref(initialPreference);
  const infoOpen = ref(false);
  const toggleDescription = computed(() => preferred.value ? "Выключить" : "Включить");

  function append(character: string) {
    if (!isLoading() && word.value.length < 25) word.value += character;
  }

  function removeLast() {
    if (!isLoading() && word.value.length > 0) word.value = word.value.slice(0, -1);
  }

  function show() {
    if (preferred.value) visible.value = true;
  }

  function hide() {
    visible.value = false;
  }

  function close() {
    preferred.value = false;
    visible.value = false;
    localStorage.setItem(preferenceKey, "false");
  }

  function toggle(closeMenu?: () => void) {
    closeMenu?.();
    preferred.value = !preferred.value;
    visible.value = preferred.value;
    localStorage.setItem(preferenceKey, String(preferred.value));
  }

  return { preferred, visible, infoOpen, toggleDescription, append, removeLast, show, hide, close, toggle };
}
