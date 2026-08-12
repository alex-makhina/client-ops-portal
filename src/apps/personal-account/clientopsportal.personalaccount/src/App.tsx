import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { LoginPage } from './pages/LoginPage';
import { PersonalAccountPage } from './pages/PersonalAccountPage';
import { AuthCallbackPage } from './pages/AuthCallbackPage';
import { useEffect, useRef, useState } from 'react';
import { userManager } from './auth/oidc';

const useAuthState = () => {
    const [checked, setChecked] = useState(false);
    const [authenticated, setAuthenticated] = useState(false);
    const redirecting = useRef(false);

    useEffect(() => {
        userManager.getUser().then((user) => {
            const isAuth = !!user && !user.expired;
            setAuthenticated(isAuth);
            setChecked(true);

            if (!isAuth && !redirecting.current) {
                redirecting.current = true;
                userManager.signinRedirect();
            }
        });
    }, []);

    return { checked, authenticated };
};

const PrivateRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const { checked, authenticated } = useAuthState();

    if (!checked) {
        return <div style={{ textAlign: 'center', padding: 60 }}>Загрузка...</div>;
    }

    if (!authenticated) {
        return null;
    }

    return <>{children}</>;
};

const RootRedirect: React.FC = () => {
    const { checked, authenticated } = useAuthState();

    if (!checked) {
        return <div style={{ textAlign: 'center', padding: 60 }}>Загрузка...</div>;
    }

    if (!authenticated) {
        return null;
    }

    return <Navigate to="/personal-account" replace />;
};

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/auth/callback" element={<AuthCallbackPage />} />

                <Route
                    path="/personal-account"
                    element={
                        <PrivateRoute>
                            <PersonalAccountPage />
                        </PrivateRoute>
                    }
                />

                <Route path="/" element={<RootRedirect />} />
                <Route path="*" element={<div>Страница не найдена</div>} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;
