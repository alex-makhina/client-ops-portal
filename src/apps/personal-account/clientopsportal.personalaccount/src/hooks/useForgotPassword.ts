import { useMutation } from '@tanstack/react-query';
import { authService } from '../api/auth';
import type { ForgotPasswordRequest, ForgotPasswordResponse } from '../types/auth.types';

export const useForgotPassword = () => {
    return useMutation<ForgotPasswordResponse, Error, ForgotPasswordRequest>({
        mutationFn: (data: ForgotPasswordRequest) => authService.forgotPassword(data),
        onSuccess: () => {
            console.log('✅ Временный пароль отправлен на email пользователя');
        },
        onError: (error: Error) => {
            console.error('❌ Ошибка сброса пароля:', error.message);
        }
    });
};