import {animate, state, style, transition, trigger} from '@angular/animations';
import {Component, HostBinding} from '@angular/core';

@Component({
  selector: 'trs-spinner-overlay',
  templateUrl: './spinner-overlay.component.html',
  styleUrls: ['./spinner-overlay.component.scss'],
  animations: [
    trigger('fadeInOut', [
      state('void', style({ opacity: 0 })),
      transition(':enter, :leave', animate(300)),
    ]),
  ],
})
export class SpinnerOverlayComponent {
  @HostBinding('@fadeInOut') fadeInOut = true;
}
