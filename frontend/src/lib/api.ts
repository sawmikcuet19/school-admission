const API_BASE_URL = (
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8080"
).replace(/\/+$/, "");

const STUDENT_ID_HEADER = "X-Student-Id";

interface RequestOptions {
  method?: string;
  body?: unknown;
  studentId?: string;
  adminPassword?: string;
}

async function request<T>(
  path: string,
  { method = "GET", body, studentId, adminPassword }: RequestOptions = {},
): Promise<T> {
  const headers: Record<string, string> = {
    Accept: "application/json",
  };

  if (body !== undefined) {
    headers["Content-Type"] = "application/json";
  }

  if (studentId) {
    headers[STUDENT_ID_HEADER] = studentId;
  }

  if (adminPassword) {
    headers["X-Admin-Password"] = adminPassword;
  }

  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  } catch {
    throw new Error(
      "Could not reach the server. Please check your connection and try again.",
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  let payload: unknown = null;

  if (text) {
    try {
      payload = JSON.parse(text);
    } catch {
      payload = text;
    }
  }

  if (!response.ok) {
    const message = extractMessage(payload) ?? `Request failed (${response.status}).`;
    throw new Error(message);
  }

  return payload as T;
}

function extractMessage(payload: unknown): string | null {
  if (!payload || typeof payload !== "object") {
    return typeof payload === "string" && payload.trim() ? payload : null;
  }

  const record = payload as Record<string, unknown>;

  for (const key of ["message", "title", "detail", "error"]) {
    const value = record[key];
    if (typeof value === "string" && value.trim()) {
      return value;
    }
  }

  if (record.errors && typeof record.errors === "object") {
    const first = Object.values(record.errors as Record<string, unknown>)[0];
    if (Array.isArray(first) && typeof first[0] === "string") {
      return first[0];
    }
  }

  return null;
}

export const api = {
  baseUrl: API_BASE_URL,

  getClasses: () => request<import("./types").ClassSeats[]>("/classes"),

  getClass: (classId: string) =>
    request<import("./types").ClassSeats>(`/classes/${encodeURIComponent(classId)}`),

  createClass: (name: string, totalSeats: number, adminPassword: string) =>
    request<import("./types").CreatedClass>("/classes", {
      method: "POST",
      body: { name, totalSeats },
      adminPassword,
    }),

  verifyAdminPassword: (password: string) =>
    request<import("./types").AdminVerification>("/admin/verify", {
      method: "POST",
      body: { password },
    }),

  changeAdminPassword: (currentPassword: string, newPassword: string) =>
    request<void>("/admin/password", {
      method: "POST",
      body: { currentPassword, newPassword },
    }),

  apply: (classId: string, studentId: string) =>
    request<import("./types").CreatedHold>(
      `/classes/${encodeURIComponent(classId)}/holds`,
      { method: "POST", studentId },
    ),

  getHold: (holdId: string) =>
    request<import("./types").HoldStatusResponse>(
      `/holds/${encodeURIComponent(holdId)}`,
    ),

  confirmHold: (holdId: string, studentId: string) =>
    request<import("./types").ConfirmHoldResponse>(
      `/holds/${encodeURIComponent(holdId)}/confirm`,
      { method: "POST", studentId },
    ),

  cancelHold: (holdId: string, studentId: string) =>
    request<void>(`/holds/${encodeURIComponent(holdId)}`, {
      method: "DELETE",
      studentId,
    }),

  getMyHolds: (studentId: string) =>
    request<import("./types").MyHold[]>("/me/holds", { studentId }),
};
