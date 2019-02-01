import * as moment from 'moment';
import {Moment} from 'moment';

export function processRequestLogEntry(log: RequestLogEntry) {
  log.onPremiseConnectorInDate = moment(log.onPremiseConnectorInDate);
  log.onPremiseConnectorOutDate = moment(log.onPremiseConnectorOutDate);

  if (log.onPremiseTargetInDate) {
    log.onPremiseTargetInDate = moment(log.onPremiseTargetInDate);
  }
  if (log.onPremiseTargetOutDate) {
    log.onPremiseTargetOutDate = moment(log.onPremiseTargetOutDate);
  }
}

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
