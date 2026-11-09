import { useState, type FormEvent } from "react";
import { Plus } from "lucide-react";

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

interface CreateClassViewProps {
  onCreate: (name: string, totalSeats: number) => Promise<boolean>;
}

export function CreateClassView({ onCreate }: CreateClassViewProps) {
  const [name, setName] = useState("");
  const [totalSeats, setTotalSeats] = useState("120");
  const [submitting, setSubmitting] = useState(false);

  const seats = Number(totalSeats);
  const isValid =
    name.trim().length > 0 && Number.isInteger(seats) && seats > 0;

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();

    if (!isValid || submitting) {
      return;
    }

    setSubmitting(true);

    try {
      const created = await onCreate(name.trim(), seats);

      if (created) {
        setName("");
        setTotalSeats("120");
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Card className="max-w-xl">
      <CardHeader>
        <CardTitle className="text-base">Create a class (school admin)</CardTitle>
        <CardDescription>
          Each class/shift is created separately with its own seat count, e.g.
          &quot;Class 6 - Morning Shift&quot; and &quot;Class 6 - Day Shift&quot;.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="class-name">Class name</Label>
            <Input
              id="class-name"
              placeholder="Class 6 - Morning Shift"
              value={name}
              maxLength={200}
              onChange={(event) => setName(event.target.value)}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="total-seats">Total seats</Label>
            <Input
              id="total-seats"
              type="number"
              min={1}
              value={totalSeats}
              onChange={(event) => setTotalSeats(event.target.value)}
            />
          </div>

          <Button type="submit" disabled={!isValid || submitting}>
            <Plus className="size-4" />
            {submitting ? "Creating..." : "Create class"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
