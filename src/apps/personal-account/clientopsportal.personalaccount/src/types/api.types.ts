export interface ApiErrorResponse {
    message?: string;
    error?: string;
    statusCode?: number;
    [key: string]: unknown;
}

export type ApiError = Error & {
    response?: {
        data?: ApiErrorResponse;
        status?: number;
    };
};