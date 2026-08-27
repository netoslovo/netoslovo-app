import { definePreset } from "@primeuix/themes";
import Aura from "@primeuix/themes/aura";

export const primeVueTheme = definePreset(Aura, {
  semantic: {
    primary: {
      50: "#f0f9f8",
      100: "#c2e8e2",
      200: "#8ad3c8",
      300: "#48b7a6",
      400: "#41a495",
      500: "#378a7d",
      600: "#2e746a",
      700: "#255d55",
      800: "#1f4f48",
      900: "#173934",
      950: "#102a26",
    },
    colorScheme: {
      light: {
        surface: {
          0: "#ffffff",
          50: "#f9fafa",
          100: "#f1f1f2",
          200: "#e6e7e9",
          300: "#d2d4d7",
          400: "#a9adb2",
          500: "#797f88",
          600: "#4d5560",
          700: "#2e3744",
          800: "#19202b",
          900: "#141a23",
          950: "#0e1218",
        },
        primary: {
          color: "{primary.500}",
          contrastColor: "{surface.0}",
          hoverColor: "{primary.600}",
          activeColor: "{primary.700}",
        },
        formField: {
          placeholderColor: "{surface.600}",
          floatLabelColor: "{surface.600}",
          floatLabelActiveColor: "{surface.600}",
        },
        text: {
          mutedColor: "{surface.600}",
          hoverMutedColor: "{surface.700}",
        },
      },
    },
  },
});
