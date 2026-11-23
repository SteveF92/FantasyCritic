import { beforeEach, describe, expect, it } from 'vitest';
import { defineComponent, h } from 'vue';
import { createPinia, setActivePinia } from 'pinia';
import { mount } from '@vue/test-utils';
import type { BasicDataViewModel, LeagueTagOptionsViewModel, MasterGameTagViewModel } from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import LeagueTagSelector from '@/components/leagueTagSelector.vue';

function tag(name: string, readableName: string, systemTagOnly = false) {
  return { name, readableName, shortName: name, systemTagOnly, badgeColor: '000000', description: '', examples: [] } as unknown as MasterGameTagViewModel;
}

const ListStub = defineComponent({
  setup(_, { slots }) {
    return () => h('div', slots.default?.());
  }
});

function mountSelector(banned: string[]) {
  const modelValue: LeagueTagOptionsViewModel = { banned, required: [] };
  return mount(LeagueTagSelector, {
    props: { modelValue },
    global: { stubs: { VueDraggable: ListStub, MasterGameTagBadge: true, FontAwesomeIcon: true } }
  });
}

function buttonTexts(wrapper: ReturnType<typeof mountSelector>) {
  return wrapper.findAll('button').map((button) => button.text());
}

describe('leagueTagSelector', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    useInterLeagueStore().basicData = {
      masterGameTags: [tag('Port', 'Port'), tag('FreeToPlay', 'Free to Play'), tag('Remaster', 'Remaster'), tag('UnannouncedGame', 'Unannounced Game', true)]
    } as unknown as BasicDataViewModel;
  });

  it('gives each tag a button naming it, and leaves out system tags', () => {
    const wrapper = mountSelector(['Port']);
    expect(buttonTexts(wrapper)).toEqual(['Reset Changes', 'Allow Port', 'Ban Free to Play', 'Ban Remaster']);
  });

  it('bans a tag onto the end of the banned list', async () => {
    const wrapper = mountSelector(['Remaster', 'Port']);
    await wrapper.findAll('button').find((button) => button.text() === 'Ban Free to Play')!.trigger('click');
    expect(wrapper.emitted('update:modelValue')).toEqual([[{ banned: ['Remaster', 'Port', 'FreeToPlay'], required: [] }]]);
  });

  it('allows a banned tag', async () => {
    const wrapper = mountSelector(['Remaster', 'Port']);
    await wrapper.findAll('button').find((button) => button.text() === 'Allow Remaster')!.trigger('click');
    expect(wrapper.emitted('update:modelValue')).toEqual([[{ banned: ['Port'], required: [] }]]);
  });
});
