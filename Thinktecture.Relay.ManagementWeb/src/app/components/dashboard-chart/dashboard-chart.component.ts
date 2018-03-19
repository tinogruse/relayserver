import {Component, OnInit} from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {map} from 'rxjs/operators';
import {ContentBytesChartDataItem} from '../../models/content-bytes-chart-data-item';
import {DashboardComponent} from '../dashboard/dashboard.component';

@Component({
  selector: 'trs-dashboard-chart',
  templateUrl: './dashboard-chart.component.html',
  styleUrls: ['./dashboard-chart.component.scss'],
})
export class DashboardChartComponent implements OnInit {
  chart$: Observable<ContentBytesChartDataItem[]>;

  constructor(private readonly _dashboard: DashboardComponent) {
  }

  ngOnInit(): void {
    this.chart$ = this._dashboard.info$.pipe(map(info => info.chart));
  }
}
