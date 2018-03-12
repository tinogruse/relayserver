import {registerLocaleData} from '@angular/common';
import localeDe from '@angular/common/locales/de';
import {enableProdMode} from '@angular/core';
import {platformBrowserDynamic} from '@angular/platform-browser-dynamic';
import 'moment/locale/de';

import {AppModule} from './app/app.module';
import {environment} from './environments/environment';

if (environment.production) {
  enableProdMode();
}

registerLocaleData(localeDe);

platformBrowserDynamic().bootstrapModule(AppModule, { preserveWhitespaces: false })
  .catch(err => console.log(err));
