import { mapGetters, mapState } from 'vuex';
import GlobalFunctions from '@/globalFunctions';

let basicMixin = {
  computed: {
    ...mapGetters(['isPlusUser', 'isAuth', 'userInfo', 'isAdmin', 'isBetaTester', 'isFactChecker', 'isActionRunner', 'authIsBusy']),
    ...mapState({
      possibleLeagueOptions: (state) => state.interLeague.possibleLeagueOptions
    }),
    displayName() {
      if (!this.$store.getters.userInfo) {
        return;
      }
      return this.$store.getters.userInfo.displayName;
    }
  },
  data() {
    return {
      toastCount: 0
    };
  },
  methods: {
    makeToast(message) {
      // Use a shorter name for this.$createElement
      const ce = this.$createElement;
      const id = `my-toast-${this.toastCount++}`;
      const vNodesMsg = ce('div', { class: ['toast-flex'] }, [
        ce('span', {}, message),
        ce('font-awesome-icon', {
          class: ['toast-close'],
          props: { icon: 'xmark', size: 'xl' },
          on: {
            click: () => {
              this.$bvToast.hide(id);
            }
          }
        })
      ]);
      // Create the title
      // Pass the VNodes as an array for message and title
      this.$bvToast.toast([vNodesMsg], {
        id: id,
        solid: true,
        variant: 'info',
        noCloseButton: true
      });
    },
    formatLongDateTime(dateTime) {
      return GlobalFunctions.formatLongDateTime(dateTime);
    },
    formatLongDate(date) {
      return GlobalFunctions.formatLongDate(date);
    }
  }
};

export default basicMixin;
