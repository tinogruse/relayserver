import {AfterViewInit, Component, OnDestroy, OnInit, TemplateRef, ViewChild} from '@angular/core';
import {MatDialog, MatSort, MatTableDataSource} from '@angular/material';
import {Observable} from 'rxjs/Observable';
import {defer} from 'rxjs/observable/defer';
import {filter, finalize, retryWhen, switchMap, tap} from 'rxjs/operators';
import {Subscription} from 'rxjs/Subscription';
import {User} from '../../models/user';
import {BackendService} from '../../services/backend.service';
import {SecurityService} from '../../services/security.service';
import {SimpleDialogComponent} from '../dialogs/simple-dialog/simple-dialog.component';

@Component({
  selector: 'trs-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
})
export class UsersComponent implements OnInit, AfterViewInit, OnDestroy {
  private _subscription = Subscription.EMPTY;

  constructor(
    private readonly _backend: BackendService,
    private readonly _security: SecurityService,
    private readonly _matDialog: MatDialog,
  ) {
  }

  @ViewChild(MatSort) sort: MatSort;
  @ViewChild('add') add: TemplateRef<any>;
  @ViewChild('edit') edit: TemplateRef<any>;
  @ViewChild('delete') delete: TemplateRef<any>;

  submitting = false;

  get blocked(): boolean {
    return this.submitting || !this.dataSource.data.length;
  }

  get userName(): string {
    return this._security.userName !;
  }

  readonly dataSource = new MatTableDataSource<User>();
  readonly userColumns = ['userName', 'lockedUntil', 'options'];

  private _loadUsers() {
    this.dataSource.data = [];
    this._subscription.unsubscribe();
    this._subscription = this._backend.getUsers().subscribe(users => this.dataSource.data = users);
  }

  ngOnInit() {
    this._loadUsers();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnDestroy() {
    this._subscription.unsubscribe();
  }

  private _openDialog(templateRef: TemplateRef<any>, user: User): Observable<User> {
    return this._matDialog.open(templateRef, { data: { user }, disableClose: true }).beforeClose().pipe(
      filter(result => !!result),
      tap(() => this.submitting = true),
    );
  }

  openAdd() {
    const user = {} as User;
    defer(() => this._openDialog(this.add, user)).pipe(
      switchMap(() => this._backend.addUser(user).pipe(finalize(() => this.submitting = false))),
      retryWhen(result => result.pipe(
        switchMap(error => this._matDialog.open(SimpleDialogComponent, {
          data: {
            title: 'Adding the user failed',
            contents: error.error.message.split('\n'),
          },
        }).afterClosed()),
      )),
    ).subscribe(() => this._loadUsers());
  }

  openEdit(user: User) {
    user = JSON.parse(JSON.stringify(user));
    defer(() => this._openDialog(this.edit, user)).pipe(
      switchMap(() => this._backend.updateUser(user).pipe(finalize(() => this.submitting = false))),
      retryWhen(result => result.pipe(
        switchMap(error => this._matDialog.open(SimpleDialogComponent, {
          data: {
            title: 'Updating the user failed',
            contents: error.error.message.split('\n'),
          },
        }).afterClosed()),
      )),
    ).subscribe(() => this._loadUsers());
  }

  openDelete(user: User) {
    this._openDialog(this.delete, user).pipe(
      switchMap(() => this._backend.deleteUser(user).pipe(finalize(() => this.submitting = false))),
      tap({ error: () => this._matDialog.open(SimpleDialogComponent, { data: { title: 'Deleting the user failed' } }) }),
    ).subscribe(() => this._loadUsers());
  }
}
