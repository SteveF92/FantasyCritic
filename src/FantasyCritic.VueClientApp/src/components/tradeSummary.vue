<template>
  <collapseCard :default-visible="defaultVisible">
    <template #header>{{ header }}</template>
    <template #body>
      <div class="row">
        <div class="col-6">
          <h4>{{ trade.proposerPublisherName }}</h4>
          <h5>Player: {{ trade.proposerDisplayName }}</h5>
          <h5>{{ recievesVerbiage }}</h5>
          <div v-for="game in trade.counterPartySendGames" :key="game.masterGameYear.masterGameID" class="component-row">
            •
            <masterGamePopover :master-game="game.masterGameYear"></masterGamePopover>
            <template v-if="game.counterPick">(Counter Pick)</template>
          </div>
          <div v-if="trade.counterPartyBudgetSendAmount" class="component-row">• ${{ trade.counterPartyBudgetSendAmount }} of budget</div>
        </div>
        <div class="col-6">
          <h4>{{ trade.counterPartyPublisherName }}</h4>
          <h5>Player: {{ trade.counterPartyDisplayName }}</h5>
          <h5>{{ recievesVerbiage }}</h5>
          <div v-for="game in trade.proposerSendGames" :key="game.masterGameYear.masterGameID" class="component-row">
            •
            <masterGamePopover :master-game="game.masterGameYear"></masterGamePopover>
            <template v-if="game.counterPick">(Counter Pick)</template>
          </div>
          <div v-if="trade.proposerBudgetSendAmount" class="component-row">• ${{ trade.proposerBudgetSendAmount }} of budget</div>
        </div>
      </div>
      <div class="trade-status-list">
        <h5>Action Log</h5>
        <div>• Trade proposed by '{{ trade.proposerPublisherName }}' on {{ formatDateTimeAt(trade.proposedTimestamp) }}.</div>
        <label>Message from {{ trade.proposerPublisherName }}:</label>
        <p class="component-row">{{ trade.message }}</p>
        <div v-if="trade.acceptedTimestamp">• Trade accepted by '{{ trade.counterPartyPublisherName }}' on {{ formatDateTimeAt(trade.acceptedTimestamp) }}.</div>

        <div v-if="trade.status === 'RejectedByCounterParty'">• Trade rejected by '{{ trade.counterPartyPublisherName }}' on {{ formatDateTimeAt(trade.completedTimestamp) }}.</div>
        <div v-if="trade.status === 'RejectedByManager'">• Trade rejected by league manager on {{ formatDateTimeAt(trade.completedTimestamp) }}.</div>
        <div v-if="trade.status === 'Rescinded'">• Trade rescinded by '{{ trade.proposerPublisherName }}' on {{ formatDateTimeAt(trade.completedTimestamp) }}.</div>
        <div v-if="trade.status === 'Executed'">• Trade executed by league manager on {{ formatDateTimeAt(trade.completedTimestamp) }}.</div>
        <div v-if="trade.status === 'Expired'">• Trade expired automatically on {{ formatDateTimeAt(trade.completedTimestamp) }}.</div>

        <div v-if="tradeIsActive && trade.error">
          <div v-if="trade.error" class="alert alert-warning">
            <p>This trade is no longer valid due to the following error:</p>
            <p>
              {{ trade.error }}
            </p>

            <template v-if="requiredLeague.isManager">
              <p>The only option at this point is to reject the trade. Another trade can be proposed if the players wish.</p>
              <b-button variant="success" @click="managerRejectTrade">Reject Trade</b-button>
            </template>
            <template v-if="isProposer">
              <p>The only option at this point is to rescind the trade. Another trade can be proposed if you wish.</p>
              <b-button variant="success" @click="rescindTrade">Rescind Trade</b-button>
            </template>
            <template v-if="isCounterParty">
              <p>The only option at this point is to reject the trade. Another trade can be proposed if you wish.</p>
              <b-button variant="success" @click="rejectTrade">Reject Trade</b-button>
            </template>
          </div>
        </div>
        <div v-else>
          <div v-if="trade.status === 'Proposed' && !isCounterParty" class="alert alert-info">
            Trade is waiting for approval from '{{ trade.counterPartyPublisherName }}'.
            <div>Trade will expire on {{ formatDateTimeAt(trade.willExpireTimestamp) }}.</div>
            <b-button v-if="isProposer" variant="danger" @click="rescindTrade">Rescind Trade</b-button>
          </div>
          <div v-if="trade.status === 'Accepted' && isProposer" class="alert alert-info">
            Trade has been accepted by '{{ trade.counterPartyPublisherName }}', but you can still rescind it.
            <div>Trade will expire on {{ formatDateTimeAt(trade.willExpireTimestamp) }}.</div>
            <b-button variant="danger" @click="rescindTrade">Rescind Trade</b-button>
          </div>
          <div v-if="trade.status === 'Proposed' && isCounterParty" class="alert alert-info">
            This trade is waiting for your approval.
            <div>Trade will expire on {{ formatDateTimeAt(trade.willExpireTimestamp) }}.</div>
            <b-button variant="success" @click="acceptTrade">Accept</b-button>
            <b-button variant="danger" @click="rejectTrade">Reject</b-button>
          </div>
          <div v-if="trade.status === 'Accepted' && isCounterParty" class="alert alert-info">
            You accepted this trade, but you are free to change your mind.
            <div>Trade will expire on {{ formatDateTimeAt(trade.willExpireTimestamp) }}.</div>
            <b-button variant="danger" @click="rejectTrade">Reject Trade</b-button>
          </div>

          <div v-if="trade.status === 'Accepted'">
            <div v-if="requiredLeague.isManager" class="alert alert-info">League members can now vote on this trade.</div>
            <div v-else class="alert alert-info">League members can now vote on this trade, and the league manager can execute or reject it.</div>

            <div v-if="isInLeagueButNotInvolved">
              When considering a trade, you should ask "is this trade reasonable?" A reasonable trade is beneficial to both parties. The following situations are cases where trades should not be
              approved:
              <ul>
                <li>A player is already losing and has agreed to a bad trade to help the other player beat a third player not involved in the trade.</li>
                <li>A player is taking a bad trade this year with the understanding that they will be 'paid back' next year.</li>
                <li>You suspect that there is some other "shady deal" going on, such as real money being exchanged behind the scenes.</li>
              </ul>
              However, you should
              <em>not</em>
              be considering whether or not this trade is good for
              <em>you.</em>
              If it's a fair trade for these two parties, you should vote to approve.
            </div>

            <div v-if="canVote" class="alert alert-secondary">
              <h5>Vote</h5>
              <div class="form-group">
                <input v-model="comment" type="text" class="form-control" aria-label="Comment" placeholder="Enter a comment (Optional)" />
              </div>
              <b-button variant="success" @click="vote(true)">Vote Approve</b-button>
              <b-button variant="danger" @click="vote(false)">Vote Reject</b-button>
            </div>

            <div v-if="requiredLeague.isManager && !trade.error" class="alert alert-warning">
              <p>
                As league manager, you ultimately make the choice to execute or reject the trade. If you reject it, the full record of the trade goes to the league history page, but nothing else
                happens. If you execute the trade, the games and budget (if applicable) will change hands immediately, and again, the full record will be visible on the league history page.
              </p>
              <p>You should, however, weigh your league's votes when making your decision.</p>

              <b-button variant="success" @click="executeTrade">Execute Trade</b-button>
              <b-button variant="danger" @click="managerRejectTrade">Reject Trade</b-button>
            </div>
          </div>

          <div v-if="trade.status === 'Accepted' && trade.votes.length === 0" class="alert alert-info">There are no votes yet.</div>

          <div v-if="trade.votes.length > 0">
            <h4>Votes</h4>
            <b-table-lite :items="trade.votes" :fields="voteFields" bordered responsive striped>
              <template #cell(approved)="data">
                {{ formatApprovedRejected(data.item.approved) }}
              </template>

              <template #cell(comment)="data">
                {{ data.item.comment }}
                <template v-if="data.item.userID === userInfo?.userID && !trade.completedTimestamp">
                  <b-button variant="danger" size="sm" @click="deleteVote">Delete Vote</b-button>
                </template>
              </template>
            </b-table-lite>
          </div>
        </div>
      </div>

      <div v-show="errorInfo" class="alert alert-danger">
        <h3>Error!</h3>
        {{ errorInfo }}
      </div>
    </template>
  </collapseCard>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import { mapActions, mapState } from 'pinia';

import { leagueClient, leagueManagerClient, type BasicTradeRequest, type TradeViewModel, type TradeVoteRequest } from '@/api/clients';
import { except, formatLongDateTime, startCase } from '@/globalFunctions';
import { useAuthStore } from '@/store/authStore';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatApprovedRejected, formatDateTimeAt } from '@/utilities/formatters';
import CollapseCard from '@/components/collapseCard.vue';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default defineComponent({
  components: {
    MasterGamePopover,
    CollapseCard
  },
  props: {
    trade: { type: Object as PropType<TradeViewModel>, required: true },
    defaultVisible: { type: Boolean }
  },
  data() {
    return {
      comment: '',
      voteFields: [
        { key: 'displayName', label: 'Display Name', thClass: ['bg-primary'] },
        { key: 'approved', label: 'Vote', thClass: ['bg-primary'] },
        { key: 'comment', label: 'Comment', thClass: ['bg-primary'] }
      ],
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useAuthStore, ['userInfo']),
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'userPublisher', 'requiredLeague']),
    recievesVerbiage(): string {
      if (this.tradeIsActive) {
        return 'Receives';
      }

      if (this.trade.status === 'Executed') {
        return 'Received';
      }

      return 'Would have Received';
    },
    isProposer(): boolean {
      return this.trade.proposerPublisherID === this.userPublisher?.publisherID;
    },
    isCounterParty(): boolean {
      return this.trade.counterPartyPublisherID === this.userPublisher?.publisherID;
    },
    tradeIsActive(): boolean {
      return this.trade.status === 'Proposed' || this.trade.status === 'Accepted';
    },
    isInLeagueButNotInvolved(): boolean {
      let involvedParties = [this.trade.proposerUserID, this.trade.counterPartyUserID];
      let allUserIDsInLeague = this.requiredLeagueYear.players.filter((x) => !!x.user).map((x) => x.user?.userID);
      let nonInvolvedParties = except(allUserIDsInLeague, involvedParties);
      return nonInvolvedParties.includes(this.userInfo?.userID);
    },
    canVote(): boolean {
      if (!this.userInfo) {
        return false;
      }

      let votedUserIDs = this.trade.votes.map((x) => x.userID);
      let alreadyVoted = votedUserIDs.includes(this.userInfo.userID);
      return this.isInLeagueButNotInvolved && !alreadyVoted;
    },
    header(): string {
      let finalHeader = `Trade between ${this.trade.proposerPublisherName} and ${this.trade.counterPartyPublisherName}`;
      if (this.trade.status === 'Proposed' || this.trade.status === 'Accepted') {
        const proposedDate = this.formatLongDateTime(this.trade.proposedTimestamp);
        finalHeader += ` (Proposed on ${proposedDate})`;
      } else {
        if (!this.trade.completedTimestamp) {
          throw new Error("Trade should have a completedTimestamp, but it doesn't");
        }
        const completedDate = this.formatLongDateTime(this.trade.completedTimestamp);
        finalHeader += ` (${startCase(this.trade.status)} on ${completedDate})`;
      }

      return finalHeader;
    }
  },
  methods: {
    formatLongDateTime,
    ...mapActions(useLeagueStore, ['notifyAction']),
    formatApprovedRejected,
    formatDateTimeAt,
    async acceptTrade(): Promise<void> {
      await this.sendGenericTradeRequest((request) => leagueClient.acceptTrade(request), 'You accepted a trade.');
    },
    async rejectTrade(): Promise<void> {
      await this.sendGenericTradeRequest((request) => leagueClient.rejectTrade(request), 'You rejected a trade.');
    },
    async rescindTrade(): Promise<void> {
      await this.sendGenericTradeRequest((request) => leagueClient.rescindTrade(request), 'You rescinded a trade.');
    },
    async executeTrade(): Promise<void> {
      await this.sendGenericTradeRequest((request) => leagueManagerClient.executeTrade(request), 'Trade has been executed.');
    },
    async managerRejectTrade(): Promise<void> {
      await this.sendGenericTradeRequest((request) => leagueManagerClient.rejectTrade(request), 'Trade has been rejected.');
    },
    async sendGenericTradeRequest(action: (request: BasicTradeRequest) => Promise<void>, message: string): Promise<void> {
      const model = {
        tradeID: this.trade.tradeID,
        leagueID: this.requiredLeagueYear.leagueID,
        year: this.requiredLeagueYear.year
      };

      try {
        await action(model);
        this.notifyAction(message);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong with your request.');
      }
    },
    async vote(approved: boolean): Promise<void> {
      const model: TradeVoteRequest = {
        tradeID: this.trade.tradeID,
        leagueID: this.requiredLeagueYear.leagueID,
        year: this.requiredLeagueYear.year,
        approved,
        comment: this.comment
      };

      try {
        await leagueClient.voteOnTrade(model);
        this.notifyAction('Voted on trade.');
        this.comment = '';
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to vote on trade.');
      }
    },
    async deleteVote(): Promise<void> {
      const model: BasicTradeRequest = {
        tradeID: this.trade.tradeID,
        leagueID: this.requiredLeagueYear.leagueID,
        year: this.requiredLeagueYear.year
      };

      try {
        await leagueClient.deleteTradeVote(model);
        this.notifyAction('Deleted vote on trade.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to delete vote on trade.');
      }
    }
  }
});
</script>

<style scoped>
.component-row {
  width: 100%;
  background-color: #555555;
  margin-bottom: 5px;
  padding: 5px;
  border-radius: 10px;
}

div,
p,
label {
  color: white;
}

.trade-status-list div {
  margin-bottom: 10px;
}
</style>
