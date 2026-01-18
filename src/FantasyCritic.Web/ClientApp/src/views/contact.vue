<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Contact</h1>
    <hr />
    <h2>Adding new Games/Corrections</h2>
    <div class="text-well">
      <ul>
        <li>
          Please use the
          <router-link :to="{ name: 'masterGameRequest' }">Master Game Request form</router-link>
          for adding new games.
        </li>
        <li>Please use the "Suggest Correction" form, which can be accessed by clicking on a master game link and clicking the "Suggest a Correction" option for correcting existing games.</li>
        <li>
          If you have a particularly "contentious" question, like debating what tags a game should have, please bring those questions to #eligibility-debates in our
          <a href="https://discord.gg/dNa7DD3">
            Discord
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          .
        </li>
      </ul>
    </div>

    <h2>General Questions</h2>
    <div class="text-well">
      If you have a question that is not answered by the
      <router-link :to="{ name: 'faq' }">FAQ page</router-link>
      , you can probably get a good answer from our
      <router-link :to="{ name: 'community' }">community</router-link>
      , particularly on Discord.
    </div>

    <h2>How to Reach Me Directly</h2>
    <div class="text-well">
      <p>I prefer that people contact me on Discord. It's the only place I check regularly. My username is "elitefusion" and I can be found at the top of our members list in the official server.</p>
      <p>
        If for some reason Discord is not an option for you, you can contact me at
        <a href="mailto:steve.fallon@fantasycritic.games">steve.fallon@fantasycritic.games</a>
        .
      </p>
      <h3>Account issues</h3>
      <p>
        If you have an issue with your account(can't login, can't verify email, etc), please reach out to me directly via DM on Discord. Additionally, if one of your league members is having issues
        with their account, you should have them reach out to me directly, rather than contacting me on their behalf.
      </p>
      <h3>Other Issues</h3>
      <p>
        I'm always happy to respond to a DM, but if your issue is not an account issue, you can also post in our #tech-support channel in our Discord. In particular, if you have an issue with the
        <router-link :to="{ name: 'discordBot' }">Discord Bot</router-link>
        , you should definitely post that in the channel as opposed to a DM.
      </p>
    </div>
  </div>
</template>
