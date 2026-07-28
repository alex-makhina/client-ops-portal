import { apiClient } from './client';
import type { ContractCreate, ContractTerminate } from '../types/contract.types';

export const contractService = {
    async create(data: ContractCreate) {
        const res = await apiClient.post('/Contracts', data);
        return res.data;
    },

    async terminate(id: string, data: ContractTerminate) {
        const res = await apiClient.put(`/Contracts/${id}`, data);
        return res.data;
    }
};