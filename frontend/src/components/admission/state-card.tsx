import * as React from "react";

import { cn } from "@/lib/utils";

export function StateCard({
  tone = "neutral",
  title,
  description,
  action,
  className,
}: {
  tone?: "neutral" | "error" | "success";
  title: string;
  description?: string;
  action?: React.ReactNode;
  className?: string;
}) {
  const toneClasses = {
    neutral: "border-dashed text-muted-foreground",
    error: "border-destructive/40 bg-destructive/5 text-destructive",
    success: "border-emerald-500/40 bg-emerald-500/5 text-emerald-700",
  } as const;

  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center gap-2 rounded-xl border px-6 py-10 text-center",
        toneClasses[tone],
        className,
      )}
    >
      <p className="text-sm font-medium">{title}</p>
      {description ? (
        <p className="max-w-md text-sm text-muted-foreground">{description}</p>
      ) : null}
      {action}
    </div>
  );
}
