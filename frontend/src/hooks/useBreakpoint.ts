import { useEffect, useState } from 'react';
import { BREAKPOINTS, getBreakpoint, isCompactSider, isMobileNav, type BreakpointKey } from '../utils/breakpoints';

export function useBreakpoint() {
  const [width, setWidth] = useState(() =>
    typeof window !== 'undefined' ? window.innerWidth : BREAKPOINTS.standardDesktop
  );

  useEffect(() => {
    const onResize = () => setWidth(window.innerWidth);
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, []);

  const breakpoint = getBreakpoint(width);

  return {
    width,
    breakpoint,
    isMobile: breakpoint === 'mobile',
    isTablet: breakpoint === 'tablet',
    isSmallDesktop: breakpoint === 'smallDesktop',
    isDesktop: breakpoint === 'desktop',
    isMobileNav: isMobileNav(width),
    isCompactSider: isCompactSider(width),
    isBelow: (key: BreakpointKey) => {
      const map: Record<BreakpointKey, number> = {
        mobile: BREAKPOINTS.mobile,
        tablet: BREAKPOINTS.tablet,
        smallDesktop: BREAKPOINTS.smallDesktop,
        desktop: BREAKPOINTS.standardDesktop,
      };
      return width < map[key];
    },
  };
}
