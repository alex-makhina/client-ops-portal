import React from 'react';
import { useTariffPlans } from '../hooks/useSubscriptionActions';
import { type TariffPlanShort } from '../types/subscription.types';

interface SubscriptionRowProps {
    sub: {
        id: string;
        serviceId: string;
        serviceName: string;
        tariffPlanName: string;
        beginDate: string;
        endDate?: string;
    };
    isActiveAction: boolean;
    isChangingTariff: boolean;
    isCancelling: boolean;
    onChangeTariffClick: () => void;
    onCancelClick: () => void;
    onCancelSubscriptionClick?: () => void;
    handleChangeTariffSubmit: (e: React.SyntheticEvent<HTMLFormElement>) => void;
}

export const SubscriptionRow: React.FC<SubscriptionRowProps> = ({
    sub,
    isActiveAction,
    isChangingTariff,
    isCancelling,
    onChangeTariffClick,
    onCancelClick,
    onCancelSubscriptionClick,
    handleChangeTariffSubmit,
}) => {
    const { data: changeTariffPlans } = useTariffPlans(sub.serviceId);

    if (isActiveAction) {
        return (
            <div className="sub-row" style={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'flex-end',
                padding: '16px 20px',
                background: '#fff'
            }}>
                <form onSubmit={handleChangeTariffSubmit} style={{
                    display: 'flex',
                    gap: '12px',
                    alignItems: 'center',
                    width: '100%',
                    justifyContent: 'space-between'
                }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '12px', flex: 1 }}>
                        <span style={{ fontSize: '13px', color: 'var(--text-muted)' }}>
                            {sub.serviceName} — Новый тариф:
                        </span>
                        <select name="newTariffPlanId" className="form-input" style={{ width: '200px', padding: '6px 10px' }} required>
                            <option value="">Выберите тариф...</option>
                            {changeTariffPlans?.map((t: TariffPlanShort) => (
                                <option key={t.id} value={t.id}>{t.name} ({t.price} ₽)</option>
                            ))}
                        </select>
                    </div>

                    <div style={{ display: 'flex', gap: '8px' }}>
                        <button type="submit" className="btn-primary" style={{ padding: '6px 16px' }} disabled={isChangingTariff}>
                            {isChangingTariff ? '...' : 'Сохранить'}
                        </button>
                        <button type="button" className="btn-secondary" style={{ padding: '8px 14px' }} onClick={onCancelClick}>
                            Отмена
                        </button>
                    </div>
                </form>
            </div>
        );
    }

    return (
        <div className="sub-row">
            <div className="sub-cell service">{sub.serviceName}</div>
            <div className="sub-cell">{sub.tariffPlanName}</div>
            <div className="sub-cell date">{new Date(sub.beginDate).toLocaleDateString('ru-RU')} — {sub.endDate ? new Date(sub.endDate).toLocaleDateString('ru-RU') : '∞'}</div>

            <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
                <button className="sub-action-btn" onClick={onChangeTariffClick}>
                    <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                            d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                    </svg>
                    Сменить тариф
                </button>
                <button className="btn-danger" onClick={onCancelSubscriptionClick} disabled={isCancelling}>
                    <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                            d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" />
                    </svg>
                    Отключить
                </button>
            </div>
        </div>
    );
};