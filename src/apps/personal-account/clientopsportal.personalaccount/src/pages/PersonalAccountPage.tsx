import React, { useState, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useAbonentDashboard } from '../hooks/useAbonentDashboard';
import { useCreateContract, useTerminateContract } from '../hooks/useContract';
import { useConnectSubscription, useChangeTariff, useCancelSubscription, useServices, useTariffPlans } from '../hooks/useSubscriptionActions';
import { contractCreateSchema, type ContractCreateFormData } from '../schemas/contract.schema';
import { SubscriptionRow } from '../components/SubscriptionRow';
import { userManager } from '../auth/oidc';
import './PersonalAccount.css';

export const PersonalAccountPage: React.FC = () => {
    const { abonent, contracts, subscriptions, isLoading, isError, error } = useAbonentDashboard();

    const [expandedContractId, setExpandedContractId] = useState<string | null>(null);
    const [showCreateForm, setShowCreateForm] = useState(false);
    const [activeAction, setActiveAction] = useState<{ type: 'connect' | 'changeTariff' | null; contractId?: string; subId?: string }>({ type: null });
    const [selectedServiceId, setSelectedServiceId] = useState<string>('');

    const { mutate: createContractMutate, isPending: isCreatingContract } = useCreateContract();
    const { mutate: terminateMutate, isPending: isTerminatingContract } = useTerminateContract();
    const { mutate: connectMutate, isPending: isConnecting } = useConnectSubscription();
    const { mutate: changeTariffMutate, isPending: isChangingTariff } = useChangeTariff();
    const { mutate: cancelMutate, isPending: isCancelling } = useCancelSubscription();

    const { data: services } = useServices();
    const { data: tariffPlans } = useTariffPlans(selectedServiceId || undefined);

    const { register: registerContract, handleSubmit: handleSubmitContract, reset: resetContract, formState: { errors: contractErrors } } = useForm<ContractCreateFormData>({
        resolver: zodResolver(contractCreateSchema),
        defaultValues: { endDate: '' }
    });

    const handleLogout = () => {
        localStorage.clear();
        userManager.signoutRedirect();
    };

    const toggleExpand = (id: string) => {
        setExpandedContractId(prev => prev === id ? null : id);
        setActiveAction({ type: null });
    };

    const onCreateContractSubmit = (data: ContractCreateFormData) => {
        createContractMutate({
            contractNumber: data.contractNumber,
            beginDate: new Date(data.beginDate).toISOString(),
            endDate: data.endDate ? new Date(data.endDate).toISOString() : undefined,
        }, { onSuccess: () => { setShowCreateForm(false); resetContract(); } });
    };

    const handleTerminateContract = (e: React.MouseEvent, contractId: string) => {
        e.stopPropagation();
        if (!window.confirm('Расторгнуть договор?')) return;
        terminateMutate({ id: contractId, endDate: new Date().toISOString() });
    };

    const handleConnectSubmit = (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        const form = e.target as HTMLFormElement;
        const fd = new FormData(form);

        const serviceId = fd.get('serviceId') as string;
        const tariffPlanId = fd.get('tariffPlanId') as string;
        const beginDateStr = fd.get('beginDate') as string;
        const endDateStr = fd.get('endDate') as string;

        if (!activeAction.contractId || !serviceId || !tariffPlanId || !beginDateStr) return;

        connectMutate({
            contractId: activeAction.contractId,
            serviceId,
            tariffPlanId,
            beginDate: new Date(beginDateStr).toISOString(),
            endDate: endDateStr ? new Date(endDateStr).toISOString() : undefined,
        }, {
            onSuccess: () => {
                setActiveAction({ type: null });
                setSelectedServiceId('');
                form.reset();
            }
        });
    };

    const handleChangeTariffSubmit = (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        const fd = new FormData(e.target as HTMLFormElement);
        const newTariffPlanId = fd.get('newTariffPlanId') as string;

        if (!activeAction.subId || !newTariffPlanId) return;

        changeTariffMutate({
            subId: activeAction.subId,
            newTariffPlanId
        }, { onSuccess: () => setActiveAction({ type: null }) });
    };

    const handleCancelSubscription = (subId: string) => {
        if (!window.confirm('Отключить услугу? Это действие необратимо.')) return;
        cancelMutate(subId);
    };

    const groupedContracts = useMemo(() => {
        if (!contracts || !subscriptions) return [];
        return contracts.map(c => ({ ...c, subs: subscriptions.filter(s => s.contractId === c.id) }));
    }, [contracts, subscriptions]);

    const activeContract = useMemo(() =>
        contracts?.find(c => c.id === activeAction.contractId),
        [contracts, activeAction.contractId]);

    if (isLoading) return <div className="dashboard-wrapper"><div style={{ textAlign: 'center', padding: '60px', color: '#fff' }}>Загрузка...</div></div>;
    if (isError) return <div className="dashboard-wrapper"><div style={{ textAlign: 'center', padding: '60px', color: '#fff' }}>{(error as Error).message}</div></div>;
    if (!abonent) return null;

    return (
        <div className="dashboard-wrapper">
            <div className="dashboard-header-bg">
                <div className="header-content">
                    <h1 className="dashboard-title">Личный кабинет</h1>
                    <button onClick={handleLogout} className="btn-logout">
                        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                        </svg>
                        Выйти
                    </button>
                </div>
            </div>

            <div className="dashboard-container">

                <div className="section-card">
                    <div className="section-header">
                        <h2 className="section-title">Данные абонента</h2>
                    </div>
                    <div className="profile-grid">
                        <div className="profile-item">
                            <div className="profile-label">ФИО</div>
                            <div className="profile-value">{[abonent.lastName, abonent.firstName, abonent.middleName].filter(Boolean).join(' ')}</div>
                        </div>
                        <div className="profile-item">
                            <div className="profile-label">Лицевой счет</div>
                            <div className="profile-value" style={{ color: '#667eea', fontSize: '18px' }}>{abonent.accountNumber}</div>
                        </div>
                        <div className="profile-item">
                            <div className="profile-label">Идентификатор</div>
                            <div className="profile-value">{abonent.identificationNumber}</div>
                        </div>
                        <div className="profile-item">
                            <div className="profile-label">Клиент с</div>
                            <div className="profile-value">{new Date(abonent.createdAt).toLocaleDateString('ru-RU')}</div>
                        </div>
                    </div>
                </div>

                <div className="section-card">
                    <div className="section-header">
                        <h2 className="section-title">Договоры и услуги</h2>
                        {!showCreateForm && (
                            <button onClick={() => setShowCreateForm(true)} className="btn-primary">
                                + Новый договор
                            </button>
                        )}
                    </div>

                    {showCreateForm && (
                        <div className="action-panel" style={{ marginBottom: 24 }}>
                            <h4 style={{ margin: '0 0 16px', fontSize: 15, color: 'var(--text-primary)' }}>Новый договор</h4>
                            <form onSubmit={handleSubmitContract(onCreateContractSubmit)}>
                                <div className="action-row">
                                    <div>
                                        <label className="action-label">Номер договора</label>
                                        <input {...registerContract('contractNumber')} className="form-input" placeholder="№ Д-2024-XX" />
                                        {contractErrors.contractNumber && <div style={{ color: '#dc2626', fontSize: 11, marginTop: 4 }}>{contractErrors.contractNumber.message}</div>}
                                    </div>
                                    <div>
                                        <label className="action-label">Дата начала</label>
                                        <input type="date" {...registerContract('beginDate')} className="form-input" />
                                    </div>
                                    <div>
                                        <label className="action-label">Дата окончания</label>
                                        <input type="date" {...registerContract('endDate')} className="form-input" />
                                    </div>
                                </div>

                                <div className="action-btn-group">
                                    <button type="button" className="btn-secondary" onClick={() => setShowCreateForm(false)}>
                                        Отмена
                                    </button>
                                    <button type="submit" className="btn-primary" disabled={isCreatingContract}>
                                        {isCreatingContract ? 'Создание...' : 'Создать договор'}
                                    </button>
                                </div>
                            </form>
                        </div>
                    )}

                    <div className="contract-list">
                        {groupedContracts.map(contract => {
                            const isTerminated = contract.endDate && new Date(contract.endDate) <= new Date();
                            const isExpanded = expandedContractId === contract.id;
                            const isConnectingToThis = activeAction.type === 'connect' && activeAction.contractId === contract.id;

                            return (
                                <div key={contract.id} className={`contract-row ${isExpanded ? 'expanded' : ''}`}>
                                    <div className="contract-main" onClick={() => toggleExpand(contract.id)}>
                                        <div className="contract-info">
                                            <span className="contract-number">{contract.contractNumber}</span>
                                            <span className={`badge ${isTerminated ? 'badge-terminated' : 'badge-active'}`}>{isTerminated ? 'Расторгнут' : 'Активен'}</span>
                                            <span className="contract-date">
                                                {new Date(contract.beginDate).toLocaleDateString('ru-RU')} — {contract.endDate ? new Date(contract.endDate).toLocaleDateString('ru-RU') : 'бессрочно'}
                                            </span>
                                        </div>
                                        <div className="contract-meta">
                                            <span className="subs-badge">Услуг: {contract.subs.length}</span>
                                            <span className="expand-icon">▼</span>
                                            {!isTerminated && <button className="btn-danger" onClick={(e) => handleTerminateContract(e, contract.id)} disabled={isTerminatingContract}>Расторгнуть</button>}
                                        </div>
                                    </div>

                                    {isExpanded && (
                                        <div className="contract-subs">

                                            {isConnectingToThis && (
                                                <form onSubmit={handleConnectSubmit} className="action-panel" style={{ margin: '8px 12px 12px' }}>
                                                    <h4 style={{ margin: '0 0 12px', fontSize: 14 }}>Подключение новой услуги</h4>
                                                    <div className="action-row">
                                                        <div>
                                                            <label className="action-label">Услуга</label>
                                                            <select name="serviceId" className="form-input" onChange={e => setSelectedServiceId(e.target.value)} required>
                                                                <option value="">Выберите...</option>
                                                                {services?.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                                                            </select>
                                                        </div>
                                                        <div>
                                                            <label className="action-label">Тариф</label>
                                                            <select name="tariffPlanId" className="form-input" required disabled={!selectedServiceId}>
                                                                <option value="">Сначала выберите услугу</option>
                                                                {tariffPlans?.map(t => <option key={t.id} value={t.id}>{t.name} ({t.price} ₽)</option>)}
                                                            </select>
                                                        </div>
                                                        <div>
                                                            <label className="action-label">Дата начала</label>
                                                            <input type="date" name="beginDate" className="form-input" required defaultValue={new Date().toISOString().split('T')[0]} min={activeContract ? new Date(activeContract.beginDate).toISOString().split('T')[0] : undefined} max={activeContract?.endDate ? new Date(activeContract.endDate).toISOString().split('T')[0] : undefined} />
                                                        </div>
                                                        <div>
                                                            <label className="action-label">Дата окончания</label>
                                                            <input type="date" name="endDate" className="form-input" min={activeContract ? new Date(activeContract.beginDate).toISOString().split('T')[0] : undefined} max={activeContract?.endDate ? new Date(activeContract.endDate).toISOString().split('T')[0] : undefined} />
                                                        </div>
                                                    </div>
                                                    <div className="action-btn-group">
                                                        <button type="button" className="btn-secondary" onClick={() => setActiveAction({ type: null })}>Отмена</button>
                                                        <button type="submit" className="btn-primary" disabled={isConnecting || !selectedServiceId}>{isConnecting ? 'Подключение...' : 'Подключить'}</button>
                                                    </div>
                                                </form>
                                            )}

                                            {!isConnectingToThis && (
                                                <div style={{
                                                    display: 'flex',
                                                    justifyContent: 'space-between',
                                                    alignItems: 'center',
                                                    padding: '8px 20px 6px',
                                                    background: '#fcfcfd',
                                                    borderBottom: '1px solid #f1f5f9'
                                                }}>
                                                    <span style={{ fontSize: '12px', fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.04em' }}>
                                                        Услуги
                                                    </span>
                                                    {!isTerminated && (
                                                        <button
                                                            className="btn-secondary"
                                                            style={{ fontSize: '12px', padding: '5px 10px', height: 'auto', gap: '6px' }}
                                                            onClick={() => { setActiveAction({ type: 'connect', contractId: contract.id }); setSelectedServiceId(''); }}
                                                        >
                                                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24" style={{ color: '#667eea' }}>
                                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M12 4v16m8-8H4" />
                                                            </svg>
                                                            Подключить услугу
                                                        </button>
                                                    )}
                                                </div>
                                            )}

                                            {contract.subs.length > 0 ? (
                                                contract.subs.map((sub, idx) => (
                                                    <SubscriptionRow
                                                        key={idx}
                                                        sub={{ id: sub.id, serviceId: sub.serviceId, serviceName: sub.serviceName, tariffPlanName: sub.tariffPlanName, beginDate: sub.beginDate, endDate: sub.endDate }}
                                                        isActiveAction={activeAction.type === 'changeTariff' && activeAction.subId === sub.id}
                                                        isChangingTariff={isChangingTariff}
                                                        isCancelling={isCancelling}
                                                        onChangeTariffClick={() => setActiveAction({ type: 'changeTariff', subId: sub.id })}
                                                        onCancelClick={() => setActiveAction({ type: null })}
                                                        onCancelSubscriptionClick={() => handleCancelSubscription(sub.id)}
                                                        handleChangeTariffSubmit={handleChangeTariffSubmit}
                                                    />
                                                ))
                                            ) : (
                                                !isConnectingToThis && (
                                                    <div style={{ padding: '32px 20px', textAlign: 'center' }}>
                                                        <p style={{ margin: '0 0 16px', color: 'var(--text-muted)', fontSize: '14px' }}>
                                                            По этому договору пока нет услуг
                                                        </p>
                                                        {!isTerminated && (
                                                            <button
                                                                className="btn-secondary"
                                                                onClick={() => { setActiveAction({ type: 'connect', contractId: contract.id }); setSelectedServiceId(''); }}
                                                            >
                                                                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24" style={{ color: '#667eea' }}>
                                                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                                                                </svg>
                                                                Подключить первую услугу
                                                            </button>
                                                        )}
                                                    </div>
                                                )
                                            )}
                                        </div>
                                    )}
                                </div>
                            );
                        })}
                    </div>

                    {groupedContracts.length === 0 && !showCreateForm && (
                        <div style={{ textAlign: 'center', padding: '40px', color: 'var(--text-muted)' }}>
                            У вас пока нет договоров. Нажмите "Новый договор", чтобы начать.
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};