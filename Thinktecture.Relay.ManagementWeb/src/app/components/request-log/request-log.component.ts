import {Component, Input} from '@angular/core';
import {RequestLogEntry} from '../../models/request-log-entry';

@Component({
  selector: 'trs-request-log',
  templateUrl: './request-log.component.html',
  styleUrls: ['./request-log.component.scss'],
})
export class RequestLogComponent {
  readonly logColumns = ['linkId', 'httpStatusCode', 'onPremiseTargetKey', 'localUrl', 'contentBytesIn', 'contentBytesOut',
    'onPremiseConnectorInDate', 'onPremiseTargetInDate', 'onPremiseTargetOutDate', 'onPremiseConnectorOutDate', 'originId'];

  @Input() logs: RequestLogEntry[];

  @Input() set hideLinkId(value: boolean) {
    if (value && this.logColumns[0] === 'linkId') {
      this.logColumns.shift();
    }
  }
}
