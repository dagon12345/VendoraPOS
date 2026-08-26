import { Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../../core/services/product';
import { UploadService } from '../../../core/services/upload';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  imports: [CommonModule, ReactiveFormsModule, ConfirmDialog],
  selector: 'app-product-form',
  styleUrl: './product-form.scss',
  templateUrl: './product-form.html',
})
export class ProductForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly uploadService = inject(UploadService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  @ViewChild('fileInput') fileInputRef?: ElementRef<HTMLInputElement>;

  readonly isEditMode = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly confirmingSave = signal(false);
  readonly uploadingImage = signal(false);
  readonly imageError = signal<string | null>(null);

  private productId: string | null = null;

  readonly form = this.fb.group({
    sku: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(50)]),
    name: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(200)]),
    price: this.fb.nonNullable.control(0, [Validators.required, Validators.min(0.01)]),
    initialQuantity: this.fb.nonNullable.control(0, [Validators.required, Validators.min(0)]),
    description: this.fb.control<string | null>(null),
    barcode: this.fb.control<string | null>(null),
    imageUrl: this.fb.control<string | null>(null),
    expiryDate: this.fb.control<string | null>(null),
  });

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id');

    if (this.productId) {
      this.isEditMode.set(true);
      this.form.controls.sku.disable();
      this.form.controls.initialQuantity.disable();

      this.productService.getById(this.productId).subscribe({
        next: (product) => {
          this.form.patchValue({
            sku: product.sku,
            name: product.name,
            price: product.price,
            description: product.description,
            barcode: product.barcode,
            imageUrl: product.imageUrl,
            expiryDate: product.expiryDate,
          });
        },
        error: () => this.error.set('Could not load this product.'),
      });
    }
  }

  /** Editing an existing product asks for confirmation first; creating a new one does not. */
  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.isEditMode()) {
      this.confirmingSave.set(true);
      return;
    }

    this.save();
  }

  confirmSave(): void {
    this.confirmingSave.set(false);
    this.save();
  }

  cancelSave(): void {
    this.confirmingSave.set(false);
  }

  private save(): void {
    this.saving.set(true);
    this.error.set(null);
    const value = this.form.getRawValue();

    const request$ = this.isEditMode()
      ? this.productService.update(this.productId!, {
          name: value.name,
          price: value.price,
          description: value.description,
          barcode: value.barcode,
          imageUrl: value.imageUrl,
          expiryDate: value.expiryDate,
        })
      : this.productService.create({
          sku: value.sku,
          name: value.name,
          price: value.price,
          initialQuantity: value.initialQuantity,
          description: value.description,
          barcode: value.barcode,
          imageUrl: value.imageUrl,
          expiryDate: value.expiryDate,
        });

    request$.subscribe({
      next: () => this.router.navigate(['/products']),
      error: () => {
        this.error.set('Could not save this product. Check the values and try again.');
        this.saving.set(false);
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/products']);
  }

  selectImage(): void {
    this.fileInputRef?.nativeElement.click();
  }

  /** Uploads the picked file (from the device's own gallery/photo picker/file browser -
   *  whatever the OS presents for `accept="image/*"`) to the API, which stores it on this
   *  machine's local disk and hands back a URL we drop straight into the form. */
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = ''; // allow re-selecting the same file later
    if (!file) {
      return;
    }

    this.imageError.set(null);
    this.uploadingImage.set(true);

    this.uploadService.uploadProductImage(file).subscribe({
      next: (result) => {
        this.form.patchValue({ imageUrl: result.url });
        this.uploadingImage.set(false);
      },
      error: (err) => {
        this.imageError.set(typeof err?.error === 'string' ? err.error : 'Could not upload this image.');
        this.uploadingImage.set(false);
      },
    });
  }

  removeImage(): void {
    this.form.patchValue({ imageUrl: null });
  }
}
