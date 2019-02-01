import {Component, OnInit, TemplateRef, ViewChild} from '@angular/core';
import {MatDialog, PageEvent, Sort} from '@angular/material';
import {BehaviorSubject, combineLatest, defer, Observable, of, throwError} from 'rxjs';
import {filter, finalize, retry, switchMap, switchMapTo, tap} from 'rxjs/operators';
import {Link} from '../../models/link';
import {PageRequest} from '../../models/page-request';
import {PageResult} from '../../models/page-result';
import {BackendService} from '../../services/backend.service';
import {SimpleDialogComponent} from '../simple-dialog/simple-dialog.component';

@Component({
  selector: 'trs-links',
  templateUrl: './links.component.html',
  styleUrls: ['./links.component.scss'],
})
export class LinksComponent implements OnInit {
  private readonly refresh$ = new BehaviorSubject<boolean>(true);
  private readonly filter$ = new BehaviorSubject<string>('');
  private readonly sort$ = new BehaviorSubject<Sort>({ active: 'displayName', direction: 'asc' });
  private readonly page$ = new BehaviorSubject<PageEvent>({ pageIndex: 0, pageSize: 10, length: 0 });

  constructor(private readonly backend: BackendService, private readonly matDialog: MatDialog) {
  }

  result$: Observable<PageResult<Link>>;
  submitting = 0;

  @ViewChild('create') create: TemplateRef<any>;
  @ViewChild('delete') delete: TemplateRef<any>;
  @ViewChild('credentials') credentials: TemplateRef<any>;

  readonly pageRequest: PageRequest = { pageIndex: 0, pageSize: 10 };
  readonly tableColumns = ['displayName', 'userName', 'creationDate', 'isConnected', 'options'];
  readonly pageSizes = [10, 20, 50, 100];

  ngOnInit() {
    this.result$ = combineLatest(
      this.refresh$,
      this.filter$.pipe(
        tap(text => this.pageRequest.searchText = text),
      ),
      this.sort$.pipe(
        tap(sort => this.pageRequest.sortField = sort.active),
        tap(sort => this.pageRequest.sortDirection = sort.direction),
      ),
      this.page$.pipe(
        tap(page => this.pageRequest.pageIndex = page.pageIndex),
        tap(page => this.pageRequest.pageSize = page.pageSize),
      ),
    ).pipe(
      tap(() => this.submitting++),
      switchMap(() => this.backend.getLinks(this.pageRequest).pipe(finalize(() => this.submitting--))),
    );
  }

  applyFilter(text: string) {
    this.pageRequest.pageIndex = 0;
    this.filter$.next(text);
  }

  changeSort(sort: Sort) {
    if (!sort.direction) {
      sort.active = '';
    }

    this.pageRequest.pageIndex = 0;
    this.sort$.next(sort);
  }

  changePage(page: PageEvent) {
    if (this.pageRequest.pageSize !== page.pageSize) {
      page.pageIndex = 0;
    }

    this.page$.next(page);
  }

  private openDialog(templateRef: TemplateRef<any>, link: Link): Observable<Link> {
    return this.matDialog.open(templateRef, { data: { link }, disableClose: true }).beforeClose().pipe(
      filter(result => !!result),
      tap(() => this.submitting++),
    );
  }

  private openErrorDialog(title: string, errors: string[]): Observable<never> {
    return this.matDialog.open(SimpleDialogComponent, { data: { title, contents: errors } }).afterClosed().pipe(
      switchMapTo(throwError('retry')),
    );
  }

  updateUserName(link: Link) {
    link.userName = link.displayName ? link.displayName.replace(/\s/g, '-') : '';
  }

  copyPassword(input: HTMLInputElement) {
    input.select();
    document.execCommand('copy');
  }

  openCreate() {
    const link = {} as Link;
    defer(() => this.openDialog(this.create, link)).pipe(
      switchMap(() => this.backend.createLink(link).pipe(finalize(() => this.submitting--))),
      switchMap(result => result.errors ? this.openErrorDialog('Creating the link failed', result.errors) : of(result)),
      retry(),
      tap(result => this.matDialog.open(this.credentials, { data: { link: result } })),
    ).subscribe(() => this.refresh$.next(true));
  }

  openDelete(link: Link) {
    this.openDialog(this.delete, link).pipe(
      switchMap(() => this.backend.deleteLink(link).pipe(finalize(() => this.submitting--))),
      switchMap(result => result.errors ? this.openErrorDialog('Deleting the link failed', result.errors) : of(result)),
    ).subscribe(() => this.refresh$.next(true));
  }
}
