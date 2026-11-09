import type { HoldStatus } from "@/lib/types";

export function formatRemaining(milliseconds: number): string {
  if (milliseconds <= 0) {
    return "00:00";
  }

  const totalSeconds = Math.floor(milliseconds / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;

  return `${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;
}

export function formatDateTime(value: string): string {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  });
}

export function statusLabel(status: HoldStatus): string {
  switch (status) {
    case "HELD":
      return "Seat held";
    case "CONFIRMED":
      return "Admission confirmed";
    case "EXPIRED":
      return "Expired";
    case "CANCELLED":
      return "Cancelled";
  }
}

export function statusVariant(
  status: HoldStatus,
): "default" | "secondary" | "destructive" | "success" | "warning" {
  switch (status) {
    case "HELD":
      return "warning";
    case "CONFIRMED":
      return "success";
    case "EXPIRED":
      return "destructive";
    case "CANCELLED":
      return "secondary";
  }
}
