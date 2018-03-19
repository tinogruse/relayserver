import {DecimalPipe} from '@angular/common';
import {Component, Inject, Input, LOCALE_ID} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {ChartOptions} from 'chart.js';
import {ContentBytesChartDataItem} from '../../models/content-bytes-chart-data-item';

interface Data {
  data: number[];
  label: string;
}

@Component({
  selector: 'trs-chart',
  templateUrl: './chart.component.html',
  styleUrls: ['./chart.component.scss'],
})
export class ChartComponent {
  private readonly _number: DecimalPipe;

  readonly options: ChartOptions = {
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

  labels: string[];
  data: Data[];

  @Input() set contentBytes(value: ContentBytesChartDataItem[]) {
    if (!value) {
      this.labels = [];
      this.data = [];
      return;
    }

    this.labels = value.map(item => item.key.format('L'));
    this.data = [
      { data: value.map(item => item.in), label: this._translate.instant('Content bytes in') },
      { data: value.map(item => item.out), label: this._translate.instant('Content bytes out') },
    ];
  }

  constructor(@Inject(LOCALE_ID) locale: string, private readonly _translate: TranslateService) {
    this._number = new DecimalPipe(locale);
  }
}
