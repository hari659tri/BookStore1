export type Role = 'Admin' | 'Customer';

export interface UserSession {
  userId: string;
  email: string;
  fullName: string;
  roles: Role[];
  token: string;
  expiresAt: string;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface Book {
  id: number;
  title: string;
  isbn: string;
  price: number;
  stockQuantity: number;
  description?: string;
  coverImageUrl?: string;
  authorId: number;
  authorName: string;
  categoryId: number;
  categoryName: string;
  isActive: boolean;
}

export interface Author {
  id: number;
  name: string;
  biography?: string;
  isActive: boolean;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface CartItem {
  id: number;
  bookId: number;
  bookTitle: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Cart {
  id: number;
  items: CartItem[];
  totalAmount: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  totalAmount: number;
  status: 'Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Cancelled';
  orderedAt: string;
  items: Array<{
    id: number;
    bookId: number;
    bookTitle: string;
    quantity: number;
    unitPrice: number;
    lineTotal: number;
  }>;
}

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
  roles: Role[];
  isActive: boolean;
}
