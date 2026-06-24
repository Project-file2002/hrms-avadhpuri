export const BREAKPOINTS = {
  mobile: 480,
  tablet: 768,
  smallDesktop: 1024,
  standardDesktop: 1280,
} as const;

export type BreakpointKey = 'mobile' | 'tablet' | 'smallDesktop' | 'desktop';

export function getBreakpoint(width: number): BreakpointKey {
  if (width < BREAKPOINTS.mobile) return 'mobile';
  if (width < BREAKPOINTS.tablet) return 'tablet';
  if (width < BREAKPOINTS.smallDesktop) return 'smallDesktop';
  return 'desktop';
}

export function isMobileNav(width: number) {
  return width < BREAKPOINTS.tablet;
}

export function isCompactSider(width: number) {
  return width >= BREAKPOINTS.tablet && width < BREAKPOINTS.smallDesktop;
}
