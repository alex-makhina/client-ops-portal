export interface SetPasswordRequest {
    userId: string;
    token: string;
    newPassword: string;
}

export interface ApiError {
    message: string;
}

export interface LoginRequest {
    login: string;
    password: string;
}

export interface AuthResponse {
    token: string;
    userId: string; 
    userName?: string;
    roles: string[];
}

export interface ForgotPasswordRequest {
    loginIdentifier: string;
}

export interface ForgotPasswordResponse {
    temporaryPassword: string;
    message: string;
}

export interface ResetPasswordRequest {
    loginIdentifier: string;
    currentPassword: string;
    newPassword: string;
}