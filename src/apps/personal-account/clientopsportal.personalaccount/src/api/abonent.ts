import { apiClient } from './client';
import type {
    Abonent,
    ContractShortData,
    SubscriptionFullData
} from '../types/abonent.types';

export const abonentService = {
    async getAbonentIdByUserId(userId: string): Promise<string> {
        const response = await apiClient.get(`/Abonents/by-user-id/${userId}`);
        return response.data;
    },

    async getAbonent(id: string): Promise<Abonent> {
        const res = await apiClient.get<Abonent>(`/Abonents/${id}`);
        return res.data;
    },

    async getContracts(abonentId: string): Promise<ContractShortData[]> {
        const res = await apiClient.get<ContractShortData[]>(
            `/Contracts/by-abonent/${abonentId}`
        );
        return res.data;
    },

    async getSubscriptions(
        abonentId: string,
        onlyActive: boolean = true
    ): Promise<SubscriptionFullData[]> {
        const res = await apiClient.get<SubscriptionFullData[]>(
            `/Subscriptions/by-abonent/${abonentId}`,
            { params: { onlyActive } }
        );
        return res.data;
    },
};