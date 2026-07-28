import React from 'react';
import { SetPasswordForm } from '../components/SetPasswordForm';

export const SetPasswordPage: React.FC = () => {
    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
            <SetPasswordForm />
        </div>
    );
};