import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { LoginPage } from './pages/LoginPage';
import { SetPasswordPage } from './pages/SetPasswordPage';
import { ForgotPasswordPage } from './pages/ForgotPasswordPage';
import { PersonalAccountPage } from './pages/PersonalAccountPage';

const PrivateRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const token = localStorage.getItem('auth_token');
    return token ? <>{children}</> : <Navigate to="/login" replace />;
};

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/set-password" element={<SetPasswordPage />} />
                <Route path="/forgot-password" element={<ForgotPasswordPage />} />

                <Route
                    path="/personal-account"
                    element={
                        <PrivateRoute>
                            <PersonalAccountPage />
                        </PrivateRoute>
                    }
                />

                <Route path="/" element={<Navigate to="/login" replace />} />
                <Route path="*" element={<div>Страница не найдена</div>} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;