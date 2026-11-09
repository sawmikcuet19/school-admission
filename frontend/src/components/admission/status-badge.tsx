import { Badge } from "@/components/ui/badge";
import { statusLabel, statusVariant } from "@/lib/format";
import type { HoldStatus } from "@/lib/types";

export function StatusBadge({ status }: { status: HoldStatus }) {
  return <Badge variant={statusVariant(status)}>{statusLabel(status)}</Badge>;
}
