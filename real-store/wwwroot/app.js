const state = {
  books: [],
  orders: [],
  cart: new Map()
};

const money = value => `$${Number(value).toFixed(2)}`;
const show = message => {
  toast.textContent = message;
  toast.classList.add('show');
  setTimeout(() => toast.classList.remove('show'), 2200);
};

async function api(path, options) {
  const response = await fetch(path, {
    headers: { 'content-type': 'application/json' },
    ...options
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || response.statusText);
  }
  return response.status === 204 ? null : response.json();
}

async function load() {
  const params = new URLSearchParams();
  if (search.value) params.set('q', search.value);
  if (category.value && category.value !== 'All') params.set('category', category.value);
  state.books = await api(`/api/books?${params}`);
  state.orders = await api('/api/orders');
  render();
}

function render() {
  bookCount.textContent = `${state.books.length} books`;
  orderCount.textContent = `${state.orders.length} orders`;
  renderCategories();
  renderBooks();
  renderCart();
  renderOrders();
}

function renderCategories() {
  const current = category.value || 'All';
  const values = ['All', ...new Set(state.books.map(book => book.category))];
  category.innerHTML = values.map(value => `<option ${value === current ? 'selected' : ''}>${value}</option>`).join('');
}

function renderBooks() {
  books.innerHTML = state.books.map(book => `
    <article class="book">
      <div class="cover">${book.title}</div>
      <h3>${book.title}</h3>
      <div class="meta">${book.author} · ${book.category}</div>
      <div class="meta">${book.stock} in stock</div>
      <div class="price">
        <strong>${money(book.price)}</strong>
        <button class="primary" ${book.stock < 1 ? 'disabled' : ''} onclick="addToCart(${book.id})">Add</button>
      </div>
    </article>
  `).join('');
}

function renderCart() {
  const items = cartItems();
  cartCount.textContent = items.reduce((sum, item) => sum + item.quantity, 0);
  cartTotal.textContent = money(items.reduce((sum, item) => sum + item.price * item.quantity, 0));
  cartRows.innerHTML = items.length ? items.map(item => `
    <div class="row">
      <div><strong>${item.title}</strong><br><span class="meta">${item.quantity} x ${money(item.price)}</span></div>
      <button onclick="removeFromCart(${item.id})">Remove</button>
    </div>
  `).join('') : '<p class="meta">No books in cart.</p>';
}

function renderOrders() {
  orders.innerHTML = state.orders.length ? state.orders.map(order => `
    <div class="row">
      <div><strong>#${order.id} ${order.customerName}</strong><br><span class="meta">${new Date(order.createdAt).toLocaleString()}</span></div>
      <div><strong>${money(order.total)}</strong> · ${order.status}</div>
    </div>
  `).join('') : '<p class="meta">No orders yet.</p>';
}

function cartItems() {
  return [...state.cart.entries()]
    .map(([id, quantity]) => ({ ...state.books.find(book => book.id === id), quantity }))
    .filter(item => item.id);
}

function addToCart(id) {
  const book = state.books.find(item => item.id === id);
  const next = (state.cart.get(id) || 0) + 1;
  if (next > book.stock) return show('No more stock available');
  state.cart.set(id, next);
  renderCart();
}

function removeFromCart(id) {
  const next = (state.cart.get(id) || 0) - 1;
  next > 0 ? state.cart.set(id, next) : state.cart.delete(id);
  renderCart();
}

async function placeOrder() {
  const items = cartItems().map(item => ({ bookId: item.id, quantity: item.quantity }));
  if (!items.length) return show('Cart is empty');
  await api('/api/orders', {
    method: 'POST',
    body: JSON.stringify({ customerName: customerName.value || 'Guest', items })
  });
  state.cart.clear();
  show('Order placed');
  await load();
}

async function addBook(event) {
  event.preventDefault();
  const data = Object.fromEntries(new FormData(bookForm));
  await api('/api/books', {
    method: 'POST',
    body: JSON.stringify({
      title: data.title,
      author: data.author,
      category: data.category,
      price: Number(data.price),
      stock: Number(data.stock)
    })
  });
  bookForm.reset();
  show('Book added');
  await load();
}

document.querySelectorAll('[data-view]').forEach(button => {
  button.addEventListener('click', () => {
    document.querySelectorAll('.view').forEach(view => view.classList.add('hidden'));
    document.getElementById(button.dataset.view).classList.remove('hidden');
  });
});

search.addEventListener('input', () => load().catch(error => show(error.message)));
category.addEventListener('change', () => load().catch(error => show(error.message)));
placeOrder.addEventListener('click', () => placeOrder().catch(error => show(error.message)));
bookForm.addEventListener('submit', event => addBook(event).catch(error => show(error.message)));

load().catch(error => show(error.message));
