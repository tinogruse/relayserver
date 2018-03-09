import {Component, OnInit} from '@angular/core';
import {ChartOptions} from 'chart.js';
import {Observable} from 'rxjs/Observable';
import {map} from 'rxjs/operators';
import {RequestLogEntry} from '../../models/requestLogEntry';
import {BackendService} from '../../services/backend.service';

interface Info {
  logs: RequestLogEntry[];
  chartLabels: string[];
  chartIn: number[];
  chartOut: number[];
}

@Component({
  selector: 'trs-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  constructor(private readonly _backend: BackendService) {
  }

  info$: Observable<Info>;

  readonly chartOptions: ChartOptions = {
    scales: {
      yAxes: [{
        ticks: { callback: value => `${Math.round(value / 1024)} kB` },
      }],
    },
  };

  readonly logColumns = ['linkId', 'httpStatusCode', 'onPremiseTargetKey', 'localUrl', 'contentBytesIn', 'contentBytesOut',
    'onPremiseConnectorInDate', 'onPremiseConnectorOutDate', 'onPremiseTargetInDate', 'onPremiseTargetOutDate', 'originId'];

  ngOnInit() {
    this.info$ = this._backend.dashboardInfo().pipe(
      map(info => ({
        logs: info.logs,
        chartLabels: info.chart.map(chart => chart.key.format('YYYY-MM-DD')),
        chartIn: info.chart.map(chart => chart.in),
        chartOut: info.chart.map(chart => chart.out),
      })),
    );
  }
}
