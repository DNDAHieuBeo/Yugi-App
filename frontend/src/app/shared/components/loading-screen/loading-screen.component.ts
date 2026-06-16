import { Component } from '@angular/core';

@Component({
  selector: 'app-loading-screen',
  standalone: true,
  template: `
    <div class="fixed inset-0 flex items-center justify-center z-[9999]"
         style="background:#060e14">
      <div class="relative flex items-center justify-center"
           style="width:200px;height:200px">

        <!-- Spinning dashed ring -->
        <svg class="absolute inset-0 ring-spin" viewBox="0 0 200 200"
             xmlns="http://www.w3.org/2000/svg">
          <defs>
            <linearGradient id="ringGrad" x1="0%" y1="0%" x2="100%" y2="100%">
              <stop offset="0%"   stop-color="#0d9e62" stop-opacity="0.3"/>
              <stop offset="60%"  stop-color="#1db87a"/>
              <stop offset="100%" stop-color="#5de8b5"/>
            </linearGradient>
          </defs>
          <circle cx="100" cy="100" r="88"
                  fill="none"
                  stroke="url(#ringGrad)"
                  stroke-width="5"
                  stroke-linecap="round"
                  stroke-dasharray="14 10"/>
        </svg>

        <!-- Card body -->
        <div class="card-body">
          <!-- Corner accent -->
          <div class="card-accent"></div>

          <!-- Vault wheel -->
          <svg class="wheel" viewBox="0 0 36 36" fill="none"
               xmlns="http://www.w3.org/2000/svg">
            <circle cx="18" cy="18" r="5" fill="#0a3d28" stroke="#5de8b5" stroke-width="1.5"/>
            <line x1="18" y1="2"  x2="18" y2="10" stroke="#5de8b5" stroke-width="2.5" stroke-linecap="round"/>
            <line x1="18" y1="26" x2="18" y2="34" stroke="#5de8b5" stroke-width="2.5" stroke-linecap="round"/>
            <line x1="2"  y1="18" x2="10" y2="18" stroke="#5de8b5" stroke-width="2.5" stroke-linecap="round"/>
            <line x1="26" y1="18" x2="34" y2="18" stroke="#5de8b5" stroke-width="2.5" stroke-linecap="round"/>
          </svg>
        </div>

      </div>
    </div>
  `,
  styles: [`
    @keyframes spin {
      from { transform: rotate(0deg); }
      to   { transform: rotate(360deg); }
    }

    .ring-spin {
      animation: spin 3s linear infinite;
    }

    .card-body {
      width: 80px;
      height: 100px;
      background: linear-gradient(145deg, #1db87a, #0d9e62);
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      position: relative;
    }

    .card-accent {
      position: absolute;
      top: -6px;
      right: -6px;
      width: 24px;
      height: 24px;
      background: linear-gradient(135deg, #5de8b5, #1db87a);
      border-radius: 4px;
      transform: rotate(8deg);
    }

    .wheel {
      width: 36px;
      height: 36px;
      position: relative;
      z-index: 1;
    }
  `],
})
export class LoadingScreenComponent {}
