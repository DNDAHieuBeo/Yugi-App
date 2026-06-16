import { Component, input, computed } from '@angular/core';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-lp-display',
  standalone: true,
  imports: [DecimalPipe],
  template: `
    <div class="flex flex-col items-center gap-1">
      <span class="text-xs font-medium uppercase tracking-wider" style="color: var(--color-text-muted)">
        {{ playerName() }}
      </span>
      <span class="text-4xl font-bold tabular-nums" [style.color]="lpColor()">
        {{ lp() | number }}
      </span>
      <div class="w-full h-1.5 rounded-full overflow-hidden" style="background-color: var(--color-surface-2)">
        <div class="h-full rounded-full transition-all duration-500"
             [style.width.%]="percentage()"
             [style.background-color]="lpColor()">
        </div>
      </div>
    </div>
  `,
  host: { class: 'block w-full' },
})
export class LpDisplayComponent {
  readonly playerName = input<string>('Player');
  readonly lp = input<number>(8000);
  readonly maxLp = input<number>(8000);

  readonly percentage = computed(() => Math.max(0, Math.min(100, (this.lp() / this.maxLp()) * 100)));

  readonly lpColor = computed(() => {
    const pct = this.percentage();
    if (pct > 50) return 'var(--color-primary)';
    if (pct > 25) return '#f59e0b';
    return '#ef4444';
  });
}
