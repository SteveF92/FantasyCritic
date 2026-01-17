
import { shallowMount, createLocalVue } from '@vue/test-utils';
import PossibleMasterGamesTable from '@/components/possibleMasterGamesTable.vue';
import Vuex from 'vuex';

let store;

const localVue = createLocalVue();
localVue.use(Vuex);

describe('getBidEligibility method',()=>{
    // Helper to create a store with a given pickup system and bid times
    function createStore(pickupSystem, bidTimes) {
        return new Vuex.Store({
            state: {
                leagueYear: {
                    options: {
                        pickupSystem: pickupSystem
                    }
                }
            },
            getters: {
                bidTimes: () => bidTimes
            }
        });
    }

    describe('SecretBidding', () => {
        beforeEach(() => {
            store = createStore('SecretBidding', {
                nextBidLockTime: '2026-01-10T20:00:00-05:00',
                nextPublicBiddingTime: null
            });
        });

        it('returns tooLate if the game releases before the next bid processing', () => {
            // Game releases: 2026-01-09 (before next round)
            const wrapper = shallowMount(PossibleMasterGamesTable, { store, localVue, propsData: { possibleGames: [] } });
            const masterGame = { releaseDate: '2026-01-09T12:00:00-05:00' };
            const result = wrapper.vm.getBidEligibility(masterGame);
            expect(result).toBe('tooLate');
        });

        it('returns lastChance if the game releases between the next and following bid processing', () => {
            // nextBidLockTime: 2026-01-10T20:00:00-05:00, following: +7 days = 2026-01-17T20:00:00-05:00
            // Game releases: 2026-01-12 (between next and following)
            const wrapper = shallowMount(PossibleMasterGamesTable, { store, localVue, propsData: { possibleGames: [] } });
            const masterGame = { releaseDate: '2026-01-12T12:00:00-05:00' };
            const result = wrapper.vm.getBidEligibility(masterGame);
            expect(result).toBe('lastChance');
        });

        it('returns empty string if the game releases at or after the following bid processing', () => {
            // nextBidLockTime: 2026-01-10T20:00:00-05:00, following: +7 days = 2026-01-17T20:00:00-05:00
            // Game releases: 2026-01-17T20:00:00-05:00 (at following)
            const wrapper = shallowMount(PossibleMasterGamesTable, { store, localVue, propsData: { possibleGames: [] } });
            const masterGame = { releaseDate: '2026-01-17T20:00:00-05:00' };
            const result = wrapper.vm.getBidEligibility(masterGame);
            expect(result).toBe('');
        });
    });

    describe('SemiPublicBidding', () => {
        beforeEach(() => {
            store = createStore('SemiPublicBidding', {
                nextBidLockTime: null,
                nextPublicBiddingTime: '2026-01-10T20:00:00-05:00'
            });
        });

        it('returns tooLate if the game releases before the next public bid processing', () => {
            // Game releases: 2026-01-09 (before next round)
            const wrapper = shallowMount(PossibleMasterGamesTable, { store, localVue, propsData: { possibleGames: [] } });
            const masterGame = { releaseDate: '2026-01-09T12:00:00-05:00' };
            const result = wrapper.vm.getBidEligibility(masterGame);
            expect(result).toBe('tooLate');
        });

        it('returns lastChance if the game releases between the next and following public bid processing', () => {
            // nextPublicBiddingTime: 2026-01-10T20:00:00-05:00, following: +14 days = 2026-01-24T20:00:00-05:00
            // Game releases: 2026-01-15 (between next and following)
            const wrapper = shallowMount(PossibleMasterGamesTable, { store, localVue, propsData: { possibleGames: [] } });
            const masterGame = { releaseDate: '2026-01-15T12:00:00-05:00' };
            const result = wrapper.vm.getBidEligibility(masterGame);
            expect(result).toBe('lastChance');
        });

        it('returns an empty string if the game releases at or after the following public bid processing', () => {
            // nextPublicBiddingTime: 2026-01-10T20:00:00-05:00, following: +14 days = 2026-01-24T20:00:00-05:00
            // Game releases: 2026-01-24T20:00:00-05:00 (at following)
            const wrapper = shallowMount(PossibleMasterGamesTable, { store, localVue, propsData: { possibleGames: [] } });
            const masterGame = { releaseDate: '2026-01-24T20:00:00-05:00' };
            const result = wrapper.vm.getBidEligibility(masterGame);
            expect(result).toBe('');
        });
    });
    describe('SemiPublicBiddingSecretCounterPicks', () => {
        beforeEach(() => {
            store = createStore('SemiPublicBiddingSecretCounterPicks', {
                nextBidLockTime: null,
                nextPublicBiddingTime: '2026-01-10T20:00:00-05:00'
            });
        });

        it('returns false if the game releases before the next public bid processing', () => {
            // Game releases: 2026-01-09 (before next round)
            const wrapper = shallowMount(PossibleMasterGamesTable, { store, localVue, propsData: { possibleGames: [] } });
            const masterGame = { releaseDate: '2026-01-09T12:00:00-05:00' };
            const result = wrapper.vm.getBidEligibility(masterGame);
            expect(result).toBe('tooLate');
        });

        it('returns lastChance if the game releases between the next and following public bid processing', () => {
            // nextPublicBiddingTime: 2026-01-10T20:00:00-05:00, following: +14 days = 2026-01-24T20:00:00-05:00
            // Game releases: 2026-01-15 (between next and following)
            const wrapper = shallowMount(PossibleMasterGamesTable, { store, localVue, propsData: { possibleGames: [] } });
            const masterGame = { releaseDate: '2026-01-15T12:00:00-05:00' };
            const result = wrapper.vm.getBidEligibility(masterGame);
            expect(result).toBe('lastChance');
        });

        it('returns empty string if the game releases at or after the following public bid processing', () => {
            // nextPublicBiddingTime: 2026-01-10T20:00:00-05:00, following: +14 days = 2026-01-24T20:00:00-05:00
            // Game releases: 2026-01-24T20:00:00-05:00 (at following)
            const wrapper = shallowMount(PossibleMasterGamesTable, { store, localVue, propsData: { possibleGames: [] } });
            const masterGame = { releaseDate: '2026-01-24T20:00:00-05:00' };
            const result = wrapper.vm.getBidEligibility(masterGame);
            expect(result).toBe('');
        });
    });

});


