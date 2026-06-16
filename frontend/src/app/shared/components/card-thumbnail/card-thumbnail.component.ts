import { Component, input } from '@angular/core';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-card-thumbnail',
  standalone: true,
  imports: [NgOptimizedImage],
  template: `
    <div class="card-surface overflow-hidden rounded-lg transition-transform hover:scale-105 cursor-pointer"
         [style.width.px]="width()" [style.height.px]="height()">
      @if (imageUrl()) {
        <img [ngSrc]="imageUrl()!" [width]="width()" [height]="height()"
             [alt]="name()" class="object-cover w-full h-full" />
      } @else {
        <div class="w-full h-full flex items-center justify-center"
             style="background-color: var(--color-surface-2); color: var(--color-text-muted)">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                  d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"/>
          </svg>
        </div>
      }
    </div>
  `,
})
export class CardThumbnailComponent {
  readonly imageUrl = input<string | null>(null);
  readonly name = input<string>('Card');
  readonly width = input<number>(100);
  readonly height = input<number>(145);
}
