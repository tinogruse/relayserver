import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {Observable} from 'rxjs';
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

  constructor(private readonly backend: BackendService, private readonly route: ActivatedRoute) {
  }

  ngOnInit() {
    this.logs$ = this.route.params.pipe(
      switchMap(params => this.backend.getLog(params.id)),
    );
  }
}
