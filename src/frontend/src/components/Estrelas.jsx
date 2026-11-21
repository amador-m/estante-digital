// Mostra de 1 a 5 estrelas. Puramente decorativo/informativo — não é
// clicável aqui (a edição da nota acontece no formulário do admin).
export default function Estrelas({ nota }) {
  if (!nota) return null;

  return (
    <span className="estrelas" title={`Avaliação: ${nota} de 5`}>
      {Array.from({ length: 5 }, (_, i) => (
        <span key={i} className={i < nota ? "estrela-cheia" : "estrela-vazia"}>
          ★
        </span>
      ))}
    </span>
  );
}
