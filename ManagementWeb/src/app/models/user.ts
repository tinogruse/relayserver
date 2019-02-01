import {Moment} from 'moment';

export interface User {
  id: string;
  userName: string;
  creationDate: Moment;
  lockedUntil?: Moment;
  password?: string;
  currentPassword?: string;
  verifyPassword?: string;
}
