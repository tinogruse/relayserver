import {ContentBytesChartDataItem} from './contentBytesChartDataItem';
import {RequestLogEntry} from './requestLogEntry';

export interface Dashboard {
  logs: RequestLogEntry[];
  chart: ContentBytesChartDataItem[];
}
