import { z } from 'zod';

export const loginSchema = z.object({
    login: z.string()
        .min(1, 'Введите логин')
        .trim(),
    password: z.string()
        .min(1, 'Введите пароль')
        .min(8, 'Пароль должен содержать минимум 8 символов'),
});

export type LoginFormData = z.infer<typeof loginSchema>;

export const forgotPasswordSchema = z.object({
    loginIdentifier: z.string()
        .min(1, 'Введите логин')
        .trim(),
});

export type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>;

export const setPasswordSchema = z.object({
    newPassword: z.string()
        .min(8, 'Пароль должен содержать минимум 8 символов')
        .regex(/[A-Z]/, 'Пароль должен содержать хотя бы одну заглавную букву')
        .regex(/[0-9]/, 'Пароль должен содержать хотя бы одну цифру'),
    confirmPassword: z.string(),
}).refine((data) => data.newPassword === data.confirmPassword, {
    message: "Пароли не совпадают",
    path: ["confirmPassword"],
});

export type SetPasswordFormData = z.infer<typeof setPasswordSchema>;