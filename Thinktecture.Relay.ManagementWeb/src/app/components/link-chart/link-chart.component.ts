import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {Observable} from 'rxjs/Observable';
import {switchMap} from 'rxjs/operators';
import {ContentBytesChartDataItem} from '../../models/content-bytes-chart-data-item';
import {BackendService} from '../../services/backend.service';

@Component({
  selector: 'trs-link-chart',
  templateUrl: './link-chart.component.html',
  styleUrls: ['./link-chart.component.scss'],
})
export class LinkChartComponent implements OnInit {
  chart$: Observable<ContentBytesChartDataItem[]>;

  constructor(private readonly _backend: BackendService, private readonly _route: ActivatedRoute) {
  }

  ngOnInit() {
    this.chart$ = this._route.params.pipe(
      switchMap(params => this._backend.getChart(params.id)),
    );
  }
}
