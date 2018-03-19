import * as moment from 'moment';
import {Moment} from 'moment';

export function processContentBytesChartDataItem(chart: ContentBytesChartDataItem) {
  chart.key = moment(chart.key);
}

export interface ContentBytesChartDataItem {
  key: Moment;
  in: number;
  out: number;
}
