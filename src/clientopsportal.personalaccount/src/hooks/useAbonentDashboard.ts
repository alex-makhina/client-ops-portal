import { useQuery } from '@tanstack/react-query';
import { abonentService } from '../api/abonent';
import axios from 'axios';

const getAbonentId = (): string => {
    return localStorage.getItem('abonent_id') || '';
};

export const useAbonentDashboard = () => {
    const abonentId = getAbonentId();

    const abonentQuery = useQuery({
        queryKey: ['abonent', abonentId],
        queryFn: () => abonentService.getAbonent(abonentId),
        enabled: !!abonentId,
    });

    const contractsQuery = useQuery({
        queryKey: ['contracts', abonentId],
        queryFn: async () => {
            try {
                return await abonentService.getContracts(abonentId);
            } catch (error) {
                if (axios.isAxiosError(error) && (error.response?.status === 404 || error.response?.status === 500)) {
                    return [];
                }
                throw error;
            }
        },
        enabled: !!abonentId,
    });

    const subscriptionsQuery = useQuery({
        queryKey: ['subscriptions', abonentId],
        queryFn: async () => {
            try {
                return await abonentService.getSubscriptions(abonentId);
            } catch (error) {
                if (axios.isAxiosError(error) && (error.response?.status === 404 || error.response?.status === 500)) {
                    return [];
                }
                throw error;
            }
        },
        enabled: !!abonentId,
    });

    return {
        abonent: abonentQuery.data,
        contracts: contractsQuery.data || [],
        subscriptions: subscriptionsQuery.data || [],
        isLoading: abonentQuery.isLoading || contractsQuery.isLoading || subscriptionsQuery.isLoading,
        isError: abonentQuery.isError,
        error: abonentQuery.error,
        refetch: () => {
            abonentQuery.refetch();
            contractsQuery.refetch();
            subscriptionsQuery.refetch();
        }
    };
};