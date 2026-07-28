import { useMutation, useQueryClient } from '@tanstack/react-query';
import { contractService } from '../api/contract';
import type { ContractCreate } from '../types/contract.types';
import { useAbonentDashboard } from './useAbonentDashboard';

export const useCreateContract = () => {
    const queryClient = useQueryClient();
    const { abonent } = useAbonentDashboard();

    return useMutation({
        mutationFn: (data: Omit<ContractCreate, 'abonentId'>) =>
            contractService.create({ ...data, abonentId: abonent!.id }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['contracts'] });
        }
    });
};

export const useTerminateContract = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ id, endDate }: { id: string; endDate: string }) =>
            contractService.terminate(id, { endDate }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['contracts'] });
        }
    });
};