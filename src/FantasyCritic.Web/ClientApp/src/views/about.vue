<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>About</h1>
      <hr />
      <div class="text-well">
        <p>
          Fantasy Critic was created by me, Steve Fallon. The visual design for the site was created by
          <a href="http://dalemahabir.rocks/" target="_blank">
            Dale Mahabir
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          . I work on the site solely in my spare time, and there are bound to be bugs, oversights, and design issues. However, I am committed to improving the site for the foreseeable future. I
          created this site because I wanted to merge my love of video games with my love of fantasy football. I think anyone that likes both of those things will have a great time playing Fantasy
          Critic.
        </p>

        <p>
          This site could not exist without
          <a href="https://opencritic.com/" target="_blank">
            opencritic.com
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          . Review aggregation is time consuming work and I'm very grateful they are allowing me to use their API. Consider supporting them on
          <a href="https://www.patreon.com/OpenCritic/" target="_blank">
            Patreon
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          .
        </p>

        <p>
          If you want to follow updates about the site, join our
          <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">
            subreddit
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          and follow
          <a href="https://twitter.com/fantasy_critic" target="_blank">
            @fantasy_critic
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          on Twitter
        </p>

        <p>
          This site is open source. If you want to help work on features or fix bugs, contact me on
          <a href="https://github.com/SteveF92/FantasyCritic" target="_blank">
            GitHub
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          .
        </p>
      </div>
      <hr />
      <h2>A little history</h2>
      <div class="text-well">
        <h4>The beginnings</h4>
        <p>
          I had the idea for Fantasy Critic in late 2017. I was already a big fantasy football fan, and I had a group of friends that I wanted to form a league with. Problem: most of them don't really
          like football. So I got to thinking about how to apply the idea of fantasy football to something we do all like. I'm not totally sure when the moment was, but that's about when I had the
          idea for Fantasy Critic. Before it was ever a website, it was a 3 page document outlining the basic rules. Some of the details have changed, but the core idea of the game hasn't changed
          since then. My friends and I played through all of 2018 using a Google Sheet as our record keeper.
        </p>
        <p>
          I wrote the first bits of code that would become this site in January 2018, but I didn't really begin in earnest until June. For most of the second half of 2018, I worked on Fantasy Critic
          nearly every day. I hired a friend of mine, Dale Mahabir, to help with the visual design of the site in late September. Things started taking shape quickly in the last few months of the
          year. I "soft" launched the site in December 2018, by posting on a few programming subreddits.
        </p>

        <p>
          At the time I'm writing this section (early January 2019), the site is "officially" launched, but we're just getting started. The site's first major advertisement - on the
          <a href="https://www.youtube.com/channel/UCZrxXp1reP8E353rZsB3jaA" target="_blank">
            Easy Allies
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          Podcast - airs in about a week. I couldn't be more excited for people to start playing. I hope you find it as entertaining as my friends and I already have.
        </p>

        <h4>2 Years later</h4>
        <p>
          Fantasy Critic has now been live for 2 full years. The site has grown from just my closest friends
          <span class="inside-joke">(and one buddy)</span>
          to nearly ten thousand users. There are now a handful of extremely engaged users that frequently assist with answering user questions, fixing errors with the games list, and even
          contributing code. It's so incredible to me to see this community spring up around this thing I created. If you've been a user for a while, or you're just starting now, please know that I'm
          super happy to have you here.
        </p>
        <p>
          The site continues to grow in features as well. In late 2019 I added "Critic's Royale", with the goal of making the site easier to jump into. 2020 saw the addition of "game dropping", which
          was intended to keep people engaged throughout the year, even if some of their games get delayed. For 2021, the big new feature so far is a streamlining of the "eligibility" system, which
          let's you choose what games you can select in your leagues. I have more planned for later in the year as well, including a way to handle "shadow drops".
        </p>
      </div>
      <hr />
      <h2><a name="privacy">Privacy Policy</a></h2>
      <div class="text-well">
        <p>
          Your privacy is important to us. As much as possible, we try to limit the amount of data we collect from you. That said, there are certain things which are necessary for the site to
          function. By using this site, you agree to these terms.
        </p>
        <h4>Data we gather</h4>
        <p>
          Making an account on the site requires an email address. We don't ask for your real name or any other personally identifiable information. All passwords are stored using industry standard
          hashing + salting. The site is open source, so if you are interested, you can check that part out for yourself.
          <br />
          Additionally, the site uses Google Analytics to track engagement with the site (which pages are used, what devices users are accessing the site from). However, we do not use Google's
          "Demographics and Interest Reports" feature, so we are not collecting any data about users advertising interests, age groups, etc.
        </p>
        <h4>Public Information</h4>
        <p>Users can choose to partipate in leagues that are accessible to the public, or to keep their leagues private. User email addresses are not exposed to other users in either case.</p>
        <h4>Cookies</h4>
        <p>Fantasy Critic uses cookies to manage user authentication, but we don't track you for advertising purposes or anything like that.</p>
        <h4>Advertising</h4>
        <p>At this time, Fantasy Critic does not run any ads. If this ever changes, this policy will be updated.</p>
        <h4>Children's Privacy</h4>
        <p>Fantasy Critic is not intended to be used by anyone under the age of 13, in compliance with the Children's Online Privacy Protection Act.</p>
      </div>
      <hr />
      <h2>Technical Credits</h2>
      <div class="text-well">
        <h3>Software Packages</h3>
        <p>This site uses the following software packages heavily, and I wouldn't be able to create this site without them.</p>

        <ul>
          <li>ASP.NET Core</li>
          <li>Vue.JS</li>
          <li>MySQL</li>
          <li>MySQLConnector</li>
          <li>IdentityServer</li>
          <li>Bootstrap</li>
          <li>Vue-Bootstrap</li>
          <li>Webpack</li>
          <li>SignalR</li>
          <li>NodaTime</li>
          <li>Dapper</li>
          <li>Newtonsoft.JSON</li>
          <li>MoreLINQ</li>
          <li>CSharpFunctionalExtensions</li>
          <li>Patreon.Net</li>
          <li>RazorLight</li>
          <li>NLog</li>
          <li>NUnit</li>
          <li>CSVHelper</li>
          <li>Python Packages (sklearn, numpy, pandas)</li>
        </ul>

        <h3>Other Credits</h3>
        <ul>
          <li>
            <a href="https://wildermuth.com/" target="_blank">Shawn Wildermuth</a>
            , whose ASP.NET Core + Vue.JS tutorial gave me a great introduction to Vue.JS.
          </li>
          <li>
            <a href="https://github.com/TrilonIO/aspnetcore-Vue-starter" target="_blank">Mark Piesak</a>
            , for his excellent ASP.NET Core + Vue.JS template, which I used for a number of years.
          </li>
          <li>
            <a href="https://github.com/AndrewKralovec" target="_blank">Andrew Kralovec</a>
            , for additional front end development.
          </li>
          <li>
            <a href="https://github.com/uioreanu" target="_blank">Calin Uioreanu</a>
            , for helping develop the "Projected Points" algorithm.
          </li>
          <li>
            <a href="https://twitter.com/WhatsGoode1" target="_blank">Matt Goode</a>
            , for the draft notification sound.
          </li>
        </ul>
      </div>

      <template v-if="donors && donors.length > 0">
        <h2>Patreon Donors</h2>
        <div class="text-well">
          <p>The following people are supporting us at the "Fantasy Critic Donor" tier on Patreon, and we thank them greatly for that:</p>
          <ul>
            <li v-for="donor in donors" :key="donor">{{ donor }}</li>
          </ul>
        </div>
      </template>
      <hr />
    </div>

    <div class="col-md-12 col-lg-10 offset-md-0 offset-lg-1">
      <h2>A few statistics...</h2>
    </div>

    <siteCounts></siteCounts>
  </div>
</template>
<script>
import axios from 'axios';
import SiteCounts from '@/components/siteCounts.vue';

export default {
  components: {
    SiteCounts
  },
  data() {
    return {
      donors: [],
      error: null
    };
  },
  mounted() {
    this.fetchDonors();
  },
  methods: {
    fetchDonors() {
      axios
        .get('/api/general/donors')
        .then((response) => {
          this.donors = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    }
  }
};
</script>
<style>
.inside-joke {
  font-size: 0;
}
</style>
