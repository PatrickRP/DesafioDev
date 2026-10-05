import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { describe, expect, it } from 'vitest';
import { App } from './app';

describe('App', () => {
  it('mostra as três seções na navegação', async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const links = Array.from<HTMLElement>(
      fixture.nativeElement.querySelectorAll('a[mat-tab-link]'),
    );
    expect(links.map((a) => a.textContent?.trim())).toEqual([
      expect.stringContaining('Comissões'),
      expect.stringContaining('Estoque'),
      expect.stringContaining('Juros'),
    ]);
  });
});
