export interface SubscriptionCreate {
    contractId: string;
    serviceId: string;
    tariffPlanId: string;
    beginDate: string;
    endDate?: string;
}

export interface ChangeTariff {
    newTariffPlanId: string;
}

export interface ServiceShort {
    id: string;
    name: string;
    description: string;
    beginDate: string;
    endDate?: string;
    isActive: boolean;
}

export interface TariffPlanShort {
    id: string;
    name: string;
    price: number;
}