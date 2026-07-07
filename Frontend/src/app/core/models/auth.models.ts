export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  phoneNumber: string;
  password: string;
  role: string;
}

export interface CurrentUserResponse {
  id: string
  displayName: string
  email: string
  phoneNumber: string
  role: string
  isActive: boolean
  profileCompleted: boolean
  trustScore?: number | null
  dashboardUrl?: string | null
}

export interface AuthResponse {
  isAuthenticated: boolean;
  succeeded?: boolean;
  requiresEmailConfirmation?: boolean;
  emailConfirmationSent?: boolean;
  isEmailConfirmed?: boolean;
  profileCompleted?: boolean;
  message: string;
  displayName: string
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  userName: string;
  email: string;
  roles: string[];
}

export type UserRole = 'customer' | 'provider' | 'admin' | 'employee';
