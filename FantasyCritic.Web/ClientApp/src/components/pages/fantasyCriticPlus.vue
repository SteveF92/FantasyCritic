<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <img class="plus-logo" src="/img/fc-plus-horizontal.svg" />
      <div class="text-well">
        <h2>What is Fantasy Critic Plus?</h2>
        <p>
          Fantasy Critic Plus is a set of premium features available to those who support us on
          <a href="https://patreon.com/fantasycritic" target="_blank">Patreon <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
          Here are the features currently included:
        </p>

        <div class="row">
          <div class="col-lg-12 col-xl-6">
            <h3>Publisher Icons</h3>
            <p>
              Plus users can chose an icon to represent their Publishers, both in normal leagues and in Critics Royale. Right now, the icon is limited to "one emoji", but we may add
              support for custom images in the future.
            </p>
            <h3>Share Publisher Option</h3>
            <p>Plus users can click a button to easily share their publishers (their lists of games) to social media.</p>
          </div>
          <div class="col-lg-12 col-xl-6">
            <img class="screenshot full-screenshot" src="/img/plus-publisher-example.png" />
            <img class="screenshot mobile-screenshot" src="/img/plus-publisher-example-mobile.png" />
          </div>
        </div>
        <hr />
        <div class="row">
          <div class="col-lg-12 col-xl-6">
            <h3>Advanced Searching Features</h3>
            <p>
              Plus users gain access to extra search tools on the "Master Games" page. More specifically, they will be able to see games that are available, or not available, in a given
              league that they are in.
            </p>
            <p>
              Players can use this to easily see all the stats for any game that is available, or see all the stats for the games that are taken, perhaps to make a decision about a counter pick.
            </p>
          </div>
          <div class="col-lg-12 col-xl-6">
            <img class="screenshot full-screenshot" src="/img/master-game-searching-example.png" />
            <img class="screenshot mobile-screenshot" src="/img/master-game-searching-example-mobile.png" />
          </div>
        </div>
        <hr />
        <div class="row">
          <div class="col-lg-12 col-xl-6">
            <h3>Extra League Views</h3>
            <p>
              In addition to being able to see "upcoming games" for a given league or across all their publishers(on the home page), plus users can also see "recent games" in the same fashion.
            </p>
            <h3>Extra Publisher Info</h3>
            <p>
              Plus users have access to extra features on the publisher details page. They can sort the games a publisher has by any field they like, and they can also see games that player has previously dropped.
              This even works for previous years, making it easy to go back and see how your draft fared.
            </p>
          </div>
          <div class="col-lg-12 col-xl-6">
            <img class="screenshot" src="/img/game-news-example.png" />
          </div>
        </div>
        <hr />
        <h2>Okay, that sounds good. What else?</h2>
        <p>
          The list of features will continue to grow over time, and we are interested in your input on what should be added. In general, features will be "quality of life" features.
          We'll never, for example, give players bonus points in their league just for being Fantasy Critic Plus users.
        </p>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
    };
  }
};
</script>
<style scoped>
  .plus-logo {
    margin-top: 10px;
    margin-bottom: 10px;
    background-color: white;
    border-radius: 20px;
    max-width: 1000px;
    margin-left: auto;
    margin-right: auto;
    display: block;
  }

  .screenshot {
    margin-left: auto;
    margin-right: auto;
    display: block;
    border-style: solid;
    border-width: 2px;
    border-color: white;
  }

  @media only screen and (max-width: 650px) {
    .full-screenshot {
      display: none;
    }
  }

  @media only screen and (min-width: 651px) {
    .mobile-screenshot {
      display: none;
    }
  }
</style>
