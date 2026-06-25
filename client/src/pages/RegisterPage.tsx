import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../app/hooks';
import { register } from '../features/auth/authSlice';

export function RegisterPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', password: '' });

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    const result = await dispatch(register(form));
    if (register.fulfilled.match(result)) {
      navigate('/');
    }
  };

  return (
    <section className="auth-panel">
      <h1>Register</h1>
      <form onSubmit={submit}>
        <label>First name<input value={form.firstName} onChange={(event) => setForm({ ...form, firstName: event.target.value })} required /></label>
        <label>Last name<input value={form.lastName} onChange={(event) => setForm({ ...form, lastName: event.target.value })} required /></label>
        <label>Email<input type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} required /></label>
        <label>Password<input type="password" minLength={8} value={form.password} onChange={(event) => setForm({ ...form, password: event.target.value })} required /></label>
        <button className="button">Register</button>
      </form>
    </section>
  );
}
