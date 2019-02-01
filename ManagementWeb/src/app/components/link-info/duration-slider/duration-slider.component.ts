import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';

@Component({
  selector: 'trs-duration-slider',
  templateUrl: './duration-slider.component.html',
  styleUrls: ['./duration-slider.component.scss'],
})
export class DurationSliderComponent implements OnInit {
  private lastValue = 0;

  @Input() caption: string;
  @Input() value: number | null;
  @Input() unit: string;
  @Input() max: number;
  @Input() step: number;

  @Output() readonly valueChange = new EventEmitter<number | null>();

  ngOnInit() {
    this.lastValue = this.value || 0;
  }

  toggleValue(checked: boolean) {
    this.value = checked ? this.lastValue : null;
    this.valueChange.emit(this.value);
  }

  setValue(value: number) {
    this.lastValue = this.value = value;
    this.valueChange.emit(this.value);
  }
}
