import {Moment} from 'moment';

export interface Connection {
  connectionId: string;
  lastActivity: Moment;
  assemblyVersion: string;
  connectorVersion: number;
}

export interface Link {
  id: string;
  userName: string;
  password: string;
  creationDate: Moment;
  isConnected: boolean;
  connections: Connection[];
  isDisabled: boolean;
  symbolicName: string;
  allowLocalClientRequestsOnly: boolean;
  forwardOnPremiseTargetErrorResponse: boolean;
}
