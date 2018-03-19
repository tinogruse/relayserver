import {registerLocaleData} from '@angular/common';
import localeDe from '@angular/common/locales/de';
import {platformBrowserDynamic} from '@angular/platform-browser-dynamic';
import 'moment/locale/de';

import {AppModule} from './app/app.module';

registerLocaleData(localeDe);

platformBrowserDynamic().bootstrapModule(AppModule, { preserveWhitespaces: false })
  .catch(err => console.log(err));
