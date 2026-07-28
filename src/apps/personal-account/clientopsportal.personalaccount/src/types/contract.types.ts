export interface ContractShort {
    id: string;
    contractNumber: string;
    abonentId: string;
    beginDate: string;
    endDate?: string;
}

export interface ContractCreate {
    contractNumber: string;
    abonentId: string;
    beginDate: string;
    endDate?: string;
}

export interface ContractTerminate {
    endDate: string;
}