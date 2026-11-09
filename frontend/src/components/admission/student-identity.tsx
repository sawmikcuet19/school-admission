import { useEffect, useState, type FormEvent } from "react";
import { IdCard, Check } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";

interface StudentIdentityProps {
  studentId: string;
  onSave: (studentId: string) => void;
}

const STUDENT_ID_PATTERN = /^\d{17}$/;

export function StudentIdentity({ studentId, onSave }: StudentIdentityProps) {
  const [draft, setDraft] = useState(studentId);

  useEffect(() => {
    setDraft(studentId);
  }, [studentId]);

  const isValid = STUDENT_ID_PATTERN.test(draft);
  const isChanged = draft !== studentId;

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();

    if (isValid) {
      onSave(draft);
    }
  };

  return (
    <Card className="border-primary/20 bg-primary/[0.03]">
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <IdCard className="size-4" />
          Student identity
        </CardTitle>
        <CardDescription>
          Enter the student&apos;s 17 digit birth registration number. It is
          stored in this browser, so refreshing the page keeps showing your
          applications. No login is required.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="flex flex-col gap-3 sm:flex-row sm:items-end">
          <div className="flex flex-1 flex-col gap-1.5">
            <Label htmlFor="student-id">Birth registration number</Label>
            <Input
              id="student-id"
              inputMode="numeric"
              autoComplete="off"
              placeholder="e.g. 20152691234567890"
              maxLength={17}
              value={draft}
              aria-invalid={draft.length > 0 && !isValid}
              onChange={(event) => {
                setDraft(event.target.value.replace(/\D/g, "").slice(0, 17));
              }}
            />
            <p className="text-xs text-muted-foreground">
              Exactly 17 digits. {draft.length}/17 entered.
            </p>
          </div>
          <Button type="submit" disabled={!isValid || !isChanged}>
            <Check className="size-4" />
            Save identity
          </Button>
        </form>

        {studentId ? (
          <div className="mt-3 flex items-center gap-2 text-sm">
            <Badge variant="success">Active</Badge>
            <span className="font-mono text-muted-foreground">{studentId}</span>
          </div>
        ) : (
          <p className="mt-3 text-sm text-muted-foreground">
            No identity saved yet. You need one before applying for a seat.
          </p>
        )}
      </CardContent>
    </Card>
  );
}
