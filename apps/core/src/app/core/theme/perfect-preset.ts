import { definePreset } from '@primeuix/themes';
import Lara from '@primeuix/themes/lara';

const perfectPalette = {
  50: '#EFF6FF',
  100: '#DBEAFE',
  200: '#C7D2FE',
  300: '#60A5FA',
  400: '#7C3AED',
  500: '#4C1D95',
  600: '#5B21B6',
  700: '#3A0CA3',
  800: '#4338CA',
  900: '#3A0CA3',
  950: '#3A0CA3'
} as const;

export const PerfectPreset = definePreset(Lara, {
  primitive: {
    emerald: perfectPalette,
    green: perfectPalette,
    indigo: perfectPalette,
    blue: perfectPalette
  }
});
