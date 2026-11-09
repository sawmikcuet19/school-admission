import { RefreshCw, Users } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { StateCard } from "@/components/admission/state-card";
import type { ClassSeats } from "@/lib/types";

interface ClassesViewProps {
  classes: ClassSeats[] | null;
  loading: boolean;
  error: string | null;
  hasStudentId: boolean;
  applyingClassId: string | null;
  onRetry: () => void;
  onApply: (admissionClass: ClassSeats) => void;
}

export function ClassesView({
  classes,
  loading,
  error,
  hasStudentId,
  applyingClassId,
  onRetry,
  onApply,
}: ClassesViewProps) {
  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-end justify-between gap-2">
        <div>
          <h2 className="text-lg font-semibold">Admission classes</h2>
          <p className="text-sm text-muted-foreground">
            Seats are first-come-first-served. Counts refresh automatically every
            few seconds.
          </p>
        </div>
      </div>

      {loading && !classes ? (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: 3 }).map((_, index) => (
            <Card key={index} className="gap-4 py-5">
              <CardHeader className="px-5">
                <Skeleton className="h-5 w-40" />
              </CardHeader>
              <CardContent className="flex flex-col gap-3 px-5">
                <Skeleton className="h-14 w-full" />
                <Skeleton className="h-2 w-full" />
                <Skeleton className="h-9 w-full" />
              </CardContent>
            </Card>
          ))}
        </div>
      ) : error && !classes ? (
        <StateCard
          tone="error"
          title="Could not load classes"
          description={error}
          action={
            <Button variant="outline" size="sm" onClick={onRetry}>
              <RefreshCw className="size-4" />
              Try again
            </Button>
          }
        />
      ) : classes && classes.length === 0 ? (
        <StateCard
          title="No classes yet"
          description="A school admin can create the first class from the Admin tab."
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

          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {classes?.map((admissionClass) => {
              const total = Math.max(1, admissionClass.totalSeats);
              const isApplying = applyingClassId === admissionClass.id;
              const isFull = admissionClass.availableSeats <= 0;

              return (
                <Card key={admissionClass.id} className="gap-4 py-5">
                  <CardHeader className="gap-1 px-5">
                    <CardTitle className="flex items-center gap-2 text-base">
                      <Users className="size-4 text-muted-foreground" />
                      {admissionClass.name}
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="flex flex-col gap-4 px-5">
                    <dl className="grid grid-cols-4 gap-2 text-center">
                      <SeatStat label="Total" value={admissionClass.totalSeats} />
                      <SeatStat label="Held" value={admissionClass.heldSeats} tone="amber" />
                      <SeatStat
                        label="Confirmed"
                        value={admissionClass.confirmedSeats}
                        tone="emerald"
                      />
                      <SeatStat
                        label="Available"
                        value={admissionClass.availableSeats}
                        tone={isFull ? "red" : "sky"}
                      />
                    </dl>

                    <div className="flex h-2 overflow-hidden rounded-full bg-muted">
                      <span
                        className="bg-amber-400"
                        style={{
                          width: `${(admissionClass.heldSeats / total) * 100}%`,
                        }}
                      />
                      <span
                        className="bg-emerald-500"
                        style={{
                          width: `${(admissionClass.confirmedSeats / total) * 100}%`,
                        }}
                      />
                    </div>

                    <Button
                      onClick={() => onApply(admissionClass)}
                      disabled={isFull || isApplying || !hasStudentId}
                    >
                      {isApplying
                        ? "Holding your seat..."
                        : isFull
                          ? "No seats available"
                          : "Apply for a seat"}
                    </Button>

                    {!hasStudentId ? (
                      <p className="text-xs text-muted-foreground">
                        Save your birth registration number above to apply.
                      </p>
                    ) : null}
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </>
      )}
    </div>
  );
}

function SeatStat({
  label,
  value,
  tone = "neutral",
}: {
  label: string;
  value: number;
  tone?: "neutral" | "amber" | "emerald" | "sky" | "red";
}) {
  const toneClass = {
    neutral: "text-foreground",
    amber: "text-amber-600",
    emerald: "text-emerald-600",
    sky: "text-sky-600",
    red: "text-red-600",
  }[tone];

  return (
    <div className="rounded-lg border bg-muted/30 px-1 py-2">
      <dt className="text-[11px] text-muted-foreground">{label}</dt>
      <dd className={`text-lg font-semibold tabular-nums ${toneClass}`}>{value}</dd>
    </div>
  );
}
