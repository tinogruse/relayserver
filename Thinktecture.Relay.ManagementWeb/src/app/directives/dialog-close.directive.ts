import {Directive, ElementRef, HostListener, Input, OnInit, Optional} from '@angular/core';
import {MatDialog, MatDialogRef} from '@angular/material';

@Directive({
  selector: 'form[trsDialogClose]',
})
export class DialogCloseDirective implements OnInit {
  constructor(
    @Optional() private _dialogRef: MatDialogRef<any>,
    private readonly _elementRef: ElementRef,
    private readonly _dialog: MatDialog,
  ) {
  }

  @Input('trsDialogClose') dialogResult: any;

  ngOnInit() {
    if (!this._dialogRef) {
      this._dialogRef = getClosestDialog(this._elementRef, this._dialog.openDialogs)!;
    }
  }

  @HostListener('submit')
  submit() {
    this._dialogRef.close(this.dialogResult);
  }
}

function getClosestDialog(element: ElementRef, openDialogs: MatDialogRef<any>[]) {
  let parent: HTMLElement | null = element.nativeElement.parentElement;

  while (parent && !parent.classList.contains('mat-dialog-container')) {
    parent = parent.parentElement;
  }

  return parent ? openDialogs.find(dialog => dialog.id === parent!.id) : null;
}
