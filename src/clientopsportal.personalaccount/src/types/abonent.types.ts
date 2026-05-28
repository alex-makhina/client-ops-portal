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
    contractNumber: string;
    abonentId: string;
    beginDate: string;
}

export interface SubscriptionFullData {
    contractId: string;
    serviceId: string;
    serviceName: string;
    tariffPlanId: string;
    tariffPlanName: string;
    beginDate: string;
    endDate?: string;
}