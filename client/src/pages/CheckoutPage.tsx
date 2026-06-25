import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { placeOrder } from '../features/orders/ordersSlice';

export function CheckoutPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const total = useAppSelector((state) => state.cart.cart?.totalAmount ?? 0);

  const submit = async () => {
    const result = await dispatch(placeOrder());
    if (placeOrder.fulfilled.match(result)) {
      navigate('/orders');
    }
  };

  return (
    <section className="auth-panel">
      <h1>Checkout</h1>
      <p className="total-line">${total.toFixed(2)}</p>
      <button className="button" onClick={submit}>Place order</button>
    </section>
  );
}
