<template>
  <div>
    <collapseCard :defaultVisible="defaultVisible">
      <div slot="header">{{header}}</div>
      <div slot="body">
        <div class="row">
          <div class="col-6">
            <h4>{{trade.proposerPublisherName}}</h4>
            <h5>Player: {{trade.proposerDisplayName}}</h5>
            <h5>Receives</h5>
            <div v-for="game in trade.counterPartySendGames" class="component-row">
              • <masterGamePopover :masterGame="game.masterGameYear"></masterGamePopover>
            </div>
            <div v-if="trade.counterPartyBudgetSendAmount" class="component-row">
              • ${{trade.counterPartyBudgetSendAmount}} of budget
            </div>
          </div>
          <div class="col-6">
            <h4>{{trade.counterPartyPublisherName}}</h4>
            <h5>Player: {{trade.counterPartyDisplayName}}</h5>
            <h5>Receives</h5>
            <div v-for="game in trade.proposerSendGames" class="component-row">
              • <masterGamePopover :masterGame="game.masterGameYear"></masterGamePopover>
            </div>
            <div v-if="trade.proposerBudgetSendAmount" class="component-row">
              • ${{trade.proposerBudgetSendAmount}} of budget
            </div>
          </div>
        </div>
        <div class="trade-status-list">
          <h5>Action Log</h5>
          <div>• Trade proposed by '{{trade.proposerPublisherName}}' at {{trade.proposedTimestamp | dateTime}}.</div>
          <div v-if="trade.acceptedTimestamp">• Trade accepted by '{{trade.counterPartyPublisherName}}' at {{trade.acceptedTimestamp | dateTime}}.</div>

          <div v-if="trade.status === 'RejectedByCounterParty'">• Trade rejected by '{{trade.counterPartyPublisherName}}' at {{trade.completedTimestamp | dateTime}}.</div>
          <div v-if="trade.status === 'RejectedByManager'">• Trade rejected by league manager at {{trade.completedTimestamp | dateTime}}.</div>
          <div v-if="trade.status === 'Rescinded'">• Trade rescinded by '{{trade.proposerPublisherName}}' at {{trade.completedTimestamp | dateTime}}.</div>
          <div v-if="trade.status === 'Executed'">• Trade executed by league manager at {{trade.completedTimestamp | dateTime}}.</div>


          <div v-if="tradeIsActive && trade.error">
            <div class="alert alert-warning" v-if="trade.error">
              <p>
                This trade is no longer valid due to the following error:
              </p>
              <p>
                {{trade.error}}
              </p>

              <template v-if="league.isManager">
                <p>The only option at this point is to reject the trade. Another trade can be proposed if the players wish.</p>
                <b-button variant="success" v-on:click="managerRejectTrade">Reject Trade</b-button>
              </template>
              <template v-if="isProposer">
                <p>The only option at this point is to rescind the trade. Another trade can be proposed if you wish.</p>
                <b-button variant="success" v-on:click="rescindTrade">Rescind Trade</b-button>
              </template>
              <template v-if="isCounterParty">
                <p>The only option at this point is to reject the trade. Another trade can be proposed if you wish.</p>
                <b-button variant="success" v-on:click="rejectTrade">Rescind Trade</b-button>
              </template>
            </div>
          </div
>
          <div v-else>
            <div class="alert alert-info" v-if="trade.status === 'Proposed' && !isCounterParty">
              Trade is waiting for approval from '{{trade.counterPartyPublisherName}}'.
              <b-button v-if="isProposer" variant="danger" v-on:click="rescindTrade">Rescind Trade</b-button>
            </div>
            <div class="alert alert-info" v-if="trade.status === 'Accepted' && isProposer">
              Trade is has been accepted by '{{trade.counterPartyPublisherName}}', but you can still rescind it.
              <b-button variant="danger" v-on:click="rescindTrade">Rescind Trade</b-button>
            </div>
            <div class="alert alert-info" v-if="trade.status === 'Proposed' && isCounterParty">
              This trade is waiting for your approval.
              <b-button variant="success" v-on:click="acceptTrade">Accept</b-button>
              <b-button variant="danger" v-on:click="rejectTrade">Reject</b-button>
            </div>
            <div class="alert alert-info" v-if="trade.status === 'Accepted' && isCounterParty">
              You accepted this trade, but you are free to change your mind.
              <b-button variant="danger" v-on:click="rejectTrade">Reject Trade</b-button>
            </div>

            <div v-if="trade.status === 'Accepted'">
              <div class="alert alert-info" v-if="league.isManager">
                League members can now vote on this trade.
              </div>
              <div class="alert alert-info" v-else>
                League members can now vote on this trade, and the league manager can execute or reject it.
              </div>

              <p v-if="isInLeagueButNotInvolved">
                When considering a trade, you should ask "is this trade reasonable?"
                A reasonable trade is beneficial to both parties. The following situations are cases where trades should not be approved:
                <ul>
                  <li>A player is already losing and has agreed to a bad trade to help the other player beat a third player not involved in the trade.</li>
                  <li>A player is taking a bad trade this year with the understanding that they will be 'paid back' next year.</li>
                  <li>You suspect that there is some other "shady deal" going on, such as real money being exchanged behind the scenes.</li>
                </ul>
                However, you should <em>not</em> be considering whether or not this trade is good for <em>you.</em> If it's a fair trade for these two parties, you should vote to approve.
              </p>

              <div v-if="canVote" class="alert alert-secondary">
                <h5>Vote</h5>
                <div class="form-group">
                  <input type="text" v-model="comment" class="form-control" aria-describedby="emailHelp" placeholder="Enter a comment (Optional)">
                </div>
                <b-button variant="success" v-on:click="vote(true)">Vote Approve</b-button>
                <b-button variant="danger" v-on:click="vote(false)">Vote Reject</b-button>
              </div>

              <div class="alert alert-warning" v-if="league.isManager && !trade.error">
                <p>
                  As league manager, you ultimately make the choice to execute or reject the trade. If you reject it, the full record of the trade goes to the league history page, but nothing else happens.
                  If you execute the trade, the games and budget (if applicable) will change hands immediately, and again, the full record will be visible on the league history page.
                </p>
                <p>
                  You should, however, weigh your leagues votes when making your decision.
                </p>

                <b-button variant="success" v-on:click="executeTrade">Execute Trade</b-button>
                <b-button variant="danger" v-on:click="managerRejectTrade">Reject Trade</b-button>
              </div>
            </div>

            <div class="alert alert-info" v-if="trade.status === 'Accepted' && trade.votes.length === 0">
              There are no votes yet.
            </div>

            <div v-if="trade.votes.length > 0">
              <h4>Votes</h4>
              <b-table-lite :items="trade.votes" :fields="voteFields"
                            bordered responsive striped>

                <template v-slot:cell(approved)="data">
                  {{data.item.approved | approvedRejected}}
                </template>

                <template v-slot:cell(comment)="data">
                  {{data.item.comment}}
                  <template v-if="data.item.userID === userInfo.userID && !trade.completedTimestamp">
                    <b-button variant="danger" size="sm" v-on:click="deleteVote">Delete Vote</b-button>
                  </template>
                </template>
              </b-table-lite>
            </div>
          </div>
        </div>

        <div class="alert alert-danger" v-show="errorInfo">
          <h3>Error!</h3>
          {{errorInfo}}
        </div>
      </div>
    </collapseCard>
  </div>
</template>

<script>
import axios from 'axios';
import MasterGamePopover from '@/components/modules/masterGamePopover';
import CollapseCard from '@/components/modules/collapseCard';
import GlobalFunctions from '@/globalFunctions';

export default {
  props: ['trade', 'league', 'leagueYear', 'publisher', 'defaultVisible'],
  components: {
    MasterGamePopover,
    CollapseCard,
    GlobalFunctions
  },
  data() {
    return {
      comment: "",
      voteFields: [
        { key: 'displayName', label: 'Display Name', thClass: ['bg-primary'] },
        { key: 'approved', label: 'Vote', thClass: ['bg-primary'] },
        { key: 'comment', label: 'Comment', thClass: ['bg-primary'] },
      ],
      errorInfo: ""
    };
  },
  computed: {
    isProposer() {
      return this.trade.proposerPublisherID === this.publisher.publisherID;
    },
    isCounterParty() {
      return this.trade.counterPartyPublisherID === this.publisher.publisherID;
    },
    tradeIsActive() {
      return this.trade.status === 'Proposed' || this.trade.status === 'Accepted';
    },
    isInLeagueButNotInvolved() {
      let involvedParties = [
        this.trade.proposerUserID,
        this.trade.counterPartyUserID
      ];
      let allUserIDsInLeague = this.leagueYear.players.map(x => x.user.userID);
      var nonInvolvedParties = _.difference(allUserIDsInLeague, involvedParties);
      return nonInvolvedParties.includes(this.userInfo.userID);
    },
    canVote() {
      let votedUserIDs = this.trade.votes.map(x => x.userID);
      let alreadyVoted = votedUserIDs.includes(this.userInfo.userID);
      return this.isInLeagueButNotInvolved && !alreadyVoted;
    },
    userInfo() {
      return this.$store.getters.userInfo;
    },
    header() {
      let finalHeader = `Trade between ${this.trade.proposerPublisherName} and ${this.trade.counterPartyPublisherName}`;
      if (this.trade.status === 'Proposed' || this.trade.status === 'Accepted') {
        const proposedDate = GlobalFunctions.formatLongDate(this.trade.proposedTimestamp);
        finalHeader += ` (Proposed on ${proposedDate})`;
      } else {
        const completedDate = GlobalFunctions.formatLongDate(this.trade.completedTimestamp);
        finalHeader += ` (${_.startCase(this.trade.status)} on ${completedDate})`;
      }

      return finalHeader;
    }
  },
  methods: {
    acceptTrade() {
      this.sendGenericTradeRequest('league/AcceptTrade');
    },
    rejectTrade() {
      this.sendGenericTradeRequest('league/RejectTrade');
    },
    rescindTrade() {
      this.sendGenericTradeRequest('league/RescindTrade');
    },
    executeTrade() {
      this.sendGenericTradeRequest('leagueManager/ExecuteTrade');
    },
    managerRejectTrade() {
      this.sendGenericTradeRequest('leagueManager/RejectTrade');
    },
    sendGenericTradeRequest(endPoint) {
      var model = {
        tradeID: this.trade.tradeID
      };
      axios
        .post(`/api/${endPoint}`, model)
        .then(response => {
          this.$emit('tradeActioned');
        })
        .catch(response => {
          this.errorInfo = response.response.data;
        });
    },
    vote(approved) {
      var model = {
        tradeID: this.trade.tradeID,
        approved,
        comment: this.comment
      };
      axios
        .post('/api/league/VoteOnTrade', model)
        .then(response => {
          this.$emit('tradeActioned');
          this.comment = "";
        })
        .catch(response => {
          this.errorInfo = response.response.data;
        });
    },
    deleteVote() {
      var model = {
        tradeID: this.trade.tradeID
      };
      axios
        .post('/api/league/DeleteTradeVote', model)
        .then(response => {
          this.$emit('tradeActioned');
        })
        .catch(response => {
          this.errorInfo = response.response.data;
        });
    }
  }
};
</script>
<style scoped>
  .component-row {
    width: 100%;
    background-color: #555555;
    margin-bottom: 3px;
    padding: 5px;
    border-radius: 10px;
  }

  div, p, label {
    color: white;
  }

  .trade-status-list div {
      margin-bottom: 10px;
  }
</style>
