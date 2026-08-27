<script setup lang="ts">
import Checkbox from "primevue/checkbox";
import Dialog from "primevue/dialog";
import { ref, watch } from "vue";
import UiButton from "../../../shared/ui/UiButton.vue";

const props = defineProps<{
  visible: boolean;
  saving?: boolean;
}>();

const emit = defineEmits<{
  dismiss: [doNotShowAgain: boolean];
  cancel: [doNotShowAgain: boolean];
  confirm: [doNotShowAgain: boolean];
}>();

const doNotShowAgain = ref(false);

watch(
  () => props.visible,
  (visible) => {
    if (visible) {
      doNotShowAgain.value = false;
    }
  },
);

function onVisibleUpdate(visible: boolean) {
  if (!visible && !props.saving) {
    emit("dismiss", doNotShowAgain.value);
  }
}
</script>

<template>
  <Dialog :visible="visible" modal :dismissable-mask="!saving" :closable="!saving" :close-on-escape="!saving"
    header="Начать новую игру?" class="game-dialog" @update:visible="onVisibleUpdate">
    <div class="new-arcade-game-dialog">
      <p class="new-arcade-game-dialog__text">
        У вас уже есть активная игра. Вы уверены, что хотите начать новую?
      </p>

      <div class="new-arcade-game-dialog__preference">
        <Checkbox input-id="new-arcade-game-do-not-show" v-model="doNotShowAgain" binary :disabled="saving" />
        <label for="new-arcade-game-do-not-show">Больше не показывать</label>
      </div>

      <div class="new-arcade-game-dialog__actions">
        <UiButton variant="outlined" :disabled="saving" @click="emit('cancel', doNotShowAgain)">
          Назад
        </UiButton>
        <UiButton :loading="saving" @click="emit('confirm', doNotShowAgain)">
          Начать
        </UiButton>
      </div>
    </div>
  </Dialog>
</template>

<style scoped>
.new-arcade-game-dialog {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.new-arcade-game-dialog__text {
  margin: 0;
  color: var(--color-gray-600);
  font-size: var(--game-dialog-body-font-size);
  line-height: 1.45;
}

.new-arcade-game-dialog__preference {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--color-gray-700);
  font-size: 14px;
}

.new-arcade-game-dialog__preference label {
  cursor: pointer;
}

.new-arcade-game-dialog__actions {
  display: flex;
  gap: 10px;
}

.new-arcade-game-dialog__actions>.ui-button {
  flex: 1;
}
</style>
