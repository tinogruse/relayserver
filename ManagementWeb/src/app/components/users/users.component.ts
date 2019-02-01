import {AfterViewInit, Component, OnInit, QueryList, TemplateRef, ViewChild, ViewChildren} from '@angular/core';
import {MatDialog, MatSort, MatTableDataSource} from '@angular/material';
import {BehaviorSubject, defer, Observable, of, throwError} from 'rxjs';
import {filter, finalize, retry, switchMap, switchMapTo, tap} from 'rxjs/operators';
import {User} from '../../models/user';
import {BackendService} from '../../services/backend.service';
import {SecurityService} from '../../services/security.service';
import {SimpleDialogComponent} from '../simple-dialog/simple-dialog.component';

@Component({
  selector: 'trs-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
})
export class UsersComponent implements OnInit, AfterViewInit {
  private readonly refresh$ = new BehaviorSubject<boolean>(true);

  constructor(private readonly backend: BackendService, private readonly security: SecurityService, private readonly matDialog: MatDialog) {
  }

  users$: Observable<User[]>;
  submitting = 0;

  @ViewChildren(MatSort) sort: QueryList<MatSort>;
  @ViewChild('create') create: TemplateRef<any>;
  @ViewChild('edit') edit: TemplateRef<any>;
  @ViewChild('delete') delete: TemplateRef<any>;

  get userName(): string {
    return this.security.userName !;
  }

  readonly dataSource = new MatTableDataSource<User>();
  readonly tableColumns = ['userName', 'lockedUntil', 'options'];

  ngOnInit() {
    this.users$ = this.refresh$.pipe(
      tap(() => this.submitting++),
      switchMap(() => this.backend.getUsers().pipe(finalize(() => this.submitting--))),
      tap(users => this.dataSource.data = users),
    );
  }

  ngAfterViewInit() {
    this.sort.changes.subscribe(() => this.dataSource.sort = this.sort.first);
  }

  applyFilter(value: string) {
    this.dataSource.filter = value;
  }

  private openDialog(templateRef: TemplateRef<any>, user: User): Observable<User> {
    return this.matDialog.open(templateRef, { data: { user }, disableClose: true }).beforeClosed().pipe(
      filter(result => !!result),
      tap(() => this.submitting++),
    );
  }

  private openErrorDialog(title: string, errors: string[]): Observable<never> {
    return this.matDialog.open(SimpleDialogComponent, { data: { title, contents: errors } }).afterClosed().pipe(
      switchMapTo(throwError('retry')),
    );
  }

  openCreate() {
    const user = {} as User;
    defer(() => this.openDialog(this.create, user)).pipe(
      switchMap(() => this.backend.createUser(user).pipe(finalize(() => this.submitting--))),
      switchMap(result => result.errors ? this.openErrorDialog('Creating the user failed', result.errors) : of(result)),
      retry(),
    ).subscribe(() => this.refresh$.next(true));
  }

  openEdit(user: User) {
    user = JSON.parse(JSON.stringify(user));
    defer(() => this.openDialog(this.edit, user)).pipe(
      switchMap(() => this.backend.updateUser(user).pipe(finalize(() => this.submitting--))),
      switchMap(result => result.errors ? this.openErrorDialog('Updating the user failed', result.errors) : of(result)),
      retry(),
    ).subscribe(() => this.refresh$.next(true));
  }

  openDelete(user: User) {
    this.openDialog(this.delete, user).pipe(
      switchMap(() => this.backend.deleteUser(user).pipe(finalize(() => this.submitting--))),
      switchMap(result => result.errors ? this.openErrorDialog('Deleting the user failed', result.errors) : of(result)),
    ).subscribe(() => this.refresh$.next(true));
  }
}
