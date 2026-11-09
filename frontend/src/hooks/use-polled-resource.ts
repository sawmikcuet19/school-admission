import { useCallback, useEffect, useRef, useState } from "react";

interface PolledState<T> {
  data: T | null;
  error: string | null;
  loading: boolean;
}

export function usePolledResource<T>(
  loader: () => Promise<T>,
  intervalMs: number,
  key: string,
  enabled = true,
): PolledState<T> & { refresh: () => void } {
  const loaderRef = useRef(loader);
  loaderRef.current = loader;

  const [state, setState] = useState<PolledState<T>>({
    data: null,
    error: null,
    loading: enabled,
  });
  const [version, setVersion] = useState(0);

  const refresh = useCallback(() => {
    setVersion((current) => current + 1);
  }, []);

  useEffect(() => {
    if (!enabled) {
      setState({ data: null, error: null, loading: false });
      return;
    }

    let cancelled = false;

    const run = async (showLoading: boolean) => {
      if (showLoading) {
        setState((current) => ({ ...current, loading: true }));
      }

      try {
        const data = await loaderRef.current();

        if (!cancelled) {
          setState({ data, error: null, loading: false });
        }
      } catch (error) {
        if (!cancelled) {
          setState((current) => ({
            data: current.data,
            error:
              error instanceof Error
                ? error.message
                : "Something went wrong. Please try again.",
            loading: false,
          }));
        }
      }
    };

    void run(true);

    const timer = window.setInterval(() => {
      void run(false);
    }, intervalMs);

    return () => {
      cancelled = true;
      window.clearInterval(timer);
    };
  }, [enabled, intervalMs, key, version]);

  return { ...state, refresh };
}
