import {Moment} from 'moment';

export interface RequestLogEntry {
  linkId: string;
  originId: string;
  httpStatusCode: number;
  onPremiseTargetKey: string;
  localUrl: string;
  contentBytesIn: number;
  contentBytesOut: number;
  onPremiseConnectorInDate: Moment;
  onPremiseConnectorOutDate: Moment;
  onPremiseTargetInDate?: Moment;
  onPremiseTargetOutDate?: Moment;
}
