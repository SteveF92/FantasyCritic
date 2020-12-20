<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>How to Play</h1>
      <div class="text-well">
        <p>
          The goal of Fantasy Critic is to assemble the best roster of games released in a single year. Basically, you want to pick games you think will review well.
        </p>
        <p>
          Here's a more detailed breakdown:
        </p>
      </div>
      <hr />
      <div class="row">
        <div class="col-xl-3 col-lg-6 col-md-12 how-step">
          <div class="how-step-header">
            <h1>1</h1>
            <font-awesome-icon class="step-icon" icon="users" size="8x" />
          </div>
          <h2>Create or join a league</h2>
          <label>
            Grab your friends and choose how you’d like your league configured.
          </label>
        </div>
        <div class="col-xl-3 col-lg-6 col-md-12 how-step">
          <div class="how-step-header">
            <h1>2</h1>
            <font-awesome-icon class="step-icon" icon="calendar-day" size="8x" />
          </div>
          <h2>Plan your draft</h2>
          <label>
            While there is no time limit, the earlier in the year the better.
          </label>
        </div>
        <div class="col-xl-3 col-lg-6 col-md-12 how-step">
          <div class="how-step-header">
            <h1>3</h1>
            <font-awesome-icon class="step-icon" icon="list-ol" size="8x" />
          </div>
          <h2>Draft your games</h2>
          <label>
            Take turns adding next year’s hottest titles to your rosters.
          </label>
        </div>
        <div class="col-xl-3 col-lg-6 col-md-12 how-step">
          <div class="how-step-header">
            <h1>4</h1>
            <font-awesome-icon class="step-icon" icon="calendar-alt" size="8x" />
          </div>
          <h2>Watch the releases</h2>
          <label>
            Find out what critics are thinking of your games and how your rankings change.
          </label>
        </div>
      </div>
      <hr />
      <div class="row">
        <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0">
          <h2>Create a League</h2>
          <div class="text-well">
            Rally your friends and start a fantasy critic league of your own.
            <ul>
              <li>Designate one member from your group as the league manager.</li>
              <li>Have them create an account and then create a league. Here, they’ll be able to decide:</li>
              <li class="sub-list">
                <ul>
                  <li>How many games your group will draft.</li>
                  <li>How many games your group will counter pick.</li>
                  <li>What games are eligible in your league.</li>
                </ul>
              </li>
              <li>
                From there, the league manager will invite individual players who will then name their publishers and get ready to pick their games!
              </li>
            </ul>
          </div>
        </div>
      </div>
      <hr />

      <div class="row">
        <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0">
          <h2>Plan your draft</h2>
          <div class="text-well">
            <p>
              The earlier in the year you can do your draft, the better, as there are more games still to be released. The site doesn't schedule your draft for you, start it automatically, or enforce any time
              limits, but we encourage you to communicate with your league about when you want to hold the draft, and set aside some time to complete your draft in one session.
            </p>
            <p>
              You can run your draft with:
              <ul>
                <li>Each player logged into the site on their own devices</li>
                <li>The league manager handling all assignments</li>
              </ul>
              Either will work fine, or you can have a mix. For example, if you have a six person league, with five of them together in person and one more absent, the league manager can handle drafting
              for the five while the other player handles their games. It makes no difference.
            </p>
            <p>
              However many people you have, we <em>strongly</em> reccommend setting aside at least an hour to dedicate to your draft. While being together in person is not required, we do think that
              is the most fun way to do it. Unless there's a pandemic. Then take appropriate precautions.
            </p>
            <p>
              Finally, make sure to set your draft order. Different groups may like to decide this in different ways. There is no obvious best position in the draft, so different players may prefer to be
              first, last, or somewhere in the middle. If you don't change anything, the system will default to the order in which people joined the league. You can choose to set a custom order,
              or you can tell the site to randomize the order.
            </p>
          </div>
        </div>
      </div>
      <hr />

      <div class="row">
        <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0">
          <h2>Draft your games</h2>
          <div class="text-well">
            <p>
              Okay, so it's draft time! The league manager starts the draft, and the first player up will choose their first game. After that, each player will follow suit. After every player has a game, the
              player that choose last in the first round will choose first in the second round. This is called a "snake draft".
            </p>
            <p>
              Once every player has drafted all of their games, each player will choose their counter picks.
              For more info, <a href="/faq#scoring">click here.</a>
            </p>
          </div>
        </div>
      </div>
      <hr />

      <div class="row">
        <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0">
          <h2>Watch the releases</h2>
          <div class="text-well">
            Throughout the year, check in on your game’s release dates!
            <ul>
              <li>Check out how reviews on opencritic.com reflect in your scores on your league dashboard.</li>
              <li>Make your mid year pickups. For more info, <a href="/faq#bidding-system">click here.</a></li>
              <li>At the end of your league year, one person will be crowned the winner.</li>
            </ul>
            What do you win? Well, that's up to you and your league. Creative prizes are encouraged!
          </div>
        </div>
      </div>
      <hr />

      <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0">
        <h3>
          If you have any more questions, check out our <router-link :to="{ name: 'faq' }" title="FAQ">FAQ</router-link> page,
          or <router-link :to="{ name: 'contact' }" title="Contact">contact</router-link> me directly!
        </h3>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .how-step-header{
    display: flex;
  }
  .how-step h1 {
   
  }
  .how-step .step-icon {
    margin-left: 80px;
  }

  .sub-list {
    list-style: none;
  }
</style>
