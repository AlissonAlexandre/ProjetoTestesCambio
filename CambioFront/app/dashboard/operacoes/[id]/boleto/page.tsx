"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useToast } from "@/hooks/use-toast"
import { api } from "@/lib/api"
import { Skeleton } from "@/components/ui/skeleton"
import { ArrowLeft, Printer } from "lucide-react"
import { format } from "date-fns"
import { ptBR } from "date-fns/locale"

interface ExchangeTicket {
  operationId: number
  customerName: string
  customerDocument: string
  fromCurrencyCode: string
  toCurrencyCode: string
  amount: number
  exchangeRate: number
  totalAmount: number
  createdAt: string
  status: number
}

export default function BoletoPage({ params }: { params: { id: string } }) {
  const [ticket, setTicket] = useState<ExchangeTicket | null>(null)
  const [loading, setLoading] = useState(true)
  const { toast } = useToast()
  const router = useRouter()
  const operationId = Number.parseInt(params.id)

  useEffect(() => {
    if (isNaN(operationId)) {
      router.push("/dashboard/operacoes")
      return
    }

    const fetchTicket = async () => {
      setLoading(true)
      try {
        const response = await api.get(`/api/ExchangeOperation/${operationId}/ticket`)
        setTicket(response.data)
      } catch (error) {
        toast({
          title: "Erro ao carregar boleto",
          description: "Não foi possível carregar os dados do boleto.",
          variant: "destructive",
        })
        router.push("/dashboard/operacoes")
      } finally {
        setLoading(false)
      }
    }

    fetchTicket()
  }, [operationId])

  const handlePrint = () => {
    window.print()
  }

  const getStatusText = (status: number) => {
    switch (status) {
      case 0:
        return "Pendente"
      case 1:
        return "Concluída"
      case 2:
        return "Cancelada"
      default:
        return "Desconhecido"
    }
  }

  const getStatusClass = (status: number) => {
    switch (status) {
      case 0:
        return "text-yellow-600 bg-yellow-100 px-2 py-1 rounded-full text-xs"
      case 1:
        return "text-green-600 bg-green-100 px-2 py-1 rounded-full text-xs"
      case 2:
        return "text-red-600 bg-red-100 px-2 py-1 rounded-full text-xs"
      default:
        return "text-gray-600 bg-gray-100 px-2 py-1 rounded-full text-xs"
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-2">
          <Button variant="outline" size="icon" onClick={() => router.push("/dashboard/operacoes")}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <h1 className="text-3xl font-bold tracking-tight">Boleto de Câmbio</h1>
        </div>
        <Button onClick={handlePrint} className="print:hidden">
          <Printer className="mr-2 h-4 w-4" />
          Imprimir
        </Button>
      </div>

      <Card className="max-w-3xl mx-auto">
        {loading ? (
          <div className="p-6 space-y-4">
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-4 w-full" />
            <div className="space-y-2 mt-6">
              <Skeleton className="h-20 w-full" />
              <Skeleton className="h-20 w-full" />
              <Skeleton className="h-20 w-full" />
            </div>
          </div>
        ) : ticket ? (
          <>
            <CardHeader className="border-b">
              <div className="flex justify-between items-center">
                <div>
                  <CardTitle>Boleto de Câmbio #{ticket.operationId}</CardTitle>
                  <CardDescription>
                    Emitido em{" "}
                    {format(new Date(ticket.createdAt), "dd 'de' MMMM 'de' yyyy 'às' HH:mm", { locale: ptBR })}
                  </CardDescription>
                </div>
                <span className={getStatusClass(ticket.status)}>{getStatusText(ticket.status)}</span>
              </div>
            </CardHeader>
            <CardContent className="space-y-6 pt-6">
              <div className="space-y-1">
                <h3 className="text-sm font-medium text-muted-foreground">Cliente</h3>
                <p className="font-medium">{ticket.customerName}</p>
                <p className="text-sm">{ticket.customerDocument}</p>
              </div>

              <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="space-y-1">
                  <h3 className="text-sm font-medium text-muted-foreground">Moeda de Origem</h3>
                  <p className="font-medium">{ticket.fromCurrencyCode}</p>
                  <p className="text-lg font-bold">{ticket.amount.toFixed(2)}</p>
                </div>

                <div className="space-y-1">
                  <h3 className="text-sm font-medium text-muted-foreground">Moeda de Destino</h3>
                  <p className="font-medium">{ticket.toCurrencyCode}</p>
                  <p className="text-lg font-bold">{ticket.totalAmount.toFixed(2)}</p>
                </div>
              </div>

              <div className="space-y-1">
                <h3 className="text-sm font-medium text-muted-foreground">Taxa de Câmbio</h3>
                <p className="font-medium">
                  1 {ticket.fromCurrencyCode} = {ticket.exchangeRate.toFixed(4)} {ticket.toCurrencyCode}
                </p>
              </div>

              <div className="rounded-lg border p-4 bg-gray-50">
                <div className="flex justify-between items-center">
                  <span className="font-medium">Valor Total</span>
                  <span className="text-xl font-bold">
                    {ticket.totalAmount.toLocaleString("pt-BR", {
                      style: "currency",
                      currency: ticket.toCurrencyCode === "BRL" ? "BRL" : "USD",
                    })}
                  </span>
                </div>
              </div>
            </CardContent>
            <CardFooter className="border-t flex justify-between pt-6 print:hidden">
              <p className="text-sm text-muted-foreground">
                Este documento é um comprovante da operação de câmbio realizada.
              </p>
            </CardFooter>
          </>
        ) : (
          <CardContent>
            <p className="text-center py-8 text-muted-foreground">Boleto não encontrado.</p>
          </CardContent>
        )}
      </Card>
    </div>
  )
}
