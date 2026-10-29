<template>
  <span v-if="timeRemaining">
    <slot v-bind="{ timeRemaining }"></slot>
  </span>
</template>

<script>
import { DateTime } from 'luxon';

export default {
  props: {
    endTime: { type: String, required: true }
  },
  data() {
    return {
      now: Date.now(),
      timer: null
    };
  },
  computed: {
    end() {
      return DateTime.fromISO(this.endTime);
    },
    timeRemaining() {
      const remaining = this.end.diff(DateTime.fromMillis(this.now), ['days', 'hours', 'minutes', 'seconds']);
      if (remaining.toMillis() <= 0) {
        return null;
      }

      const twoDigits = (value) => Math.floor(value).toString().padStart(2, '0');
      return `${remaining.days} Days, ${twoDigits(remaining.hours)} Hours, ${twoDigits(remaining.minutes)} Minutes, ${twoDigits(remaining.seconds)} Seconds`;
    }
  },
  created() {
    this.tick();
  },
  beforeDestroy() {
    clearTimeout(this.timer);
  },
  methods: {
    tick() {
      this.now = Date.now();
      const millisecondsLeft = this.end.toMillis() - this.now;
      if (millisecondsLeft <= 0) {
        this.$emit('finish');
        return;
      }

      this.timer = setTimeout(this.tick, Math.min(1000, millisecondsLeft));
    }
  }
};
</script>
