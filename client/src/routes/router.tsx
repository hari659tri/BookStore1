import { createBrowserRouter, Navigate } from 'react-router-dom';
import { AppLayout } from '../layouts/AppLayout';
import { AdminDashboard } from '../pages/AdminDashboard';
import { BookDetails } from '../pages/BookDetails';
import { CartPage } from '../pages/CartPage';
import { CheckoutPage } from '../pages/CheckoutPage';
import { LoginPage } from '../pages/LoginPage';
import { OrdersPage } from '../pages/OrdersPage';
import { RegisterPage } from '../pages/RegisterPage';
import { StorefrontPage } from '../pages/StorefrontPage';
import { useAppSelector } from '../app/hooks';

function ProtectedRoute({ children, role }: { children: React.ReactNode; role?: 'Admin' | 'Customer' }) {
  const session = useAppSelector((state) => state.auth.session);
  if (!session) {
    return <Navigate to="/login" replace />;
  }

  if (role && !session.roles.includes(role)) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <StorefrontPage /> },
      { path: 'books', element: <StorefrontPage /> },
      { path: 'books/:id', element: <BookDetails /> },
      { path: 'login', element: <LoginPage /> },
      { path: 'register', element: <RegisterPage /> },
      {
        path: 'cart',
        element: (
          <ProtectedRoute>
            <CartPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'checkout',
        element: (
          <ProtectedRoute>
            <CheckoutPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'orders',
        element: (
          <ProtectedRoute>
            <OrdersPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'admin',
        element: (
          <ProtectedRoute role="Admin">
            <AdminDashboard />
          </ProtectedRoute>
        ),
      },
    ],
  },
]);
