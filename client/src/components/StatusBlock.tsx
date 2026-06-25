export function StatusBlock({ title, message }: { title: string; message?: string }) {
  return (
    <section className="status-block">
      <h2>{title}</h2>
      {message && <p>{message}</p>}
    </section>
  );
}
