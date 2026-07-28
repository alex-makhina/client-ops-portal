import { apiClient } from './client';
import type {
    LoginRequest,
    AuthResponse,
    ForgotPasswordRequest,
    ForgotPasswordResponse,
    ResetPasswordRequest,
    SetPasswordRequest
} from '../types/auth.types';

export const authService = {
    async login(data: LoginRequest): Promise<AuthResponse> {
        const response = await apiClient.post<AuthResponse>('/Auth/login', {
            login: data.login, 
            password: data.password
        });
        return response.data;
    },

    async forgotPassword(data: ForgotPasswordRequest): Promise<ForgotPasswordResponse> {
        const response = await apiClient.post<ForgotPasswordResponse>('/Auth/forgot-password', {
            loginIdentifier: data.loginIdentifier
        });
        return response.data;
    },

    async resetPassword(data: ResetPasswordRequest): Promise<{ message: string }> {
        const response = await apiClient.post('/Auth/reset-password', {
            loginIdentifier: data.loginIdentifier,
            currentPassword: data.currentPassword,
            newPassword: data.newPassword
        });
        return response.data;
    },

    async setPassword(data: SetPasswordRequest): Promise<{ message: string }> {
        const response = await apiClient.post('/Auth/set-password', {
            userId: data.userId,
            token: data.token,
            newPassword: data.newPassword
        });
        return response.data;
    },
};