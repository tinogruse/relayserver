import {Moment} from 'moment';

export interface User {
  id: string;
  userName: string;
  lockedUntil?: Moment;
  password?: string;
  currentPassword?: string;
  verifyPassword?: string;
}
