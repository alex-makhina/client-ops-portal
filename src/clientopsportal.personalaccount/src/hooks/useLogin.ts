import { useMutation } from '@tanstack/react-query';
import { authService } from '../api/auth';
import { abonentService } from '../api/abonent';
import type { LoginRequest, AuthResponse } from '../types/auth.types';

export const useLogin = () => {
    return useMutation<AuthResponse, Error, LoginRequest>({
        mutationFn: (data: LoginRequest) => authService.login(data),

        onSuccess: async (data) => {
            localStorage.setItem('auth_token', data.token);
            localStorage.setItem('user_id', data.userId);
            localStorage.setItem('user_name', data.userName || '');
            localStorage.setItem('user_roles', JSON.stringify(data.roles));

            try {
                const abonentId = await abonentService.getAbonentIdByUserId(data.userId);
                localStorage.setItem('abonent_id', abonentId);
            } catch (err) {
                console.error('❌ Не удалось получить AbonentId:', err);

                localStorage.removeItem('auth_token');
                localStorage.removeItem('user_id');
                localStorage.removeItem('user_name');
                localStorage.removeItem('user_roles');

                throw new Error('Не удалось загрузить профиль абонента. Пожалуйста, обратитесь в службу поддержки.', {
                    cause: err
                });
            }
        },
        onError: (error: Error) => {
            console.error('❌ Вход отменен:', error.message);
        }
    });
};