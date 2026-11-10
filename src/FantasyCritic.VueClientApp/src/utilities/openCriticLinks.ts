interface OpenCriticGame {
  openCriticID?: number | null;
  openCriticSlug?: string | null;
}

export function openCriticLink(game: OpenCriticGame): string | undefined {
  if (!game.openCriticID) {
    return undefined;
  }
  return `https://opencritic.com/game/${game.openCriticID}/${game.openCriticSlug ?? 'b'}`;
}
