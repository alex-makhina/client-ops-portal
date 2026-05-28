import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate, Link } from 'react-router-dom';
import { useLogin } from '../hooks/useLogin';
import { loginSchema, type LoginFormData } from '../schemas/auth.schema';
import './SetPasswordForm.css'; 

export const LoginForm: React.FC = () => {
    const navigate = useNavigate();
    const [showPassword, setShowPassword] = useState(false);

    const { mutate, isPending, isError, error } = useLogin();

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<LoginFormData>({
        resolver: zodResolver(loginSchema),
        mode: 'onTouched'
    });

    const onSubmit = (data: LoginFormData) => {
        mutate(data, {
            onSuccess: () => {
                navigate('/personal-account', { replace: true });
            }
        });
    };

    const getErrorMessage = (err: unknown): string => {
        if (err instanceof Error) {
            const axiosErr = err as {
                response?: {
                    data?: { message?: string; errors?: string[] };
                    status?: number;
                };
            };
            if (axiosErr.response?.status === 401) {
                return 'Неверный логин или пароль';
            }
            return axiosErr.response?.data?.message || err.message;
        }
        return 'Произошла ошибка при входе';
    };

    return (
        <div className="set-password-container">
            <div className="set-password-card">
                <div className="set-password-header">
                    <h1 className="set-password-title">Вход в систему</h1>
                    <p className="set-password-subtitle">
                        Введите ваши учетные данные для доступа к личному кабинету
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
                            {...register('login')}
                            className={`form-input ${errors.login ? 'error' : ''}`}
                            placeholder="Введите ваш логин"
                            disabled={isPending}
                            autoComplete="username"
                        />
                        {errors.login && (
                            <div className="field-error">
                                <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                        d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                {errors.login.message}
                            </div>
                        )}
                    </div>

                    <div className="form-group">
                        <label className="form-label">Пароль</label>
                        <div style={{ position: 'relative' }}>
                            <input
                                type={showPassword ? 'text' : 'password'}
                                {...register('password')}
                                className={`form-input ${errors.password ? 'error' : ''}`}
                                placeholder="Введите ваш пароль"
                                disabled={isPending}
                                autoComplete="current-password"
                                style={{ paddingRight: '40px' }}
                            />
                            <button
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                                style={{
                                    position: 'absolute',
                                    right: '12px',
                                    top: '50%',
                                    transform: 'translateY(-50%)',
                                    background: 'none',
                                    border: 'none',
                                    color: '#718096',
                                    cursor: 'pointer',
                                    padding: '4px',
                                    fontSize: '14px'
                                }}
                                tabIndex={-1}
                            >
                                {showPassword ? '🙈' : '👁️'}
                            </button>
                        </div>
                        {errors.password && (
                            <div className="field-error">
                                <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                        d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                {errors.password.message}
                            </div>
                        )}
                    </div>

                    <div style={{ textAlign: 'right', marginBottom: '24px' }}>
                        <Link
                            to="/forgot-password"
                            style={{
                                color: '#667eea',
                                textDecoration: 'none',
                                fontSize: '14px',
                                fontWeight: '500'
                            }}
                        >
                            Забыли пароль?
                        </Link>
                    </div>

                    <button
                        type="submit"
                        disabled={isPending}
                        className={`submit-button ${isPending ? 'loading' : ''}`}
                    >
                        {isPending && <div className="loading-spinner"></div>}
                        Войти
                    </button>
                </form>
            </div>
        </div>
    );
};