import { ShoppingCart } from 'lucide-react';
import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { addCartItem } from '../features/cart/cartSlice';
import { fetchBook } from '../features/catalog/catalogSlice';

export function BookDetails() {
  const { id } = useParams();
  const dispatch = useAppDispatch();
  const book = useAppSelector((state) => state.catalog.selectedBook);
  const session = useAppSelector((state) => state.auth.session);

  useEffect(() => {
    if (id) {
      dispatch(fetchBook(Number(id)));
    }
  }, [dispatch, id]);

  if (!book) return null;

  return (
    <section className="details">
      <img src={book.coverImageUrl || '/placeholder-book.svg'} alt={book.title} />
      <div>
        <p className="meta">{book.categoryName}</p>
        <h1>{book.title}</h1>
        <p className="lede">{book.description}</p>
        <dl className="facts">
          <div><dt>Author</dt><dd>{book.authorName}</dd></div>
          <div><dt>ISBN</dt><dd>{book.isbn}</dd></div>
          <div><dt>Stock</dt><dd>{book.stockQuantity}</dd></div>
        </dl>
        <div className="purchase-row">
          <strong>${book.price.toFixed(2)}</strong>
          <button className="button" disabled={!session} onClick={() => dispatch(addCartItem({ bookId: book.id, quantity: 1 }))}>
            <ShoppingCart size={18} /> Add
          </button>
        </div>
      </div>
    </section>
  );
}
