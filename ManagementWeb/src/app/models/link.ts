import {Duration, Moment} from 'moment';

export interface Connection {
  id: string;
  lastActivity: Moment;
  assemblyVersion: string;
  protocolVersion: number;
  isStalled: boolean;
}

export interface Link {
  id: string;
  creationDate: Moment;
  userName: string;
  displayName: string;
  connectionCount: number;
  isDisabled: boolean;
}

export interface LinkCreate {
  userName: string;
  password: string;
}

export interface LinkDetail extends Link {
  connections: Connection[];
  allowLocalClientRequestsOnly: boolean;
  forwardOnPremiseTargetErrorResponse: boolean;
  tokenRefreshWindow?: Duration;
  heartbeatInterval?: Duration;
  reconnectMinWaitTime?: Duration;
  reconnectMaxWaitTime?: Duration;
  absoluteConnectionLifetime?: Duration;
  slidingConnectionLifetime?: Duration;
}
