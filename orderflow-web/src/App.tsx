import { useState } from 'react';
import {
  QueryClient,
  QueryClientProvider,
  useMutation,
  useQuery
} from '@tanstack/react-query';
import {
  Plus,
  Trash2,
  ShoppingBag,
  Clock,
  CheckCircle2,
  AlertCircle,
  Loader2,
  Search,
  PackageCheck
} from 'lucide-react';
import { createOrder, getOrderById } from './api';
import type { CreateOrderItem } from './types';

const queryClient = new QueryClient();

interface FormItem extends CreateOrderItem {
  id: string; // ID local para key do React
}

function OrderFlowDashboard() {
  const [customerId, setCustomerId] = useState(crypto.randomUUID());
  const [items, setItems] = useState<FormItem[]>([
    {
      id: crypto.randomUUID(),
      productId: crypto.randomUUID(),
      productName: 'Monitor UltraWide 29"',
      unitPrice: 1899.90,
      quantity: 1,
    },
    {
      id: crypto.randomUUID(),
      productId: crypto.randomUUID(),
      productName: 'Teclado Mecânico RGB',
      unitPrice: 350.00,
      quantity: 2,
    }
  ]);
  const [trackedOrderId, setTrackedOrderId] = useState<string>('');
  const [searchId, setSearchId] = useState<string>('');
  const [errorList, setErrorList] = useState<string[]>([]);

  // Consulta do pedido com Auto-Refetch (Polling a cada 1.5s enquanto não for aprovado)
  const { data: order, isLoading: isTrackingLoading } = useQuery({
    queryKey: ['order', trackedOrderId],
    queryFn: () => getOrderById(trackedOrderId),
    enabled: !!trackedOrderId,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      return status === 'Approved' || status === 'Failed' ? false : 1500;
    },
  });

  // Mutação para criar pedido
  const createOrderMutation = useMutation({
    mutationFn: createOrder,
    onSuccess: (data) => {
      setTrackedOrderId(data.id);
      setSearchId(data.id);
      setErrorList([]);
    },
    onError: (error: any) => {
      const data = error.response?.data;

      // Se a API devolveu o dicionário de erros detalhados
      if (data?.errors) {
        // Achata todos os arrays de mensagens em uma lista única
        const messages: string[] = Object.values(data.errors).flat() as string[];
        setErrorList(messages);
      } else if (data?.detail) {
        setErrorList([data.detail]);
      } else {
        setErrorList(['Erro ao criar pedido. Verifique os campos preenchidos.']);
      }
    },
  });

  const handleAddItem = () => {
    setItems([
      ...items,
      {
        id: crypto.randomUUID(),
        productId: crypto.randomUUID(),
        productName: '',
        unitPrice: 0,
        quantity: 1,
      },
    ]);
  };

  const handleRemoveItem = (id: string) => {
    if (items.length > 1) {
      setItems(items.filter((item) => item.id !== id));
    }
  };

  const handleItemChange = (id: string, field: keyof CreateOrderItem, value: string | number) => {
    setItems(
      items.map((item) =>
        item.id === id ? { ...item, [field]: value } : item
      )
    );
  };

  const totalCalculated = items.reduce(
    (acc, item) => acc + (Number(item.unitPrice) || 0) * (Number(item.quantity) || 0),
    0
  );

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setErrorList([]);

    createOrderMutation.mutate({
      customerId,
      items: items.map(({ productId, productName, unitPrice, quantity }) => ({
        productId,
        productName,
        unitPrice: Number(unitPrice),
        quantity: Number(quantity),
      })),
    });
  };

  return (
    <div className="min-h-screen bg-slate-900 text-slate-100 p-6 md:p-10 font-sans">
      {/* Header */}
      <header className="max-w-6xl mx-auto flex items-center justify-between pb-8 border-b border-slate-800">
        <div className="flex items-center gap-3">
          <div className="bg-indigo-600 p-2.5 rounded-xl shadow-lg shadow-indigo-500/20">
            <PackageCheck className="w-7 h-7 text-white" />
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight text-white">OrderFlow</h1>
            <p className="text-xs text-slate-400">Clean Architecture + RabbitMQ Mensageria</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
            ● .NET 8 + MassTransit Ativo
          </span>
        </div>
      </header>

      {/* Grid Principal */}
      <main className="max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-12 gap-8 mt-8">
        {/* Formulário de Criação (Coluna 7) */}
        <section className="lg:col-span-7 bg-slate-800/60 backdrop-blur border border-slate-700/60 rounded-2xl p-6 shadow-xl">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-lg font-semibold flex items-center gap-2 text-white">
              <ShoppingBag className="w-5 h-5 text-indigo-400" />
              Novo Pedido Assíncrono
            </h2>
            <button
              type="button"
              onClick={handleAddItem}
              className="inline-flex items-center gap-1.5 text-xs bg-indigo-600/20 text-indigo-400 hover:bg-indigo-600/30 border border-indigo-500/30 px-3 py-1.5 rounded-lg transition"
            >
              <Plus className="w-4 h-4" /> Adicionar Produto
            </button>
          </div>

          {errorList.length > 0 && (
            <div className="mb-6 p-4 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
              <div className="flex items-center gap-2 font-semibold mb-2">
                <AlertCircle className="w-5 h-5 shrink-0" />
                <span>Não foi possível enviar o pedido:</span>
              </div>
              <ul className="list-disc list-inside space-y-1 text-xs text-red-300">
                {errorList.map((err, idx) => (
                  <li key={idx}>{err}</li>
                ))}
              </ul>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-xs font-medium text-slate-400 mb-1.5">
                Customer ID (Identificador do Cliente)
              </label>
              <input
                type="text"
                value={customerId}
                onChange={(e) => setCustomerId(e.target.value)}
                className="w-full bg-slate-900/80 border border-slate-700 rounded-xl px-3.5 py-2.5 text-xs font-mono text-slate-300 focus:outline-none focus:border-indigo-500 transition"
                required
              />
            </div>

            <div className="space-y-3">
              <label className="block text-xs font-medium text-slate-400">
                Itens do Pedido ({items.length})
              </label>
              {items.map((item, index) => (
                <div
                  key={item.id}
                  className="bg-slate-900/60 border border-slate-700/60 rounded-xl p-3.5 flex flex-col md:flex-row gap-3 items-center"
                >
                  <div className="flex-1 w-full">
                    <input
                      type="text"
                      placeholder="Nome do produto"
                      value={item.productName}
                      onChange={(e) => handleItemChange(item.id, 'productName', e.target.value)}
                      className="w-full bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-sm text-slate-200 focus:outline-none focus:border-indigo-500"
                      required
                    />
                  </div>
                  <div className="w-full md:w-32">
                    <input
                      type="number"
                      step="0.01"
                      placeholder="Preço (R$)"
                      value={item.unitPrice || ''}
                      onChange={(e) => handleItemChange(item.id, 'unitPrice', parseFloat(e.target.value) || 0)}
                      className="w-full bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-sm text-slate-200 focus:outline-none focus:border-indigo-500"
                      required
                    />
                  </div>
                  <div className="w-full md:w-24">
                    <input
                      type="number"
                      min="1"
                      placeholder="Qtd"
                      value={item.quantity || ''}
                      onChange={(e) => handleItemChange(item.id, 'quantity', parseInt(e.target.value) || 1)}
                      className="w-full bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-sm text-slate-200 text-center focus:outline-none focus:border-indigo-500"
                      required
                    />
                  </div>
                  <button
                    type="button"
                    onClick={() => handleRemoveItem(item.id)}
                    disabled={items.length === 1}
                    className="text-slate-500 hover:text-red-400 disabled:opacity-30 p-2 transition"
                    title="Remover item"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>

            <div className="pt-4 border-t border-slate-700/60 flex items-center justify-between">
              <div>
                <span className="text-xs text-slate-400 block">Total Previsto:</span>
                <span className="text-2xl font-bold text-white">
                  {totalCalculated.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                </span>
              </div>
              <button
                type="submit"
                disabled={createOrderMutation.isPending}
                className="bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white font-medium px-6 py-3 rounded-xl flex items-center gap-2 shadow-lg shadow-indigo-600/30 transition"
              >
                {createOrderMutation.isPending ? (
                  <>
                    <Loader2 className="w-4 h-4 animate-spin" /> Processando...
                  </>
                ) : (
                  <>
                    Enviar Pedido ao RabbitMQ
                  </>
                )}
              </button>
            </div>
          </form>
        </section>

        {/* Painel de Rastreamento em Tempo Real (Coluna 5) */}
        <section className="lg:col-span-5 space-y-6">
          {/* Card de Busca Manual */}
          <div className="bg-slate-800/60 border border-slate-700/60 rounded-2xl p-5">
            <label className="block text-xs font-medium text-slate-400 mb-2">
              Rastrear Pedido por ID
            </label>
            <div className="flex gap-2">
              <input
                type="text"
                placeholder="Cole o ID do pedido..."
                value={searchId}
                onChange={(e) => setSearchId(e.target.value)}
                className="flex-1 bg-slate-900 border border-slate-700 rounded-xl px-3 py-2 text-xs font-mono text-slate-300 focus:outline-none focus:border-indigo-500"
              />
              <button
                type="button"
                onClick={() => setTrackedOrderId(searchId)}
                className="bg-slate-700 hover:bg-slate-600 px-4 rounded-xl flex items-center justify-center text-white transition"
              >
                <Search className="w-4 h-4" />
              </button>
            </div>
          </div>

          {/* Card do Status em Tempo Real */}
          {trackedOrderId ? (
            <div className="bg-slate-800/60 border border-slate-700/60 rounded-2xl p-6 space-y-6 shadow-xl">
              <div className="flex items-center justify-between pb-4 border-b border-slate-700/60">
                <div>
                  <span className="text-xs text-slate-400 block">Status do Pedido:</span>
                  <div className="mt-1">
                    {order?.status === 'Pending' && (
                      <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-amber-500/10 text-amber-400 border border-amber-500/20 animate-pulse">
                        <Clock className="w-3.5 h-3.5" /> Pendente (No Outbox)
                      </span>
                    )}
                    {order?.status === 'Processing' && (
                      <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-sky-500/10 text-sky-400 border border-sky-500/20">
                        <Loader2 className="w-3.5 h-3.5 animate-spin" /> Em Processamento (Worker)
                      </span>
                    )}
                    {order?.status === 'Approved' && (
                      <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
                        <CheckCircle2 className="w-3.5 h-3.5" /> Aprovado & Faturado
                      </span>
                    )}
                    {order?.status === 'Failed' && (
                      <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-rose-500/10 text-rose-400 border border-rose-500/20">
                        <AlertCircle className="w-3.5 h-3.5" /> Pagamento Falhou
                      </span>
                    )}
                  </div>
                </div>
                <div className="text-right">
                  <span className="text-xs text-slate-400 block">Valor Total:</span>
                  <span className="text-xl font-bold text-white">
                    {order?.totalAmount.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                  </span>
                </div>
              </div>

              <div className="text-xs text-slate-400 space-y-1 font-mono">
                <div>ID: <span className="text-slate-300">{order?.id}</span></div>
                <div>Criado em: <span className="text-slate-300">{order?.createdAt ? new Date(order.createdAt).toLocaleTimeString() : '-'}</span></div>
                <div>Atualizado: <span className="text-slate-300">{order?.updatedAt ? new Date(order.updatedAt).toLocaleTimeString() : 'Aguardando...'}</span></div>
              </div>

              <div>
                <h4 className="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  Itens Faturados
                </h4>
                <div className="space-y-2 max-h-48 overflow-y-auto pr-1">
                  {order?.items.map((item, idx) => (
                    <div key={idx} className="flex justify-between items-center bg-slate-900/60 p-2.5 rounded-lg text-xs">
                      <div>
                        <span className="font-medium text-slate-200 block">{item.productName}</span>
                        <span className="text-slate-400">{item.quantity}x de {item.unitPrice.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</span>
                      </div>
                      <span className="font-semibold text-slate-200">
                        {item.subtotal.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          ) : (
            <div className="bg-slate-800/40 border border-dashed border-slate-700/80 rounded-2xl p-10 text-center text-slate-500">
              <ShoppingBag className="w-10 h-10 mx-auto mb-2 opacity-40" />
              <p className="text-sm">Nenhum pedido selecionado.</p>
              <p className="text-xs text-slate-600 mt-1">Crie um pedido ao lado para acompanhar o fluxo de mensageria em tempo real.</p>
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <OrderFlowDashboard />
    </QueryClientProvider>
  );
}