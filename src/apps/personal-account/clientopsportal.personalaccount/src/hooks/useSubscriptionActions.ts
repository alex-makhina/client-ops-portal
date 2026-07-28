import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { subscriptionService } from '../api/subscription';

export const useServices = () => useQuery({
    queryKey: ['services'],
    queryFn: subscriptionService.getActiveServices,
    staleTime: 1000 * 60 * 5,
});

export const useTariffPlans = (serviceId?: string) => useQuery({
    queryKey: ['tariffPlans', serviceId],
    queryFn: () => serviceId ? subscriptionService.getTariffPlansByService(serviceId) : Promise.resolve([]),
    enabled: !!serviceId,
    staleTime: 1000 * 60 * 5,
});

export const useConnectSubscription = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: subscriptionService.connect,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['contracts'] });
            queryClient.invalidateQueries({ queryKey: ['subscriptions'] });
        },
    });
};

export const useChangeTariff = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ subId, newTariffPlanId }: { subId: string; newTariffPlanId: string }) =>
            subscriptionService.changeTariff(subId, newTariffPlanId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['contracts'] });
            queryClient.invalidateQueries({ queryKey: ['subscriptions'] });
        },
    });
};

export const useCancelSubscription = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: subscriptionService.cancel,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['contracts'] });
            queryClient.invalidateQueries({ queryKey: ['subscriptions'] });
        },
    });
};