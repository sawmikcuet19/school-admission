import { useState, type FormEvent } from "react";
import { KeyRound, Lock, LogOut, ShieldAlert, ShieldCheck } from "lucide-react";

import { CreateClassView } from "@/components/admission/create-class-view";
import { Badge } from "@/components/ui/badge";
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
import { Separator } from "@/components/ui/separator";

const DEMO_ADMIN_PASSWORD = "ChangeMe-Admin1";

interface AdminViewProps {
  unlocked: boolean;
  onVerify: (password: string) => Promise<boolean>;
  onChangePassword: (currentPassword: string, newPassword: string) => Promise<boolean>;
  onCreateClass: (name: string, totalSeats: number) => Promise<boolean>;
  onLock: () => void;
}

export function AdminView({
  unlocked,
  onVerify,
  onChangePassword,
  onCreateClass,
  onLock,
}: AdminViewProps) {
  const [verifyPassword, setVerifyPassword] = useState("");
  const [verifying, setVerifying] = useState(false);

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [changing, setChanging] = useState(false);

  const handleVerify = async (event: FormEvent) => {
    event.preventDefault();

    if (!verifyPassword || verifying) {
      return;
    }

    setVerifying(true);

    try {
      const verified = await onVerify(verifyPassword);

      if (verified) {
        setVerifyPassword("");
      }
    } finally {
      setVerifying(false);
    }
  };

  const canChangePassword =
    currentPassword.length > 0 &&
    newPassword.length >= 8 &&
    newPassword === confirmPassword;

  const handleChangePassword = async (event: FormEvent) => {
    event.preventDefault();

    if (!canChangePassword || changing) {
      return;
    }

    setChanging(true);

    try {
      const changed = await onChangePassword(currentPassword, newPassword);

      if (changed) {
        setCurrentPassword("");
        setNewPassword("");
        setConfirmPassword("");
      }
    } finally {
      setChanging(false);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      <Card className="max-w-xl border-primary/20 bg-primary/[0.03]">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <KeyRound className="size-4" />
            Admin verification
          </CardTitle>
          <CardDescription>
            Creating classes is an admin action. The verification password is stored in the
            database only as a salted hash and is seeded on first startup from the{" "}
            <span className="font-mono">ADMIN_PASSWORD</span> setting. There is no login
            session: enter the password to unlock this tab for the current page. Only someone
            who knows the current password can change it.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <div className="rounded-lg border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-700">
            <p className="flex items-center gap-2 font-medium">
              <ShieldAlert className="size-4" />
              Demo password: <span className="font-mono">{DEMO_ADMIN_PASSWORD}</span>
            </p>
            <p className="mt-1 text-xs">
              This password is shown only because this project is a demo. In a real production
              deployment it would never be displayed; it would be configured privately and kept
              secret.
            </p>
          </div>

          {unlocked ? (
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div className="flex items-center gap-2 text-sm">
                <Badge variant="success">
                  <ShieldCheck className="size-3" />
                  Admin verified
                </Badge>
                <span className="text-muted-foreground">This tab is unlocked.</span>
              </div>
              <Button variant="outline" size="sm" onClick={onLock}>
                <LogOut className="size-4" />
                Lock admin
              </Button>
            </div>
          ) : (
            <form onSubmit={handleVerify} className="flex flex-col gap-3 sm:flex-row sm:items-end">
              <div className="flex flex-1 flex-col gap-1.5">
                <Label htmlFor="admin-password">Verification password</Label>
                <Input
                  id="admin-password"
                  type="password"
                  autoComplete="current-password"
                  placeholder="Enter the admin verification password"
                  value={verifyPassword}
                  onChange={(event) => setVerifyPassword(event.target.value)}
                />
              </div>
              <Button type="submit" disabled={!verifyPassword || verifying}>
                <Lock className="size-4" />
                {verifying ? "Verifying..." : "Unlock admin"}
              </Button>
            </form>
          )}
        </CardContent>
      </Card>

      {unlocked ? (
        <>
          <CreateClassView onCreate={onCreateClass} />

          <Card className="max-w-xl">
            <CardHeader>
              <CardTitle className="text-base">Change verification password</CardTitle>
              <CardDescription>
                Only someone who knows the current password can change it. The new password is
                hashed before it is stored, so the database never keeps it in plain text.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleChangePassword} className="flex flex-col gap-4">
                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="current-admin-password">Current password</Label>
                  <Input
                    id="current-admin-password"
                    type="password"
                    autoComplete="current-password"
                    value={currentPassword}
                    onChange={(event) => setCurrentPassword(event.target.value)}
                  />
                </div>

                <Separator />

                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="new-admin-password">New password</Label>
                  <Input
                    id="new-admin-password"
                    type="password"
                    autoComplete="new-password"
                    value={newPassword}
                    onChange={(event) => setNewPassword(event.target.value)}
                  />
                  <p className="text-xs text-muted-foreground">
                    At least 8 characters.
                  </p>
                </div>

                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="confirm-admin-password">Confirm new password</Label>
                  <Input
                    id="confirm-admin-password"
                    type="password"
                    autoComplete="new-password"
                    value={confirmPassword}
                    onChange={(event) => setConfirmPassword(event.target.value)}
                  />
                  {confirmPassword.length > 0 && newPassword !== confirmPassword ? (
                    <p className="text-xs text-destructive">The passwords do not match.</p>
                  ) : null}
                </div>

                <Button type="submit" disabled={!canChangePassword || changing}>
                  {changing ? "Saving..." : "Change password"}
                </Button>
              </form>
            </CardContent>
          </Card>
        </>
      ) : null}
    </div>
  );
}
