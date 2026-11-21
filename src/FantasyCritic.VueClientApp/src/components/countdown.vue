<template>
  <span v-if="timeRemaining">
    <slot v-bind="{ timeRemaining }"></slot>
  </span>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { DateTime } from 'luxon';

interface CountdownData {
  now: number;
  timer: number | undefined;
}

export default defineComponent({
  props: {
    endTime: { type: String, required: true }
  },
  emits: { finish: () => true },
  data(): CountdownData {
    return {
      now: Date.now(),
      timer: undefined
    };
  },
  computed: {
    end(): DateTime {
      return DateTime.fromISO(this.endTime);
    },
    timeRemaining(): string {
      const remaining = this.end.diff(DateTime.fromMillis(this.now));
      return remaining.toMillis() > 0 ? remaining.toFormat("d 'Days', hh 'Hours', mm 'Minutes', ss 'Seconds'") : '';
    }
  },
  created() {
    this.tick();
  },
  beforeUnmount() {
    clearTimeout(this.timer);
  },
  methods: {
    tick(): void {
      this.now = Date.now();
      const millisecondsLeft = this.end.toMillis() - this.now;
      if (millisecondsLeft <= 0) {
        this.$emit('finish');
        return;
      }

      this.timer = setTimeout(this.tick, Math.min(1000, millisecondsLeft));
    }
  }
});
</script>
