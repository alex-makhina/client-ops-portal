export interface Abonent {
    id: string;
    userId: string;
    identificationNumber: string;
    firstName: string;
    lastName: string;
    middleName?: string;
    accountNumber: string;
    createdAt: string;
    createdBy?: string;
    updatedAt?: string;
    updatedBy?: string;
}

export interface ContractShortData {
    id: string;
    contractNumber: string;
    abonentId: string;
    beginDate: string;
    endDate: string;
}

export interface SubscriptionFullData {
    id: string;
    contractId: string;
    serviceId: string;
    serviceName: string;
    tariffPlanId: string;
    tariffPlanName: string;
    beginDate: string;
    endDate?: string;
}