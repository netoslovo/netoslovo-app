import type { PopoverPassThroughOptions } from "primevue/popover";
import { ref, useId } from "vue";

type PopoverRef = {
  show: (event: Event, target: HTMLElement) => void;
  hide: () => void;
};

export function useInfoPopoverSemantics() {
  const id = useId();
  const triggerId = `${id}-trigger`;
  const panelId = `${id}-panel`;
  const popoverPt: PopoverPassThroughOptions = {
    root: {
      id: panelId,
      role: "tooltip",
      "aria-modal": undefined,
    },
  };

  return { triggerId, panelId, popoverPt };
}

export function useInfoPopover() {
  const semantics = useInfoPopoverSemantics();
  const popover = ref<PopoverRef | null>(null);
  const visible = ref(false);

  function setPopover(value: unknown) {
    popover.value = value as PopoverRef | null;
  }

  function show(event: Event) {
    if (event.currentTarget instanceof HTMLElement) {
      popover.value?.show(event, event.currentTarget);
    }
  }

  function hide() {
    popover.value?.hide();
  }

  function onPointerDown(event: PointerEvent) {
    event.preventDefault();
    if (event.pointerType === "mouse") return;

    event.stopPropagation();
    if (visible.value) hide();
    else show(event);
  }

  function onShow() {
    visible.value = true;
  }

  function onHide() {
    visible.value = false;
  }

  return {
    ...semantics,
    setPopover,
    visible,
    show,
    hide,
    onPointerDown,
    onShow,
    onHide,
  };
}
