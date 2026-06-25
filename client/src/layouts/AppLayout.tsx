import { BookOpen, LogOut, ShoppingCart, UserRoundCog } from 'lucide-react';
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { logout } from '../features/auth/authSlice';

export function AppLayout() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const session = useAppSelector((state) => state.auth.session);
  const isAdmin = session?.roles.includes('Admin');

  const signOut = () => {
    dispatch(logout());
    navigate('/');
  };

  return (
    <div className="shell">
      <header className="topbar">
        <Link to="/" className="brand" aria-label="Book Store Home">
          <BookOpen size={26} />
          <span>BookStore</span>
        </Link>
        <nav className="nav">
          <NavLink to="/books">Books</NavLink>
          {session && <NavLink to="/orders">Orders</NavLink>}
          {isAdmin && <NavLink to="/admin">Admin</NavLink>}
        </nav>
        <div className="actions">
          {session ? (
            <>
              <Link className="icon-button" to="/cart" aria-label="Cart">
                <ShoppingCart size={20} />
              </Link>
              {isAdmin && (
                <Link className="icon-button" to="/admin" aria-label="Admin dashboard">
                  <UserRoundCog size={20} />
                </Link>
              )}
              <button className="icon-button" onClick={signOut} aria-label="Sign out">
                <LogOut size={20} />
              </button>
            </>
          ) : (
            <Link className="button compact" to="/login">
              Login
            </Link>
          )}
        </div>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  );
}
