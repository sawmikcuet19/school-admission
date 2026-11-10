import { useState } from "react";
import {
  Building2,
  ClipboardList,
  GraduationCap,
  Settings2,
} from "lucide-react";
import { toast } from "sonner";

import { AdminView } from "@/components/admission/admin-view";
import { ClassesView } from "@/components/admission/classes-view";
import { MyApplicationsView } from "@/components/admission/my-applications-view";
import { StudentIdentity } from "@/components/admission/student-identity";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs";
import { usePolledResource } from "@/hooks/use-polled-resource";
import { useLocalStorage } from "@/hooks/use-local-storage";
import { api } from "@/lib/api";
import type { ClassSeats, MyHold } from "@/lib/types";

const POLL_INTERVAL_MS = 4000;
const STUDENT_ID_STORAGE_KEY = "school-admission.student-id";

function errorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : "Something went wrong. Please try again.";
}

export default function App() {
  const [studentId, setStudentId] = useLocalStorage(STUDENT_ID_STORAGE_KEY, "");
  const [activeTab, setActiveTab] = useState("classes");
  const [applyingClassId, setApplyingClassId] = useState<string | null>(null);
  const [busyHoldId, setBusyHoldId] = useState<string | null>(null);
  const [adminPassword, setAdminPassword] = useState("");
  const [adminUnlocked, setAdminUnlocked] = useState(false);

  const classes = usePolledResource(api.getClasses, POLL_INTERVAL_MS, "classes");

  const holds = usePolledResource(
    () => api.getMyHolds(studentId),
    POLL_INTERVAL_MS,
    studentId,
    Boolean(studentId),
  );

  const handleSaveIdentity = (value: string) => {
    setStudentId(value);
    toast.success("Student identity saved in this browser.");
  };

  const handleApply = async (admissionClass: ClassSeats) => {
    if (!studentId) {
      toast.error("Save your 17 digit birth registration number first.");
      return;
    }

    setApplyingClassId(admissionClass.id);

    try {
      await api.apply(admissionClass.id, studentId);

      classes.refresh();
      holds.refresh();
      setActiveTab("applications");
    } catch (error) {
      toast.error(errorMessage(error));
    } finally {
      setApplyingClassId(null);
    }
  };

  const handleConfirm = async (hold: MyHold) => {
    setBusyHoldId(hold.holdId);

    try {
      await api.confirmHold(hold.holdId, studentId);
      toast.success("Payment received. Admission confirmed.");
    } catch (error) {
      toast.error(errorMessage(error));
    } finally {
      setBusyHoldId(null);
      classes.refresh();
      holds.refresh();
    }
  };

  const handleCancel = async (hold: MyHold) => {
    const confirmed = window.confirm(
      `Cancel your application for ${hold.className}? The seat will be released.`,
    );

    if (!confirmed) {
      return;
    }

    setBusyHoldId(hold.holdId);

    try {
      await api.cancelHold(hold.holdId, studentId);
      toast.success("Application cancelled.");
    } catch (error) {
      toast.error(errorMessage(error));
    } finally {
      setBusyHoldId(null);
      classes.refresh();
      holds.refresh();
    }
  };

  const handleVerifyAdmin = async (password: string) => {
    try {
      await api.verifyAdminPassword(password);
      setAdminPassword(password);
      setAdminUnlocked(true);
      toast.success("Admin verified. You can manage classes now.");
      return true;
    } catch (error) {
      toast.error(errorMessage(error));
      return false;
    }
  };

  const handleChangeAdminPassword = async (currentPassword: string, newPassword: string) => {
    try {
      await api.changeAdminPassword(currentPassword, newPassword);
      setAdminPassword(newPassword);
      setAdminUnlocked(true);
      toast.success("Verification password updated.");
      return true;
    } catch (error) {
      toast.error(errorMessage(error));
      return false;
    }
  };

  const handleLockAdmin = () => {
    setAdminUnlocked(false);
    setAdminPassword("");
  };

  const handleCreateClass = async (name: string, totalSeats: number) => {
    if (!adminPassword) {
      toast.error("Unlock the admin tab before creating a class.");
      return false;
    }

    try {
      const created = await api.createClass(name, totalSeats, adminPassword);
      toast.success(`Class "${created.name}" created with ${created.totalSeats} seats.`);
      classes.refresh();
      return true;
    } catch (error) {
      toast.error(errorMessage(error));
      return false;
    }
  };

  return (
    <div className="min-h-screen bg-muted/20">
      <header className="border-b bg-background">
        <div className="mx-auto flex max-w-6xl flex-col gap-3 py-6 md:flex-row md:items-center md:justify-between md:px-8">
          <div className="flex items-center gap-3">
            <span className="flex size-11 items-center justify-center rounded-xl bg-primary text-primary-foreground">
              <GraduationCap className="size-6" />
            </span>
            <div>
              <h1 className="text-xl font-semibold tracking-tight">
                School Admission System
              </h1>
              <p className="text-sm text-muted-foreground">
                First-come-first-served admission with time-limited seat holds.
              </p>
            </div>
          </div>
          <div className="text-xs text-muted-foreground">
            API: <span className="font-mono">{api.baseUrl}</span>
          </div>
        </div>
      </header>

      <main className="mx-auto flex max-w-6xl flex-col gap-6 px-4 py-6 md:px-8">
        <StudentIdentity studentId={studentId} onSave={handleSaveIdentity} />

        <Tabs value={activeTab} onValueChange={setActiveTab}>
          <TabsList className="w-full max-w-xl">
            <TabsTrigger value="classes">
              <Building2 className="size-4" />
              Classes
            </TabsTrigger>
            <TabsTrigger value="applications">
              <ClipboardList className="size-4" />
              My applications
            </TabsTrigger>
            <TabsTrigger value="admin">
              <Settings2 className="size-4" />
              Admin
            </TabsTrigger>
          </TabsList>

          <TabsContent value="classes" className="mt-4">
            <ClassesView
              classes={classes.data}
              loading={classes.loading}
              error={classes.error}
              hasStudentId={Boolean(studentId)}
              applyingClassId={applyingClassId}
              onRetry={classes.refresh}
              onApply={handleApply}
            />
          </TabsContent>

          <TabsContent value="applications" className="mt-4">
            <MyApplicationsView
              holds={holds.data}
              loading={holds.loading}
              error={holds.error}
              hasStudentId={Boolean(studentId)}
              busyHoldId={busyHoldId}
              onRetry={holds.refresh}
              onConfirm={handleConfirm}
              onCancel={handleCancel}
            />
          </TabsContent>

          <TabsContent value="admin" className="mt-4">
            <AdminView
              unlocked={adminUnlocked}
              onVerify={handleVerifyAdmin}
              onChangePassword={handleChangeAdminPassword}
              onCreateClass={handleCreateClass}
              onLock={handleLockAdmin}
            />
          </TabsContent>
        </Tabs>

        <p className="pb-4 text-center text-xs text-muted-foreground">
          No student login required. Admin actions are protected by the verification
          password. Seat counts refresh automatically, and expired holds release their
          seats without a page reload.
        </p>
      </main>
    </div>
  );
}
