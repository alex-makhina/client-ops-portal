import { useMutation } from '@tanstack/react-query';
import { authService } from '../api/auth';
import type { SetPasswordRequest } from '../types/auth.types';
import type { ApiError } from '../types/api.types';

export const useSetPassword = () => {
    return useMutation({
        mutationFn: (data: SetPasswordRequest) => authService.setPassword(data),
        onSuccess: () => {
            console.log("Пароль успешно установлен");
        },
        onError: (error: ApiError) => {
            const message = error.response?.data?.message || error.message;
            console.error("Ошибка:", message);
        }
    });
};