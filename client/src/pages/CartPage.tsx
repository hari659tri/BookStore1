import { Link } from 'react-router-dom';
import { useEffect } from 'react';
import { Minus, Plus, Trash2 } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { fetchCart, removeCartItem, updateCartItem } from '../features/cart/cartSlice';

export function CartPage() {
  const dispatch = useAppDispatch();
  const cart = useAppSelector((state) => state.cart.cart);

  useEffect(() => {
    dispatch(fetchCart());
  }, [dispatch]);

  return (
    <section className="content">
      <div className="page-heading"><h1>Cart</h1></div>
      <div className="table-card">
        {cart?.items.map((item) => (
          <div className="cart-row" key={item.id}>
            <strong>{item.bookTitle}</strong>
            <div className="quantity">
              <button className="icon-button" onClick={() => dispatch(updateCartItem({ itemId: item.id, quantity: Math.max(1, item.quantity - 1) }))}><Minus size={16} /></button>
              <span>{item.quantity}</span>
              <button className="icon-button" onClick={() => dispatch(updateCartItem({ itemId: item.id, quantity: item.quantity + 1 }))}><Plus size={16} /></button>
            </div>
            <span>${item.lineTotal.toFixed(2)}</span>
            <button className="icon-button" onClick={() => dispatch(removeCartItem(item.id))} aria-label="Remove"><Trash2 size={16} /></button>
          </div>
        ))}
        <div className="summary-row">
          <strong>Total</strong>
          <strong>${(cart?.totalAmount ?? 0).toFixed(2)}</strong>
        </div>
      </div>
      <Link className="button" to="/checkout">Checkout</Link>
    </section>
  );
}
