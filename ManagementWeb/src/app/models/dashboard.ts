import {ContentBytesChartDataItem} from './content-bytes-chart-data-item';
import {RequestLogEntry} from './request-log-entry';

export interface Dashboard {
  logs: RequestLogEntry[];
  chart: ContentBytesChartDataItem[];
}
