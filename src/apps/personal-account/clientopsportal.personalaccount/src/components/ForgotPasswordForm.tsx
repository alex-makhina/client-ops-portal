import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from 'react-router-dom';
import { useForgotPassword } from '../hooks/useForgotPassword';
import { forgotPasswordSchema, type ForgotPasswordFormData } from '../schemas/auth.schema';
import './Login.css';

export const ForgotPasswordForm: React.FC = () => {
    const { mutate, isPending, isError, error, isSuccess } = useForgotPassword();

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<ForgotPasswordFormData>({
        resolver: zodResolver(forgotPasswordSchema),
        mode: 'onTouched'
    });

    const onSubmit = (data: ForgotPasswordFormData) => {
        mutate(data);
    };

    const getErrorMessage = (err: unknown): string => {
        if (err instanceof Error) {
            const axiosErr = err as {
                response?: {
                    data?: { message?: string };
                    status?: number;
                };
            };

            if (axiosErr.response?.status === 404) {
                return "Пользователь не найден";
            }

            return axiosErr.response?.data?.message || err.message;
        }
        return "Произошла ошибка при восстановлении пароля";
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
                                    d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"
                                />
                            </svg>
                        </div>
                        <h2 className="success-title">Проверьте email</h2>
                        <p className="success-text">
                            Временный пароль отправлен на вашу электронную почту.
                        </p>
                        <div style={{
                            background: '#f0fff4',
                            padding: '16px',
                            borderRadius: '8px',
                            border: '1px solid #68d391',
                            marginBottom: '24px'
                        }}>
                            <p style={{ margin: 0, color: '#276749', fontSize: '14px' }}>
                                💡 <strong>Совет:</strong> Проверьте папку "Спам", если письмо не пришло в течение 5 минут
                            </p>
                        </div>
                        <Link
                            to="/login"
                            className="submit-button"
                            style={{ textDecoration: 'none', display: 'inline-block', textAlign: 'center' }}
                        >
                            Вернуться ко входу
                        </Link>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="set-password-container">
            <div className="set-password-card">
                <div className="set-password-header">
                    <h1 className="set-password-title">Восстановление пароля</h1>
                    <p className="set-password-subtitle">
                        Введите ваш логин, и мы отправим временный пароль на ваш email
                    </p>
                </div>

                {isError && (
                    <div className="error-message">
                        <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <span>{getErrorMessage(error)}</span>
                    </div>
                )}

                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="form-group">
                        <label className="form-label">Логин</label>
                        <input
                            type="text"
                            {...register('loginIdentifier')}
                            className={`form-input ${errors.loginIdentifier ? 'error' : ''}`}
                            placeholder="Введите ваш логин"
                            disabled={isPending}
                            autoComplete="username"
                        />
                        {errors.loginIdentifier && (
                            <div className="field-error">
                                <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                        d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                {errors.loginIdentifier.message}
                            </div>
                        )}
                    </div>

                    <div style={{
                        background: '#f7fafc',
                        padding: '16px',
                        borderRadius: '8px',
                        marginBottom: '24px',
                        fontSize: '14px',
                        color: '#718096'
                    }}>
                        <p style={{ margin: '0 0 8px 0' }}>
                            📧 Временный пароль будет отправлен на email, привязанный к аккаунту
                        </p>
                        <p style={{ margin: 0 }}>
                            🔒 После входа с временным паролем вы сможете установить свой постоянный пароль
                        </p>
                    </div>

                    <button
                        type="submit"
                        disabled={isPending}
                        className={`submit-button ${isPending ? 'loading' : ''}`}
                    >
                        {isPending && <div className="loading-spinner"></div>}
                        Отправить временный пароль
                    </button>
                </form>

                <div style={{ textAlign: 'center', marginTop: '24px', fontSize: '14px', color: '#718096' }}>
                    Вспомнили пароль?{' '}
                    <Link
                        to="/login"
                        style={{ color: '#667eea', textDecoration: 'none', fontWeight: '500' }}
                    >
                        Войти в систему
                    </Link>
                </div>
            </div>
        </div>
    );
};