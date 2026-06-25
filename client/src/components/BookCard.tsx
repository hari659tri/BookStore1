import { ShoppingCart } from 'lucide-react';
import { Link } from 'react-router-dom';
import type { Book } from '../types';

export function BookCard({ book, onAdd }: { book: Book; onAdd: (bookId: number) => void }) {
  return (
    <article className="book-card">
      <Link to={`/books/${book.id}`} className="cover-wrap" aria-label={book.title}>
        <img src={book.coverImageUrl || '/placeholder-book.svg'} alt={book.title} />
      </Link>
      <div className="book-card-body">
        <p className="meta">{book.categoryName}</p>
        <h2>{book.title}</h2>
        <p>{book.authorName}</p>
        <div className="book-card-footer">
          <strong>${book.price.toFixed(2)}</strong>
          <button className="icon-button filled" onClick={() => onAdd(book.id)} aria-label={`Add ${book.title} to cart`}>
            <ShoppingCart size={18} />
          </button>
        </div>
      </div>
    </article>
  );
}
