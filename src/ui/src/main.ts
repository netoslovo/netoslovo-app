import "@fontsource/golos-text/400.css";
import "@fontsource/golos-text/500.css";
import "@fontsource/golos-text/600.css";
import "@fontsource/golos-text/700.css";
import PrimeVue from "primevue/config";
import type { DialogPassThroughOptions } from "primevue/dialog";
import ToastService from "primevue/toastservice";
import "primeicons/primeicons.css";
import { createApp } from "vue";
import App from "./App.vue";
import router from "./app/router";
import { primeVueTheme } from "./app/theme/primeVueTheme";
import "./shared/styles/tokens.css";
import "./shared/styles/base.css";
import "./shared/styles/primevue-overrides.css";

function focusDialogRootAfterEnter(element: Element) {
  if (!(element instanceof HTMLElement)) {
    return;
  }

  const explicitFocusTarget = element.querySelector<HTMLElement>(
    '[autofocus]:not([data-pc-group-section="headericon"])',
  );

  if (explicitFocusTarget !== null) {
    explicitFocusTarget.focus({ preventScroll: true });
    return;
  }

  element.focus({ preventScroll: true });
}

const dialogPassThrough: DialogPassThroughOptions = {
  root: { tabindex: -1 },
  transition: { onAfterEnter: focusDialogRootAfterEnter },
};

const app = createApp(App);

app.use(PrimeVue, {
  pt: {
    dialog: dialogPassThrough,
  },
  theme: {
    preset: primeVueTheme,
    options: {
      darkModeSelector: false,
    },
  },
});
app.use(ToastService);
app.use(router);

app.mount("#app");
