interface GGGame {
  ggToken?: string | null;
  ggSlug?: string | null;
  ggCoverArtFileName?: string | null;
}

export function getGGLinkForGame(masterGame: GGGame): string | null {
  if (!masterGame.ggToken) {
    return null;
  }

  let slug = masterGame.ggSlug;
  if (!slug) {
    slug = 'a';
  }
  return `https://ggapp.io/games/${masterGame.ggToken}/${slug}`;
}

export function getGGCoverArtLinkForGame(masterGame: GGGame, width: number): string | null {
  if (!masterGame.ggCoverArtFileName) {
    return null;
  }

  const key = `media/games/${masterGame.ggToken}/${masterGame.ggCoverArtFileName}`;
  const requestOptions = {
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
