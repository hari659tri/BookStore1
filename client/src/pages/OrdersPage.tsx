import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { fetchOrders } from '../features/orders/ordersSlice';

export function OrdersPage() {
  const dispatch = useAppDispatch();
  const orders = useAppSelector((state) => state.orders.orders);

  useEffect(() => {
    dispatch(fetchOrders());
  }, [dispatch]);

  return (
    <section className="content">
      <div className="page-heading"><h1>Orders</h1></div>
      <div className="table-card">
        {orders.map((order) => (
          <div className="order-row" key={order.id}>
            <strong>{order.orderNumber}</strong>
            <span>{order.status}</span>
            <span>{new Date(order.orderedAt).toLocaleDateString()}</span>
            <strong>${order.totalAmount.toFixed(2)}</strong>
          </div>
        ))}
      </div>
    </section>
  );
}
