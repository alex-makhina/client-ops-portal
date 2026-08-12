import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { userManager } from '../auth/oidc';

export const AuthCallbackPage: React.FC = () => {
    const navigate = useNavigate();
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        userManager.signinRedirectCallback()
            .then(async () => {
                const user = await userManager.getUser();
                if (!user) {
                    navigate('/login', { replace: true });
                    return;
                }

                try {
                    const abonentModule = await import('../api/abonent');
                    const abonentId = await abonentModule.abonentService.getAbonentIdByUserId(user.profile.sub ?? '');
                    localStorage.setItem('abonent_id', abonentId);
                } catch (err) {
                    console.error('❌ Не удалось получить AbonentId:', err);
                    await userManager.signoutRedirect();
                    setError('Не удалось загрузить профиль абонента. Пожалуйста, обратитесь в службу поддержки.');
                    return;
                }

                navigate('/personal-account', { replace: true });
            })
            .catch((err) => {
                console.error('❌ Ошибка OIDC callback:', err);
                setError('Ошибка авторизации. Попробуйте ещё раз.');
            });
    }, [navigate]);

    if (error) {
        return (
            <div className="dashboard-wrapper">
                <div className="error-message" style={{ maxWidth: 420, margin: '80px auto', padding: 24, background: 'rgba(255,255,255,0.1)', borderRadius: 12 }}>
                    {error}
                    <a href="/login" style={{ display: 'block', marginTop: 16, color: '#fff', textDecoration: 'underline' }}>
                        Вернуться ко входу
                    </a>
                </div>
            </div>
        );
    }

    return (
        <div className="dashboard-wrapper">
            <div style={{ textAlign: 'center', padding: 60, color: '#fff' }}>
                <div className="loading-spinner" style={{ display: 'inline-block' }}></div>
            </div>
        </div>
    );
};
