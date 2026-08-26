import { Directive, ElementRef, Input, OnChanges, inject } from '@angular/core';
import JsBarcode from 'jsbarcode';

/** Renders a scannable Code128 barcode into the host <svg> from the bound value. */
@Directive({
  selector: 'svg[appBarcode]',
})
export class BarcodeDirective implements OnChanges {
  private readonly el = inject(ElementRef<SVGElement>);

  @Input('appBarcode') value: string | null | undefined;

  ngOnChanges(): void {
    if (!this.value) {
      return;
    }
    try {
      JsBarcode(this.el.nativeElement, this.value, {
        format: 'CODE128',
        displayValue: false,
        height: 28,
        margin: 0,
      });
    } catch {
      // Not every string is valid for the CODE128 symbology - leave the card without a barcode
      // graphic rather than crashing the checkout page over it.
    }
  }
}
