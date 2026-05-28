import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAbonentDashboard } from '../hooks/useAbonentDashboard';
import './PersonalAccount.css';

export const PersonalAccountPage: React.FC = () => {
    const navigate = useNavigate();
    const { abonent, contracts, subscriptions, isLoading, isError, error, refetch } = useAbonentDashboard();

    const handleLogout = () => {
        localStorage.clear();
        navigate('/login', { replace: true });
    };

    if (isLoading) {
        return (
            <div className="dashboard-wrapper">
                <div className="loading-spinner">Загрузка данных...</div>
            </div>
        );
    }

    if (isError) {
        return (
            <div className="dashboard-wrapper">
                <div className="error-state">
                    <h3>⚠️ Ошибка загрузки</h3>
                    <p>{(error as Error).message || 'Не удалось получить данные'}</p>
                    <button onClick={refetch} className="logout-btn" style={{ marginTop: 16, color: '#c53030' }}>
                        Попробовать снова
                    </button>
                </div>
            </div>
        );
    }

    if (!abonent) {
        return (
            <div className="dashboard-wrapper">
                <div className="loading-spinner">Данные абонента не найдены</div>
            </div>
        );
    }

    return (
        <div className="dashboard-wrapper">
            <div className="dashboard-container">
                <div className="dashboard-header">
                    <h1 className="dashboard-title">Личный кабинет</h1>
                    <button onClick={handleLogout} className="logout-btn">Выйти</button>
                </div>

                <div className="card">
                    <div className="card-header">
                        <h2 className="card-title">👤 Данные абонента</h2>
                        <button onClick={refetch} style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#667eea' }}>
                            🔄 Обновить
                        </button>
                    </div>
                    <div className="info-grid">
                        <div className="info-item">
                            <label>ФИО</label>
                            <span>{[abonent.lastName, abonent.firstName, abonent.middleName].filter(Boolean).join(' ')}</span>
                        </div>
                        <div className="info-item">
                            <label>Идентификационный номер</label>
                            <span>{abonent.identificationNumber || '—'}</span>
                        </div>
                        <div className="info-item">
                            <label>Номер лицевого счета</label>
                            <span>{abonent.accountNumber}</span>
                        </div>
                        <div className="info-item">
                            <label>Дата регистрации</label>
                            <span>{new Date(abonent.createdAt).toLocaleDateString('ru-RU')}</span>
                        </div>
                    </div>
                </div>

                <div className="card">
                    <div className="card-header">
                        <h2 className="card-title">📄 Договоры</h2>
                    </div>
                    {contracts.length > 0 ? (
                        <div className="table-container">
                            <table>
                                <thead>
                                    <tr>
                                        <th>Номер договора</th>
                                        <th>Дата начала</th>
                                        <th>Статус</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {contracts.map((c, i) => (
                                        <tr key={i}>
                                            <td style={{ fontWeight: 500 }}>{c.contractNumber}</td>
                                            <td>{new Date(c.beginDate).toLocaleDateString('ru-RU')}</td>
                                            <td><span className="status-badge">Активен</span></td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    ) : (
                        <div className="empty-state">Нет активных договоров</div>
                    )}
                </div>

                <div className="card">
                    <div className="card-header">
                        <h2 className="card-title">📦 Подписки</h2>
                    </div>
                    {subscriptions.length > 0 ? (
                        <div className="table-container">
                            <table>
                                <thead>
                                    <tr>
                                        <th>Услуга</th>
                                        <th>Тарифный план</th>
                                        <th>Период действия</th>
                                        <th>Статус</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {subscriptions.map((s, i) => (
                                        <tr key={i}>
                                            <td style={{ fontWeight: 500 }}>{s.serviceName}</td>
                                            <td>{s.tariffPlanName}</td>
                                            <td>
                                                {new Date(s.beginDate).toLocaleDateString('ru-RU')} — {s.endDate ? new Date(s.endDate).toLocaleDateString('ru-RU') : 'бессрочно'}
                                            </td>
                                            <td><span className="status-badge">Активна</span></td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    ) : (
                        <div className="empty-state">Нет активных подписок</div>
                    )}
                </div>
            </div>
        </div>
    );
};