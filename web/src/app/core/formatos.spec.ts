import { describe, expect, it } from 'vitest';
import { normalizarValor } from './formatos';

describe('normalizarValor', () => {
  it.each([
    ['1234,56', '1234.56'],
    ['1.234,56', '1234.56'],
    ['R$ 1.234,56', '1234.56'],
    ['1234.56', '1234.56'],
    ['1000', '1000'],
  ])('%s → %s', (entrada, esperado) => expect(normalizarValor(entrada)).toBe(esperado));

  it.each(['', 'abc', '0', '0,00', '-5', '12,345', '1.2.3'])('rejeita "%s"', (entrada) =>
    expect(normalizarValor(entrada)).toBeNull(),
  );
});
