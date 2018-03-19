import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {Observable} from 'rxjs/Observable';
import {switchMap} from 'rxjs/operators';
import {RequestLogEntry} from '../../models/request-log-entry';
import {BackendService} from '../../services/backend.service';

@Component({
  selector: 'trs-link-logs',
  templateUrl: './link-log.component.html',
  styleUrls: ['./link-log.component.scss'],
})
export class LinkLogComponent implements OnInit {
  logs$: Observable<RequestLogEntry[]>;

  constructor(private readonly _backend: BackendService, private readonly _route: ActivatedRoute) {
  }

  ngOnInit() {
    this.logs$ = this._route.params.pipe(
      switchMap(params => this._backend.getLog(params.id)),
    );
  }
}
