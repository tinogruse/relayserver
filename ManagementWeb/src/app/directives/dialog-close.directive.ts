import {Directive, ElementRef, HostListener, Input, OnInit, Optional} from '@angular/core';
import {MatDialog, MatDialogRef} from '@angular/material';

function getClosestDialog(element: ElementRef, openDialogs: MatDialogRef<any>[]) {
  let parent: HTMLElement | null = element.nativeElement.parentElement;

  while (parent && !parent.classList.contains('mat-dialog-container')) {
    parent = parent.parentElement;
  }

  return parent ? openDialogs.find(dialog => dialog.id === parent !.id) : null;
}

@Directive({
  selector: 'form[trsDialogClose]',
})
export class DialogCloseDirective implements OnInit {
  constructor(
    @Optional() private dialogRef: MatDialogRef<any>,
    private readonly elementRef: ElementRef,
    private readonly matDialog: MatDialog,
  ) {
  }

  @Input('trsDialogClose') dialogResult: any;

  ngOnInit() {
    if (!this.dialogRef) {
      this.dialogRef = getClosestDialog(this.elementRef, this.matDialog.openDialogs) !;
    }
  }

  @HostListener('submit')
  submit() {
    this.dialogRef.close(this.dialogResult);
  }
}
