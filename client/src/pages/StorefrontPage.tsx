import { Search } from 'lucide-react';
import { FormEvent, useEffect, useState } from 'react';
import { BookCard } from '../components/BookCard';
import { StatusBlock } from '../components/StatusBlock';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { addCartItem } from '../features/cart/cartSlice';
import { fetchBooks, fetchCatalogLookups } from '../features/catalog/catalogSlice';

export function StorefrontPage() {
  const dispatch = useAppDispatch();
  const { books, categories, authors, status } = useAppSelector((state) => state.catalog);
  const session = useAppSelector((state) => state.auth.session);
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [authorId, setAuthorId] = useState('');

  useEffect(() => {
    dispatch(fetchCatalogLookups());
    dispatch(fetchBooks({ pageSize: 12 }));
  }, [dispatch]);

  const submit = (event: FormEvent) => {
    event.preventDefault();
    dispatch(fetchBooks({ search, categoryId: categoryId || undefined, authorId: authorId || undefined, pageSize: 12 }));
  };

  const addToCart = (bookId: number) => {
    if (session) {
      dispatch(addCartItem({ bookId, quantity: 1 }));
    }
  };

  return (
    <section className="content">
      <div className="page-heading">
        <div>
          <p className="meta">Coforge trainee project</p>
          <h1>Book Store Management</h1>
        </div>
      </div>

      <form className="filters" onSubmit={submit}>
        <label className="search-box">
          <Search size={18} />
          <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search title, ISBN, author" />
        </label>
        <select value={categoryId} onChange={(event) => setCategoryId(event.target.value)}>
          <option value="">All categories</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
        <select value={authorId} onChange={(event) => setAuthorId(event.target.value)}>
          <option value="">All authors</option>
          {authors.map((author) => (
            <option key={author.id} value={author.id}>
              {author.name}
            </option>
          ))}
        </select>
        <button className="button" type="submit">
          Apply
        </button>
      </form>

      {status === 'loading' && <StatusBlock title="Loading books" />}
      {books?.items.length === 0 && <StatusBlock title="No books found" />}
      <div className="book-grid">
        {books?.items.map((book) => <BookCard key={book.id} book={book} onAdd={addToCart} />)}
      </div>
    </section>
  );
}
