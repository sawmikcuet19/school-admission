import { useEffect, useState } from "react";

export function useRemainingMs(expiresAt: string): number {
  const compute = () => {
    const target = new Date(expiresAt).getTime();

    if (Number.isNaN(target)) {
      return 0;
    }

    return Math.max(0, target - Date.now());
  };

  const [remaining, setRemaining] = useState(compute);

  useEffect(() => {
    setRemaining(compute);

    const timer = window.setInterval(() => {
      setRemaining(compute());
    }, 1000);

    return () => window.clearInterval(timer);
  }, [expiresAt]);

  return remaining;
}
