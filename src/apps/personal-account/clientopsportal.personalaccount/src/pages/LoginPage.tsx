import React, { useEffect, useRef } from 'react';
import { useLogin } from '../hooks/useLogin';
import '../components/Login.css';

const AUTH_BASE = 'http://localhost:5110';

export const LoginPage: React.FC = () => {
    const { login } = useLogin();
    const redirected = useRef(false);

    useEffect(() => {
        if (!redirected.current) {
            redirected.current = true;
            login().catch((err) => {
                console.error('❌ Не удалось начать вход:', err);
            });
        }
    }, [login]);

    return (
        <div className="set-password-container">
            <div className="login-redirect">
                <div className="loading-spinner"></div>
                <p className="text-muted mt-3">Перенаправление на страницу авторизации...</p>
                <div style={{ marginTop: '12px' }}>
                    <a
                        href={`${AUTH_BASE}/ForgotPassword?returnUrl=${encodeURIComponent('http://localhost:62000/')}`}
                        style={{
                            color: '#667eea',
                            textDecoration: 'none',
                            fontSize: '14px',
                            fontWeight: '500'
                        }}
                    >
                        Забыли пароль?
                    </a>
                </div>
            </div>
        </div>
    );
};
