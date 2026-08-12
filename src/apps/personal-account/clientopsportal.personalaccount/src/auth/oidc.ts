import { UserManager, WebStorageStateStore, type UserManagerSettings } from 'oidc-client-ts';

const baseUrl = window.location.origin;

export const oidcConfig: UserManagerSettings = {
    authority: 'http://localhost:5110',
    client_id: 'personal-account',
    redirect_uri: `${baseUrl}/auth/callback`,
    post_logout_redirect_uri: 'http://localhost:5110/LoggedOut?returnUrl=http://localhost:62000/',
    response_type: 'code',
    scope: 'openid profile roles api',
    automaticSilentRenew: true,
    includeIdTokenInSilentRenew: true,
    userStore: new WebStorageStateStore({ store: window.sessionStorage }),
};

export const userManager = new UserManager(oidcConfig);

export const getAccessToken = async (): Promise<string | null> => {
    const user = await userManager.getUser();
    return user?.access_token ?? null;
};
