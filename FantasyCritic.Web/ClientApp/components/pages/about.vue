<template>
  <div>
    <h1>About</h1>
    <hr />
    <div class="col-lg-8 col-md-10 col-sm-12">
      <div class="text-well">
        <p>
          Fantasy Critic was created by me, Steve Fallon. The visual design for the site was created by <a href="http://dalemahabir.rocks/" target="_blank">Dale Mahabir <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
          As of now, this site is brand new and there are bound to be bugs, oversights, and design issues. However, I am committed to improving the site for the foreseeable future.
          I created this site because I wanted to merge my love of video games with my love of fantasy football. I think anyone that likes both of those things will have a great time playing Fantasy Critic.
        </p>

        <p>
          This site could not exist without <a href="https://opencritic.com/" target="_blank">opencritic.com <font-awesome-icon icon="external-link-alt" size="sm" /></a>. Review aggregation is time consuming work and I'm very grateful they are allowing me to use their API.
          Consider supporting them on <a href="https://www.patreon.com/OpenCritic/" target="_blank">Patreon <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
        </p>

        <p>
          If you want to follow updates about the site, join our <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">subreddit <font-awesome-icon icon="external-link-alt" size="sm" /></a>
          and follow <a href="https://twitter.com/fantasy_critic" target="_blank">@fantasy_critic <font-awesome-icon icon="external-link-alt" size="sm" /></a> on Twitter
        </p>

        <p>
          This site is open source. If you want to help work on features or fix bugs, contact me on <a href="https://github.com/SteveF92/FantasyCritic" target="_blank">GitHub <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
        </p>
      </div>
      <hr />
      <h2>A little history</h2>
      <div class="text-well">
        <p>
          I had the idea for Fantasy Critic in late 2017. I was already a big fantasy football fan, and I had a group of friends that I wanted to form a league with. Problem: most of them don't really like football.
          So I got to thinking about how to apply the idea of fantasy football to something we do all like. I'm not totally sure when the moment was, but that's about when I had the idea for Fantasy Critic.
          Before it was ever a website, it was a 3 page document outlining the basic rules. Some of the details have changed, but the core idea of the game hasn't changed since then. My friends and I played through
          all of 2018 using a Google Sheet as our record keeper.
        </p>
        <p>
          I wrote the first bits of code that would become this site in January 2018, but I didn't really begin in earnest until June. For most of the second half of 2018, I worked on Fantasy Critic nearly every day.
          I hired a friend of mine, Dale Mahabir, to help with the visual design of the site in late September. Things started taking shape quickly in the last few months of the year. I "soft" launched the site
          in December 2018, by posting on a few programming subreddits.
        </p>

        <p>
          At the time I'm writing this (early January 2019), the site is "officially" launched, but we're just getting started. The site's first major advertisement -
          on the <a href="https://www.youtube.com/channel/UCZrxXp1reP8E353rZsB3jaA" target="_blank">Easy Allies <font-awesome-icon icon="external-link-alt" size="sm" /></a> Podcast - airs in about a week.
          I couldn't be more excited for people to start playing. I hope you find it as entertaining as my friends and I already have.
        </p>
      </div>
      <hr />
      <h2>Technical Credits</h2>
      <div class="text-well">
        <p>
          This site uses the following software packages heavily, and I wouldn’t be able to create this site without them.
        </p>

        <ul>
          <li>ASP.NET Core</li>
          <li>Vue.JS</li>
          <li>MySQL</li>
          <li>Bootstrap</li>
          <li>Vue-Bootstrap</li>
          <li>Webpack</li>
          <li>SendGrid</li>
          <li>SignalR</li>
          <li>NodaTime</li>
          <li>Dapper</li>
          <li>Newtonsoft.JSON</li>
          <li>MoreLINQ</li>
          <li>CSharpFunctionalExtensions</li>
          <li>NLog</li>
          <li>NUnit</li>
        </ul>
      </div>
    </div>
    <hr />
    <h2>A few statistics...</h2>
    <siteCounts></siteCounts>
  </div>
</template>
<script>
  import SiteCounts from "components/modules/siteCounts";

  export default {
    components: {
      SiteCounts
    }
  }
</script>
