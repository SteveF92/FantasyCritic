export default {
    state: {
        advancedProjections: false
    },
    getters: {
        advancedProjections: (state) => state.advancedProjections
    },
    mutations: {
        setAdvancedProjections(state, advancedProjections) {
            state.advancedProjections = advancedProjections;
        }
    }
};
