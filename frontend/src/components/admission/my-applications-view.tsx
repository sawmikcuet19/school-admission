import { Clock, RefreshCw } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { StateCard } from "@/components/admission/state-card";
import { StatusBadge } from "@/components/admission/status-badge";
import { useRemainingMs } from "@/hooks/use-remaining-ms";
import { formatDateTime, formatRemaining } from "@/lib/format";
import type { HoldStatus, MyHold } from "@/lib/types";

interface MyApplicationsViewProps {
  holds: MyHold[] | null;
  loading: boolean;
  error: string | null;
  hasStudentId: boolean;
  busyHoldId: string | null;
  onRetry: () => void;
  onConfirm: (hold: MyHold) => void;
  onCancel: (hold: MyHold) => void;
}

export function MyApplicationsView({
  holds,
  loading,
  error,
  hasStudentId,
  busyHoldId,
  onRetry,
  onConfirm,
  onCancel,
}: MyApplicationsViewProps) {
  if (!hasStudentId) {
    return (
      <StateCard
        title="Enter your birth registration number"
        description="Save your 17 digit birth registration number above to see your applications."
      />
    );
  }

  return (
    <div className="flex flex-col gap-4">
      <div>
        <h2 className="text-lg font-semibold">My applications</h2>
        <p className="text-sm text-muted-foreground">
          Pay the admission fee before the hold expires to confirm your seat.
        </p>
      </div>

      {loading && !holds ? (
        <div className="flex flex-col gap-3">
          {Array.from({ length: 2 }).map((_, index) => (
            <Card key={index} className="gap-3 py-5">
              <CardHeader className="px-5">
                <Skeleton className="h-5 w-52" />
              </CardHeader>
              <CardContent className="flex flex-col gap-3 px-5">
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-9 w-40" />
              </CardContent>
            </Card>
          ))}
        </div>
      ) : error && !holds ? (
        <StateCard
          tone="error"
          title="Could not load your applications"
          description={error}
          action={
            <Button variant="outline" size="sm" onClick={onRetry}>
              <RefreshCw className="size-4" />
              Try again
            </Button>
          }
        />
      ) : holds && holds.length === 0 ? (
        <StateCard
          title="No applications yet"
          description="Apply for a seat from the Classes tab. It will appear here."
        />
      ) : (
        <>
          {error ? (
            <div className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-700">
              <span>Live refresh failed: {error}</span>
              <Button variant="ghost" size="sm" onClick={onRetry}>
                Retry
              </Button>
            </div>
          ) : null}

          <div className="flex flex-col gap-3">
            {holds?.map((hold) => (
              <ApplicationCard
                key={hold.holdId}
                hold={hold}
                busy={busyHoldId === hold.holdId}
                onConfirm={() => onConfirm(hold)}
                onCancel={() => onCancel(hold)}
              />
            ))}
          </div>
        </>
      )}
    </div>
  );
}

function ApplicationCard({
  hold,
  busy,
  onConfirm,
  onCancel,
}: {
  hold: MyHold;
  busy: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}) {
  const remaining = useRemainingMs(hold.expiresAt);
  const effectiveStatus: HoldStatus =
    hold.status === "HELD" && remaining <= 0 ? "EXPIRED" : hold.status;
  const canAct = effectiveStatus === "HELD";

  return (
    <Card className="gap-4 py-5">
      <CardHeader className="gap-2 px-5">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <CardTitle className="text-base">{hold.className}</CardTitle>
          <StatusBadge status={effectiveStatus} />
        </div>
      </CardHeader>
      <CardContent className="flex flex-col gap-4 px-5">
        <div className="flex flex-wrap items-center gap-x-6 gap-y-2 text-sm">
          <span className="text-muted-foreground">
            Application <span className="font-mono">{hold.holdId}</span>
          </span>
          <span className="text-muted-foreground">
            {effectiveStatus === "HELD" ? "Pay before" : "Expires"}:{" "}
            <span className="text-foreground">{formatDateTime(hold.expiresAt)}</span>
          </span>
        </div>

        {effectiveStatus === "HELD" ? (
          <div className="flex items-center gap-2 rounded-lg border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-700">
            <Clock className="size-4" />
            <span>
              Time left to pay:{" "}
              <span className="font-mono text-base font-semibold tabular-nums">
                {formatRemaining(remaining)}
              </span>
            </span>
          </div>
        ) : null}

        {effectiveStatus === "CONFIRMED" ? (
          <p className="rounded-lg border border-emerald-500/40 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-700">
            Admission confirmed. Your seat is secured.
          </p>
        ) : null}

        {effectiveStatus === "EXPIRED" ? (
          <p className="rounded-lg border border-destructive/30 bg-destructive/5 px-3 py-2 text-sm text-destructive">
            This hold expired, so the seat was released. You can apply again from
            the Classes tab.
          </p>
        ) : null}

        {effectiveStatus === "CANCELLED" ? (
          <p className="rounded-lg border bg-muted/40 px-3 py-2 text-sm text-muted-foreground">
            This application was cancelled.
          </p>
        ) : null}

        {canAct ? (
          <>
            <Separator />
            <div className="flex flex-wrap gap-2">
              <Button onClick={onConfirm} disabled={busy}>
                {busy ? "Processing..." : "Pay admission fee"}
              </Button>
              <Button variant="outline" onClick={onCancel} disabled={busy}>
                Cancel application
              </Button>
            </div>
          </>
        ) : null}
      </CardContent>
    </Card>
  );
}
