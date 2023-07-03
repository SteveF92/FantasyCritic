let ggMixin = {
  methods: {
    getGGLinkForGame(masterGame) {
      if (!masterGame.ggToken) {
        return null;
      }

      let slug = masterGame.ggSlug;
      if (!slug) {
        slug = 'a';
      }
      return `https://ggapp.io/games/${masterGame.ggToken}/${slug}`;
    },
    getGGCoverArtLinkForGame(masterGame, width) {
      if (!masterGame.ggCoverArtFileName) {
        return null;
      }

      const key = `media/games/${masterGame.ggToken}/${masterGame.ggCoverArtFileName}`;
      let requestOptions = {
        bucket: 'ggapp',
        key: key,
        edits: {
          toFormat: 'jpg',
          jpeg: {
            quality: 80,
            chromaSubsampling: '4:4:4'
          },
          resize: {
            width: width,
            height: width * 1.5
          }
        }
      };

      const encodedOptions = window.btoa(JSON.stringify(requestOptions));
      return `https://d2d2z3qzqjizpf.cloudfront.net/${encodedOptions}`;
    }
  }
};

export default ggMixin;
