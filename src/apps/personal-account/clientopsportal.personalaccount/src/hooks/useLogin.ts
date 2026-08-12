import { userManager } from '../auth/oidc';

export const useLogin = () => {
    const login = async (): Promise<void> => {
        await userManager.signinRedirect();
    };

    return { login };
};
