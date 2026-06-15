<template>
  <div>
    <h3>What is Critics Royale?</h3>
    <div class="text-well">
      <p>
        Critics Royale is an alternate way to play Fantasy Critic - no league required. Every quarter, you can create a publisher that will compete against the entire site. Instead of drafting, you're
        given a 'budget' which you will use to buy games that you believe will score well during that quarter. Your goal is to spend that money wisely and put together the best lineup of games that
        you can.
      </p>
    </div>

    <div v-if="is2026Q3Plus">
      <h3>Changes for Quarter 3 of 2026!</h3>
      <div class="text-well">
        <p>
          We are continuing to refine the rule set for Critic's Royale. Here's a list of this quarter's changes.
          <ul>
            <li>Ad budget is back to giving a 5% boost per dollar, for a total possible boost of 50%.</li>
            <li>The rules for selling a game have been reverted to be closer to what they were pre-2026. Now, selling a game gets you back 50% of what you spent (plus any advertising budget assigned). If a game has been delayed out of the quarter, you instead get 75%, and if a game has been deemed ineligible in Royale, you get a full refund.</li>
            <li>You now cannot buy a game that was added to the site until a normal bidding cycle has passed.</li>
            <li>Game prices used to be calculated as (Projected Points × 1.5), but now that 1.5 multiplier has been removed. This has the effect of making the most expensive games less costly.</li>
            <li>The minimum price of a game is now $3, up from $2.</li>
            <li>The "lockout window" where you cannot buy/sell/change advertising budget on a game has been raised from 5 days to 7 days.</li>
            <li>There is now a 10 minute "regret window" where you can sell a game back for exactly what you bought it for.</li>
          </ul>
        </p>
        <p>Note that these rule changes <em>do not</em> affect Q2 of 2026. Those rules are locked in.</p>
      </div>
    </div>

    <div v-if="is2026Q1Q2">
      <h3>Changes for 2026!</h3>
      <div class="text-well">
        <p>
          There have been several changes for the 2026 seasons. All of the information here is also elsewhere on this page, but here's a short summary of what's changed this year:
          <ul>
            <li>Maximum roster size is now 15 games, down from 25.</li>
            <li>The following tags are now banned, in addition to previously banned tags: Remake, Partial Remake, and Planned for Early Access.</li>
            <li>The 5 day lockout for buying games now also applies to selling games and changing advertising budget.</li>
            <li>When you sell a game, you get back 50% of its current market value, instead of 50% of what you bought it for.</li>
            <li>If a game won't release in the quarter, you get back 75% of market value when you sell.</li>
            <li>Marketing budget bonus is now 10% per dollar, up from 5% per dollar.</li>
            <li>There is a one week grace period after a quarter ends where scores can still change.</li>
          </ul>
        </p>
      </div>
    </div>

    <div v-if="isPre2026">
      <h3>How's it work?</h3>
      <div class="text-well">
        <p>
          You'll be given a $100 "budget" with which to "buy" games to add them to your roster. Each game's price is set based upon how popular it is on the site. When the game releases, you get
          points using the same system as the regular Fantasy Critic leagues. You can also boost your points by setting an "advertising budget" for a game before it comes out. More on that below.
        </p>
        <p>
          If you lose confidence in a game, you can choose to "sell" it, and get back half the money you spent on it. You can't sell a game that has come out, or one that has reviews already.
        </p>
      </div>
    </div>
    <div v-if="is2026Q1Q2">
      <h3>How's it work?</h3>
      <div class="text-well">
        <p>
          You'll be given a $100 "budget" with which to "buy" games to add them to your roster. There is a roster limit of 15 games per quarter. Each game's price is set based upon how popular it
          is on the site. When the game releases, you get points using the same system as the regular Fantasy Critic leagues. You can also boost your points by setting an "advertising budget" for a
          game before it comes out. More on that below.
        </p>
        <p>
          If you lose confidence in a game, you can choose to "sell" it, and get back half of the current sell price of the game. If a game is confirmed to not release in the quarter, you get back
          75% of the current value. You can't sell a game that has come out, or one that has reviews already.
        </p>
      </div>
    </div>
    <div v-if="is2026Q3Plus">
      <h3>How's it work?</h3>
      <div class="text-well">
        <p>
          You'll be given a $100 "budget" with which to "buy" games to add them to your roster. There is a roster limit of 15 games per quarter. Each game's price is set based upon how popular it
          is on the site. When the game releases, you get points using the same system as the regular Fantasy Critic leagues. You can also boost your points by setting an "advertising budget" for a
          game before it comes out. More on that below.
        </p>
        <p>
          If you lose confidence in a game, you can choose to "sell" it, and get back half of what you paid for it (plus any advertising budget you assigned). If a game is confirmed to not release
          in the quarter, you get back 75% of what you paid. If a game is deemed ineligible in Royale, you get a full refund. You can't sell a game that has come out, or one that has reviews already.
        </p>
        <p>
          There is also a 10 minute "regret window" after purchasing a game during which you can sell it back for exactly what you paid.
        </p>
      </div>
    </div>

    <div v-if="hasSecretRoster">
      <h3>Secret Roster Rule</h3>
      <div class="text-well">
        <p>Your games are hidden from other players until one of the following happens:</p>
        <ul>
          <li>The game gets a score from OpenCritic.</li>
          <li>The game releases.</li>
          <li>The game is delayed out of the quarter.</li>
          <li>The quarter ends.</li>
        </ul>
        <p>In other words, games can't be seen by other players until that game is not available for purchase anymore. The idea of this feature is to force every player to do their own research.</p>
      </div>
    </div>

    <div v-if="!is2026Q1Q2">
      <h3>What's an "advertising budget"?</h3>
      <div class="text-well">
        <p>
          You can choose to assign some of your budget (the same one you use to buy games) to boost the score you get for a game. Every
          <strong>$1</strong>
          assigned to a game will increase its points received by
          <strong>5%</strong>.
        </p>
        <p>
          For example, a game that receives a critic score of
          <strong>80</strong>
          usually gets you
          <strong>10</strong>
          points. But, with an advertising budget of
          <strong>$5</strong>,
          it will be boosted by
          <strong>25%</strong>,
          giving you
          <strong>12.5</strong>
          points. You don't need to spend even dollars to get a bonus, every cent counts.
        </p>
        <p>
          You can assign up to
          <strong>$10</strong>
          in budget to any single game. If a game has already been released or already has review scores, you can't change its advertising budget anymore.
        </p>
      </div>
    </div>
    <div v-else>
      <h3>What's an "advertising budget"?</h3>
      <div class="text-well">
        <p>
          You can choose to assign some of your budget (the same one you use to buy games) to boost the score you get for a game. Every
          <strong>$1</strong>
          assigned to a game will increase its points received by
          <strong>10%</strong>.
        </p>
        <p>
          For example, a game that receives a critic score of
          <strong>80</strong>
          usually gets you
          <strong>10</strong>
          points. But, with an advertising budget of
          <strong>$5</strong>,
          it will be boosted by
          <strong>50%</strong>,
          giving you
          <strong>15</strong>
          points. You don't need to spend even dollars to get a bonus, every cent counts.
        </p>
        <p>
          You can assign up to
          <strong>$10</strong>
          in budget to any single game. If a game has already been released or already has review scores, you can't change its advertising budget anymore.
        </p>
      </div>
    </div>

    <div v-if="is2026Q3Plus">
      <h3>What games are eligible?</h3>
      <div class="text-well">
        <label>The following tags are banned:</label>
        <p>
          <masterGameTagBadge tag-name="Remake"></masterGameTagBadge>
          <masterGameTagBadge tag-name="PartialRemake"></masterGameTagBadge>
          <masterGameTagBadge tag-name="DirectorsCut"></masterGameTagBadge>
          <masterGameTagBadge tag-name="Remaster"></masterGameTagBadge>
          <masterGameTagBadge tag-name="YearlyInstallment"></masterGameTagBadge>
          <masterGameTagBadge tag-name="PlannedForEarlyAccess"></masterGameTagBadge>
          <masterGameTagBadge tag-name="CurrentlyInEarlyAccess"></masterGameTagBadge>
          <masterGameTagBadge tag-name="ReleasedInternationally"></masterGameTagBadge>
          <masterGameTagBadge tag-name="Port"></masterGameTagBadge>
        </p>
        <p>
          You cannot purchase, sell, or change advertising budget on a game that will release in 7 days or less. This is to prevent people from taking games that are about to come out that may have
          reviews starting to come in. In the case of "Shadow Drops", if you purchased the game the day before the game was revealed, you can keep it, but if you purchase it the same day as the
          reveal + release, then it will be marked as ineligible once the release date is set correctly in the system.
        </p>
        <p>Additionally, you cannot buy a game that was recently added to the site until a normal bidding cycle has passed.</p>
      </div>
    </div>
    <div v-if="is2026Q1Q2">
      <h3>What games are eligible?</h3>
      <div class="text-well">
        <label>The following tags are banned:</label>
        <p>
          <masterGameTagBadge tag-name="Remake"></masterGameTagBadge>
          <masterGameTagBadge tag-name="PartialRemake"></masterGameTagBadge>
          <masterGameTagBadge tag-name="DirectorsCut"></masterGameTagBadge>
          <masterGameTagBadge tag-name="Remaster"></masterGameTagBadge>
          <masterGameTagBadge tag-name="YearlyInstallment"></masterGameTagBadge>
          <masterGameTagBadge tag-name="PlannedForEarlyAccess"></masterGameTagBadge>
          <masterGameTagBadge tag-name="CurrentlyInEarlyAccess"></masterGameTagBadge>
          <masterGameTagBadge tag-name="ReleasedInternationally"></masterGameTagBadge>
          <masterGameTagBadge tag-name="Port"></masterGameTagBadge>
        </p>
        <p>
          You cannot purchase, sell, or change advertising budget on a game that will release in 5 days or less. This is to prevent people from taking games that are about to come out that may have
          reviews starting to come in. In the case of "Shadow Drops", if you purchased the game the day before the game was revealed, you can keep it, but if you purchase it the same day as the
          reveal + release, then it will be marked as ineligible once the release date is set correctly in the system.
        </p>
      </div>
    </div>
    <div v-if="isPre2026">
      <h3>What games are eligible?</h3>
      <div class="text-well">
        <label>The following tags are banned:</label>
        <p>
          <masterGameTagBadge tag-name="DirectorsCut"></masterGameTagBadge>
          <masterGameTagBadge tag-name="Remaster"></masterGameTagBadge>
          <masterGameTagBadge tag-name="YearlyInstallment"></masterGameTagBadge>
          <masterGameTagBadge tag-name="CurrentlyInEarlyAccess"></masterGameTagBadge>
          <masterGameTagBadge tag-name="ReleasedInternationally"></masterGameTagBadge>
          <masterGameTagBadge tag-name="Port"></masterGameTagBadge>
        </p>
        <p>
          Additionally, you cannot purchase a game that will release in 5 days or less. This is to prevent people from taking games that are about to come out that may have reviews starting to come
          in. In the case of "Shadow Drops", if you purchased the game the day before the game was revealed, you can keep it, but if you purchase it the same day as the reveal + release, then it
          will be marked as ineligible once the release date is set correctly in the system.
        </p>
      </div>
    </div>
  </div>
</template>

<script>
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

export default {
  components: {
    MasterGameTagBadge
  },
  props: {
    year: { type: Number, default: null },
    quarter: { type: Number, default: null }
  },
  computed: {
    // Secret Roster Rule was introduced in Q4 2022
    hasSecretRoster() {
      return this.year !== null && (this.year > 2022 || (this.year === 2022 && this.quarter !== null && this.quarter >= 4));
    },
    isPre2026() {
      return this.year !== null && this.year < 2026;
    },
    is2026Q1Q2() {
      return this.year === 2026 && this.quarter !== null && this.quarter <= 2;
    },
    is2026Q3Plus() {
      return this.year !== null && (this.year > 2026 || (this.year === 2026 && this.quarter !== null && this.quarter >= 3));
    }
  }
};
</script>
