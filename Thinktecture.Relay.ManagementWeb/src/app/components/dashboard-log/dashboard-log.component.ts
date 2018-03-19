import {Component, OnInit} from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {map} from 'rxjs/operators';
import {RequestLogEntry} from '../../models/request-log-entry';
import {DashboardComponent} from '../dashboard/dashboard.component';

@Component({
  selector: 'trs-dashboard-log',
  templateUrl: './dashboard-log.component.html',
  styleUrls: ['./dashboard-log.component.scss'],
})
export class DashboardLogComponent implements OnInit {
  logs$: Observable<RequestLogEntry[]>;

  constructor(private readonly _dashboard: DashboardComponent) {
  }

  ngOnInit(): void {
    this.logs$ = this._dashboard.info$.pipe(map(info => info.logs));
  }
}
