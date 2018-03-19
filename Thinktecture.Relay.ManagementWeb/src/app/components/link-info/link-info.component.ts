import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {Observable} from 'rxjs/Observable';
import {switchMap} from 'rxjs/operators';
import {Link} from '../../models/link';
import {BackendService} from '../../services/backend.service';

@Component({
  selector: 'trs-link-info',
  templateUrl: './link-info.component.html',
  styleUrls: ['./link-info.component.scss'],
})
export class LinkInfoComponent implements OnInit {
  link$: Observable<Link>;

  constructor(private readonly _backend: BackendService, private readonly _route: ActivatedRoute) {
  }

  ngOnInit() {
    this.link$ = this._route.params.pipe(
      switchMap(params => this._backend.getLink(params.id)),
    );
  }
}
