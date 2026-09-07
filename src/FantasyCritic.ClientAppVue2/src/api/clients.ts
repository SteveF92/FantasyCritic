import {
  AccountClient,
  ActionRunnerClient,
  AdminClient,
  CombinedDataClient,
  ConferenceClient,
  FactCheckerClient,
  GameClient,
  GeneralClient,
  LeagueClient,
  LeagueManagerClient,
  RoyaleClient,
  RoyaleGroupClient
} from './generated/FantasyCriticClients';

export { ApiException } from './generated/FantasyCriticClients';
export type * from './generated/FantasyCriticClients';

const http = {
  fetch(url: RequestInfo, init?: RequestInit): Promise<Response> {
    return window.fetch(url, {
      credentials: 'include',
      ...init
    });
  }
};

function createClient<T>(Client: new (baseUrl?: string, http?: typeof http) => T): T {
  return new Client('', http);
}

export const accountClient = createClient(AccountClient);
export const actionRunnerClient = createClient(ActionRunnerClient);
export const adminClient = createClient(AdminClient);
export const combinedDataClient = createClient(CombinedDataClient);
export const conferenceClient = createClient(ConferenceClient);
export const factCheckerClient = createClient(FactCheckerClient);
export const gameClient = createClient(GameClient);
export const generalClient = createClient(GeneralClient);
export const leagueClient = createClient(LeagueClient);
export const leagueManagerClient = createClient(LeagueManagerClient);
export const royaleClient = createClient(RoyaleClient);
export const royaleGroupClient = createClient(RoyaleGroupClient);
