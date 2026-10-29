<template>
  <label class="toggle-switch" :class="{ disabled }">
    <input type="checkbox" class="toggle-switch-input" :checked="value" :disabled="disabled" @change.stop="toggle" />
    <span class="toggle-switch-core" :style="coreStyle">
      <span class="toggle-switch-button" :style="buttonStyle"></span>
    </span>
    <span class="toggle-switch-label" :class="value ? 'checked' : 'unchecked'" :style="labelStyle">
      {{ value ? labels.checked : labels.unchecked }}
    </span>
  </label>
</template>

<script>
const buttonMargin = 3;

export default {
  props: {
    value: { type: Boolean, required: true },
    labels: { type: Object, required: true },
    width: { type: Number, default: 60 },
    height: { type: Number, default: 28 },
    disabled: { type: Boolean, default: false }
  },
  computed: {
    coreStyle() {
      return {
        width: `${this.width}px`,
        height: `${this.height}px`,
        borderRadius: `${Math.round(this.height / 2)}px`
      };
    },
    buttonStyle() {
      const size = this.height - buttonMargin * 2;
      const left = this.value ? this.width - this.height + buttonMargin : buttonMargin;
      return {
        width: `${size}px`,
        height: `${size}px`,
        transform: `translate3d(${left}px, ${buttonMargin}px, 0)`
      };
    },
    labelStyle() {
      return {
        lineHeight: `${this.height}px`
      };
    }
  },
  methods: {
    toggle() {
      const toggled = !this.value;
      this.$emit('input', toggled);
      this.$emit('change', toggled);
    }
  }
};
</script>

<style scoped>
.toggle-switch {
  display: inline-block;
  position: relative;
  vertical-align: middle;
  user-select: none;
  cursor: pointer;
}

.toggle-switch.disabled {
  pointer-events: none;
  opacity: 0.6;
}

.toggle-switch-input {
  opacity: 0;
  position: absolute;
  width: 1px;
  height: 1px;
}

.toggle-switch-core {
  display: block;
  position: relative;
  box-sizing: border-box;
  margin: 0;
  background-color: #414141;
}

.toggle-switch-button {
  display: block;
  position: absolute;
  overflow: hidden;
  top: 0;
  left: 0;
  border-radius: 100%;
  background-color: white;
  transition: transform 300ms;
  z-index: 2;
}

.toggle-switch-label {
  position: absolute;
  top: 0;
  font-size: 13px;
  font-weight: 600;
  color: white;
  z-index: 1;
}

.toggle-switch-label.checked {
  left: 10px;
}

.toggle-switch-label.unchecked {
  right: 10px;
}
</style>
