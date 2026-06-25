import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { fetchAdminOrders, fetchAdminUsers, updateOrderStatus } from '../features/admin/adminSlice';

const statuses = ['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'];

export function AdminDashboard() {
  const dispatch = useAppDispatch();
  const { users, orders } = useAppSelector((state) => state.admin);

  useEffect(() => {
    dispatch(fetchAdminUsers());
    dispatch(fetchAdminOrders());
  }, [dispatch]);

  return (
    <section className="content admin-grid">
      <div>
        <div className="page-heading"><h1>Admin Dashboard</h1></div>
        <div className="table-card">
          {users.map((user) => (
            <div className="admin-row" key={user.id}>
              <strong>{user.fullName}</strong>
              <span>{user.email}</span>
              <span>{user.roles.join(', ')}</span>
            </div>
          ))}
        </div>
      </div>
      <div>
        <div className="page-heading"><h1>Orders</h1></div>
        <div className="table-card">
          {orders.map((order) => (
            <div className="admin-row" key={order.id}>
              <strong>{order.orderNumber}</strong>
              <span>${order.totalAmount.toFixed(2)}</span>
              <select value={order.status} onChange={(event) => dispatch(updateOrderStatus({ id: order.id, status: event.target.value }))}>
                {statuses.map((status) => <option key={status}>{status}</option>)}
              </select>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
