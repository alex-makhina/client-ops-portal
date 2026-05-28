import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useSetPassword } from '../hooks/useSetPassword';
import { setPasswordSchema, type SetPasswordFormData } from '../schemas/auth.schema';
import './SetPasswordForm.css';

export const SetPasswordForm: React.FC = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const userId = searchParams.get('userId');
    const token = searchParams.get('token');

    const { mutate, isPending, isError, error, isSuccess } = useSetPassword();

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<SetPasswordFormData>({
        resolver: zodResolver(setPasswordSchema),
        mode: 'onTouched'
    });

    const onSubmit = (data: SetPasswordFormData) => {
        if (!userId || !token) {
            return;
        }

        mutate({
            userId,
            token,
            newPassword: data.newPassword
        }, {
            onSuccess: () => {
                setTimeout(() => navigate('/login'), 3000);
            }
        });
    };

    const getErrorMessage = (err: unknown): string => {
        if (err instanceof Error) {
            const axiosErr = err as {
                response?: {
                    data?: {
                        message?: string;
                        error?: string;
                    };
                    status?: number;
                };
                request?: unknown;
            };
            return (
                axiosErr.response?.data?.message ||
                axiosErr.response?.data?.error ||
                err.message
            );
        }
        return "Произошла ошибка при установке пароля";
    };

    if (isSuccess) {
        return (
            <div className="set-password-container">
                <div className="set-password-card">
                    <div className="success-container">
                        <div className="success-icon">
                            <svg
                                fill="none"
                                stroke="currentColor"
                                viewBox="0 0 24 24"
                                xmlns="http://www.w3.org/2000/svg"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={3}
                                    d="M5 13l4 4L19 7"
                                />
                            </svg>
                        </div>
                        <h2 className="success-title">Пароль установлен!</h2>
                        <p className="success-text">
                            Ваш пароль успешно установлен. Теперь вы можете войти в систему.
                        </p>
                        <div className="redirect-timer">
                            <svg
                                width="20"
                                height="20"
                                fill="none"
                                stroke="currentColor"
                                viewBox="0 0 24 24"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={2}
                                    d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                                />
                            </svg>
                            <span>Перенаправление...</span>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="set-password-container">
            <div className="set-password-card">
                <div className="set-password-header">
                    <h1 className="set-password-title">Установка пароля</h1>
                    <p className="set-password-subtitle">
                        Придумайте надежный пароль для вашего аккаунта
                    </p>
                </div>

                {(!userId || !token) && (
                    <div className="invalid-link">
                        ⚠️ Ссылка для сброса некорректна. Проверьте URL.
                    </div>
                )}

                {isError && (
                    <div className="error-message">
                        <svg
                            width="20"
                            height="20"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                        >
                            <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                            />
                        </svg>
                        <span>{getErrorMessage(error)}</span>
                    </div>
                )}

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="form-group">
                        <label className="form-label">Новый пароль</label>
                        <input
                            type="password"
                            {...register('newPassword')}
                            className={`form-input ${errors.newPassword ? 'error' : ''}`}
                            placeholder="Введите новый пароль"
                            disabled={isPending || !userId || !token}
                        />
                        {errors.newPassword && (
                            <div className="field-error">
                                <svg
                                    width="16"
                                    height="16"
                                    fill="none"
                                    stroke="currentColor"
                                    viewBox="0 0 24 24"
                                >
                                    <path
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        strokeWidth={2}
                                        d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                                    />
                                </svg>
                                {errors.newPassword.message}
                            </div>
                        )}
                    </div>

                    <div className="form-group">
                        <label className="form-label">Подтвердите пароль</label>
                        <input
                            type="password"
                            {...register('confirmPassword')}
                            className={`form-input ${errors.confirmPassword ? 'error' : ''}`}
                            placeholder="Повторите пароль"
                            disabled={isPending || !userId || !token}
                        />
                        {errors.confirmPassword && (
                            <div className="field-error">
                                <svg
                                    width="16"
                                    height="16"
                                    fill="none"
                                    stroke="currentColor"
                                    viewBox="0 0 24 24"
                                >
                                    <path
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        strokeWidth={2}
                                        d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                                    />
                                </svg>
                                {errors.confirmPassword.message}
                            </div>
                        )}
                    </div>

                    <button
                        type="submit"
                        disabled={isPending || !userId || !token}
                        className={`submit-button ${isPending ? 'loading' : ''}`}
                    >
                        {isPending && <div className="loading-spinner"></div>}
                        Установить пароль
                    </button>
                </form>
            </div>
        </div>
    );
};