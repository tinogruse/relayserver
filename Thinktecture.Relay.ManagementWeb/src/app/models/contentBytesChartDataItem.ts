import {Moment} from 'moment';

export interface ContentBytesChartDataItem {
  key: Moment;
  in: number;
  out: number;
}
