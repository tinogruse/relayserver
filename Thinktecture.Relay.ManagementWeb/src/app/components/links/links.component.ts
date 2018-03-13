import {Component, OnInit, TemplateRef, ViewChild} from '@angular/core';
import {MatDialog, PageEvent, Sort} from '@angular/material';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';
import {Observable} from 'rxjs/Observable';
import {combineLatest} from 'rxjs/observable/combineLatest';
import {defer} from 'rxjs/observable/defer';
import {filter, finalize, retryWhen, switchMap, tap} from 'rxjs/operators';
import {Link} from '../../models/link';
import {PageRequest} from '../../models/pageRequest';
import {PageResult} from '../../models/pageResult';
import {BackendService} from '../../services/backend.service';
import {SimpleDialogComponent} from '../dialogs/simple-dialog/simple-dialog.component';

@Component({
  selector: 'trs-links',
  templateUrl: './links.component.html',
  styleUrls: ['./links.component.scss'],
})
export class LinksComponent implements OnInit {
  private readonly _refresh$ = new BehaviorSubject<boolean>(true);
  private readonly _filter$ = new BehaviorSubject<string>('');
  private readonly _sort$ = new BehaviorSubject<Sort>({ active: 'symbolicName', direction: 'asc' });
  private readonly _page$ = new BehaviorSubject<PageEvent>({ pageIndex: 0, pageSize: 10, length: 0 });

  constructor(private readonly _backend: BackendService, private readonly _matDialog: MatDialog) {
  }

  result$: Observable<PageResult<Link>>;
  submitting = 0;

  @ViewChild('create') create: TemplateRef<any>;
  @ViewChild('delete') delete: TemplateRef<any>;

  readonly pageRequest: PageRequest = {
    pageIndex: 0,
    pageSize: 10,
  };

  readonly tableColumns = ['symbolicName', 'userName', 'creationDate', 'isConnected', 'options'];
  readonly pageSizes = [10, 20, 50, 100];

  ngOnInit() {
    this.result$ = combineLatest(
      this._refresh$,
      this._filter$.pipe(
        tap(text => this.pageRequest.searchText = text),
      ),
      this._sort$.pipe(
        tap(sort => this.pageRequest.sortField = sort.active),
        tap(sort => this.pageRequest.sortDirection = sort.direction),
      ),
      this._page$.pipe(
        tap(page => this.pageRequest.pageIndex = page.pageIndex),
        tap(page => this.pageRequest.pageSize = page.pageSize),
      ),
    ).pipe(
      tap(() => this.submitting++),
      switchMap(() => this._backend.getLinks(this.pageRequest).pipe(finalize(() => this.submitting--))),
    );
  }

  applyFilter(text: string) {
    this.pageRequest.pageIndex = 0;
    this._filter$.next(text);
  }

  changeSort(sort: Sort) {
    if (!sort.direction) {
      sort.active = '';
    }

    this.pageRequest.pageIndex = 0;
    this._sort$.next(sort);
  }

  changePage(page: PageEvent) {
    if (this.pageRequest.pageSize !== page.pageSize) {
      page.pageIndex = 0;
    }

    this._page$.next(page);
  }

  private _openDialog(templateRef: TemplateRef<any>, link: Link): Observable<Link> {
    return this._matDialog.open(templateRef, { data: { link }, disableClose: true }).beforeClose().pipe(
      filter(result => !!result),
      tap(() => this.submitting++),
    );
  }

  updateUserName(link: Link, value: string) {
    link.userName = value ? value.replace(/\s/g, '-') : '';
  }

  openCreate() {
    const link = {} as Link;
    defer(() => this._openDialog(this.create, link)).pipe(
      switchMap(() => this._backend.createLink(link).pipe(finalize(() => this.submitting--))),
      retryWhen(result => result.pipe(
        switchMap(error => this._matDialog.open(SimpleDialogComponent, {
          data: {
            title: 'Creating the link failed',
            contents: error.error.message.split('\n'),
          },
        }).afterClosed()),
      )),
    ).subscribe(() => this._refresh$.next(true));
  }

  openDelete(link: Link) {
    this._openDialog(this.delete, link).pipe(
      switchMap(() => this._backend.deleteLink(link).pipe(finalize(() => this.submitting--))),
      tap({ error: () => this._matDialog.open(SimpleDialogComponent, { data: { title: 'Deleting the link failed' } }) }),
    ).subscribe(() => this._refresh$.next(true));
  }
}
