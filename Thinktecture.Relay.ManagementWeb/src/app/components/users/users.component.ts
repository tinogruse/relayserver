import {AfterViewInit, Component, OnInit, QueryList, TemplateRef, ViewChild, ViewChildren} from '@angular/core';
import {MatDialog, MatSort, MatTableDataSource} from '@angular/material';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';
import {Observable} from 'rxjs/Observable';
import {defer} from 'rxjs/observable/defer';
import {filter, finalize, retryWhen, switchMap, tap} from 'rxjs/operators';
import {Link} from '../../models/link';
import {User} from '../../models/user';
import {BackendService} from '../../services/backend.service';
import {SecurityService} from '../../services/security.service';
import {SimpleDialogComponent} from '../dialogs/simple-dialog/simple-dialog.component';

@Component({
  selector: 'trs-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
})
export class UsersComponent implements OnInit, AfterViewInit {
  private readonly _refresh$ = new BehaviorSubject<boolean>(true);

  constructor(
    private readonly _backend: BackendService,
    private readonly _security: SecurityService,
    private readonly _matDialog: MatDialog,
  ) {
  }

  users$: Observable<User[]>;
  submitting = 0;

  @ViewChildren(MatSort) sort: QueryList<MatSort>;
  @ViewChild('create') create: TemplateRef<any>;
  @ViewChild('edit') edit: TemplateRef<any>;
  @ViewChild('delete') delete: TemplateRef<any>;

  get userName(): string {
    return this._security.userName !;
  }

  readonly dataSource = new MatTableDataSource<User>();
  readonly tableColumns = ['userName', 'lockedUntil', 'options'];

  ngOnInit() {
    this.users$ = this._refresh$.pipe(
      tap(() => this.submitting++),
      switchMap(() => this._backend.getUsers().pipe(finalize(() => this.submitting--))),
      tap(users => this.dataSource.data = users),
    );
  }

  ngAfterViewInit() {
    this.sort.changes.subscribe(() => this.dataSource.sort = this.sort.first);
  }

  private _openDialog(templateRef: TemplateRef<any>, user: User): Observable<User> {
    return this._matDialog.open(templateRef, { data: { user }, disableClose: true }).beforeClose().pipe(
      filter(result => !!result),
      tap(() => this.submitting++),
    );
  }

  openCreate() {
    const user = {} as User;
    defer(() => this._openDialog(this.create, user)).pipe(
      switchMap(() => this._backend.createUser(user).pipe(finalize(() => this.submitting--))),
      retryWhen(result => result.pipe(
        switchMap(error => this._matDialog.open(SimpleDialogComponent, {
          data: {
            title: 'Creating the user failed',
            contents: error.error.message.split('\n'),
          },
        }).afterClosed()),
      )),
    ).subscribe(() => this._refresh$.next(true));
  }

  openEdit(user: User) {
    user = JSON.parse(JSON.stringify(user));
    defer(() => this._openDialog(this.edit, user)).pipe(
      switchMap(() => this._backend.updateUser(user).pipe(finalize(() => this.submitting--))),
      retryWhen(result => result.pipe(
        switchMap(error => this._matDialog.open(SimpleDialogComponent, {
          data: {
            title: 'Updating the user failed',
            contents: error.error.message.split('\n'),
          },
        }).afterClosed()),
      )),
    ).subscribe(() => this._refresh$.next(true));
  }

  openDelete(user: User) {
    this._openDialog(this.delete, user).pipe(
      switchMap(() => this._backend.deleteUser(user).pipe(finalize(() => this.submitting--))),
      tap({ error: () => this._matDialog.open(SimpleDialogComponent, { data: { title: 'Deleting the user failed' } }) }),
    ).subscribe(() => this._refresh$.next(true));
  }
}
