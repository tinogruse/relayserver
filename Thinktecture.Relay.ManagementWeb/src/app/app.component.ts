import {Component, Inject, LOCALE_ID} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import * as moment from 'moment';

@Component({
  selector: 'trs-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  constructor(@Inject(LOCALE_ID) locale: string, translate: TranslateService) {
    translate.setDefaultLang('en');
    translate.use(locale.substring(0, 2));

    moment.locale(locale);
  }
}
