import { z } from 'zod';

export const contractCreateSchema = z.object({
    contractNumber: z.string()
        .min(1, 'Введите номер договора')
        .max(50, 'Максимум 50 символов')
        .trim(),
    beginDate: z.string().min(1, 'Выберите дату начала'),
    endDate: z.string().optional().or(z.literal('')),
});

export type ContractCreateFormData = z.infer<typeof contractCreateSchema>;