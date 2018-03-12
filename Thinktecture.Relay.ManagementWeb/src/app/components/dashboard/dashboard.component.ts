import {DecimalPipe} from '@angular/common';
import {Component, Inject, LOCALE_ID, OnInit} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {ChartOptions} from 'chart.js';
import {Observable} from 'rxjs/Observable';
import {map} from 'rxjs/operators';
import {RequestLogEntry} from '../../models/requestLogEntry';
import {BackendService} from '../../services/backend.service';

interface Data {
  data: number[];
  label: string;
}

interface Info {
  logs: RequestLogEntry[];
  chartLabels: string[];
  chartData: Data[];
}

@Component({
  selector: 'trs-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  private readonly _number: DecimalPipe;

  constructor(private readonly _backend: BackendService, @Inject(LOCALE_ID) locale: string, private readonly _translate: TranslateService) {
    this._number = new DecimalPipe(locale);
  }

  info$: Observable<Info>;

  readonly chartOptions: ChartOptions = {
    scales: {
      yAxes: [
        {
          ticks: {
            callback: value => {
              const suffixes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];
              let factor = suffixes.findIndex((suffix, index) => value < Math.pow(1024, index));

              if (factor-- === 0) {
                return `${this._number.transform(value)} B`;
              }

              return `${this._number.transform(value / Math.pow(1024, factor), '0.1-1')} ${suffixes[factor]}`;
            },
          },
        },
      ],
    },
  };

  readonly logColumns = ['linkId', 'httpStatusCode', 'onPremiseTargetKey', 'localUrl', 'contentBytesIn', 'contentBytesOut',
    'onPremiseConnectorInDate', 'onPremiseTargetInDate', 'onPremiseTargetOutDate', 'onPremiseConnectorOutDate', 'originId'];

  ngOnInit() {
    this.info$ = this._backend.dashboardInfo().pipe(
      map(info => ({
        logs: info.logs,
        chartLabels: info.chart.map(chart => chart.key.format('L')),
        chartData: [
          { data: info.chart.map(chart => chart.in), label: this._translate.instant('Content bytes in') },
          { data: info.chart.map(chart => chart.out), label: this._translate.instant('Content bytes out') },
        ],
      })),
    );
  }
}
