import { apiClient } from './client';
import type {
    SubscriptionCreate,
    ServiceShort,
    TariffPlanShort
} from '../types/subscription.types';

export const subscriptionService = {
    async getActiveServices() {
        const res = await apiClient.get<ServiceShort[]>('/Services/active');
        return res.data;
    },
    async getTariffPlansByService(serviceId: string) {
        const res = await apiClient.get<TariffPlanShort[]>(`/TariffPlans/by-service/active/${serviceId}`);
        return res.data;
    },

    async connect(data: SubscriptionCreate) {
        return await apiClient.post('/Subscriptions/connect', data);
    },
    async changeTariff(subscriptionId: string, newTariffPlanId: string) {
        return await apiClient.patch(`/Subscriptions/${subscriptionId}/change-tariff`, null, {
            params: { newTariffPlanId }
        });
    },
    async cancel(subscriptionId: string) {
        return await apiClient.patch(`/Subscriptions/${subscriptionId}/cancel`);
    }
};