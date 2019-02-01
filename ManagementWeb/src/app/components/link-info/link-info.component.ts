import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import * as moment from 'moment';
import {Duration} from 'moment';
import {Observable} from 'rxjs';
import {switchMap, tap} from 'rxjs/operators';
import {LinkDetail} from '../../models/link';
import {BackendService} from '../../services/backend.service';

@Component({
  selector: 'trs-link-info',
  templateUrl: './link-info.component.html',
  styleUrls: ['./link-info.component.scss'],
})
export class LinkInfoComponent implements OnInit {
  link$: Observable<LinkDetail>;

  tokenRefreshWindow: number | null = null;
  heartbeatInterval: number | null = null;
  reconnectMinWaitTime: number | null = null;
  reconnectMaxWaitTime: number | null = null;
  absoluteConnectionLifetime: number | null = null;
  slidingConnectionLifetime: number | null = null;

  readonly tableColumns = ['id', 'lastActivity', 'protocolVersion', 'assemblyVersion', 'isStalled'];

  constructor(private readonly backend: BackendService, private readonly route: ActivatedRoute) {
  }

  ngOnInit() {
    this.link$ = this.route.params.pipe(
      switchMap(params => this.backend.getLink(params.id)),
      tap(link => {
        this.tokenRefreshWindow = link.tokenRefreshWindow ? link.tokenRefreshWindow.asSeconds() : null;
        this.heartbeatInterval = link.heartbeatInterval ? link.heartbeatInterval.asSeconds() : null;
        this.reconnectMinWaitTime = link.reconnectMaxWaitTime ? link.reconnectMaxWaitTime.asSeconds() : null;
        this.reconnectMinWaitTime = link.reconnectMinWaitTime ? link.reconnectMinWaitTime.asSeconds() : null;
        this.absoluteConnectionLifetime = link.absoluteConnectionLifetime ? link.absoluteConnectionLifetime.asMinutes() : null;
        this.slidingConnectionLifetime = link.slidingConnectionLifetime ? link.slidingConnectionLifetime.asMinutes() : null;
      }),
    );
  }

  formatDuration(value): string {
    return moment.duration(value).toISOString();
  }

  parseDuration(value): Duration | undefined {
    return undefined;
  }

  // noinspection JSUnusedLocalSymbols
  updateLink(link: LinkDetail, assignment?: any) {
    link.tokenRefreshWindow = this.tokenRefreshWindow === null ? null : moment.duration(this.tokenRefreshWindow, 'second');
    link.heartbeatInterval = this.heartbeatInterval === null ? null : moment.duration(this.heartbeatInterval, 'second');
    link.reconnectMinWaitTime = this.reconnectMinWaitTime === null ? null : moment.duration(this.reconnectMinWaitTime, 'second');
    link.reconnectMaxWaitTime = this.reconnectMaxWaitTime === null ? null : moment.duration(this.reconnectMaxWaitTime, 'second');
    link.absoluteConnectionLifetime = this.absoluteConnectionLifetime === null ? null : moment.duration(this.absoluteConnectionLifetime, 'minute'); // tslint:disable-line:max-line-length
    link.slidingConnectionLifetime = this.slidingConnectionLifetime === null ? null : moment.duration(this.slidingConnectionLifetime, 'minute'); // tslint:disable-line:max-line-length
    console.log(link);
  }
}
