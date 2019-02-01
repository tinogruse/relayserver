import {AfterViewInit, Component, OnInit, QueryList, TemplateRef, ViewChild, ViewChildren} from '@angular/core';
import {MatDialog, MatSort, MatTableDataSource} from '@angular/material';
import {BehaviorSubject, defer, Observable} from 'rxjs';
import {filter, finalize, retryWhen, switchMap, tap} from 'rxjs/operators';
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
    return this.matDialog.open(templateRef, { data: { user }, disableClose: true }).beforeClose().pipe(
      filter(result => !!result),
      tap(() => this.submitting++),
    );
  }

  openCreate() {
    const user = {} as User;
    defer(() => this.openDialog(this.create, user)).pipe(
      switchMap(() => this.backend.createUser(user).pipe(finalize(() => this.submitting--))),
      retryWhen(result => result.pipe(
        switchMap(error => this.matDialog.open(SimpleDialogComponent, {
          data: {
            title: 'Creating the user failed',
            contents: error.error.message.split('\n'),
          },
        }).afterClosed()),
      )),
    ).subscribe(() => this.refresh$.next(true));
  }

  openEdit(user: User) {
    user = JSON.parse(JSON.stringify(user));
    defer(() => this.openDialog(this.edit, user)).pipe(
      switchMap(() => this.backend.updateUser(user).pipe(finalize(() => this.submitting--))),
      retryWhen(result => result.pipe(
        switchMap(error => this.matDialog.open(SimpleDialogComponent, {
          data: {
            title: 'Updating the user failed',
            contents: error.error.message.split('\n'),
          },
        }).afterClosed()),
      )),
    ).subscribe(() => this.refresh$.next(true));
  }

  openDelete(user: User) {
    this.openDialog(this.delete, user).pipe(
      switchMap(() => this.backend.deleteUser(user).pipe(finalize(() => this.submitting--))),
      tap({ error: () => this.matDialog.open(SimpleDialogComponent, { data: { title: 'Deleting the user failed' } }) }),
    ).subscribe(() => this.refresh$.next(true));
  }
}
