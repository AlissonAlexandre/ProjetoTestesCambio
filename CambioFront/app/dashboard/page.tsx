"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useToast } from "@/hooks/use-toast"
import { DollarSign, Users, RefreshCw, ArrowRightLeft } from "lucide-react"
import { Skeleton } from "@/components/ui/skeleton"
import { api } from "@/lib/api"

interface DashboardStats {
  TotalCustomers: number
  TotalOperations: number
  TotalExchangeVolume: number
}

interface ExchangeRate {
  FromCurrencyId: number
  FromCurrencyCode: string
  FromCurrencyName: string
  ToCurrencyId: number
  ToCurrencyCode: string
  ToCurrencyName: string
  Rate: number
}

interface PaginatedOperationsResponse {
  Operations: RecentOperation[];
  PageNumber: number;
  PageSize: number;
  TotalPages: number;
  TotalCount: number;
  TotalAmount: number;
  HasPreviousPage: boolean;
  HasNextPage: boolean;
}

interface RecentOperation {
  Id: number
  CustomerId: number
  CustomerName: string
  FromCurrencyId: number
  FromCurrencyCode: string
  ToCurrencyId: number
  ToCurrencyCode: string
  Amount: number
  ExchangeRate: number
  FinalAmount: number
  Status: number
  CreatedAt: string
  CreatedByUserId: number
  CreatedByUserName: string
}

export default function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [exchangeRates, setExchangeRates] = useState<ExchangeRate[]>([])
  const [recentOperations, setRecentOperations] = useState<RecentOperation[]>([])
  const [loading, setLoading] = useState(true)
  const { toast } = useToast()

  const fetchDashboardData = async () => {
    setLoading(true)
    try {
      
      const statsResponse = await api.get("/api/Dashboard/stats")
      setStats(statsResponse.data)

      
      const ratesResponse = await api.get("/api/Currency/rates")
      setExchangeRates(ratesResponse.data?.Rates || [])

      
      const recentOpsResponse = await api.get<PaginatedOperationsResponse>("/api/ExchangeOperation/paged", {
        params: {
          pageNumber: 1,
          pageSize: 5,
          sortBy: "CreatedAt",
          ascending: false
        }
      })
      console.log("Operações recentes:", recentOpsResponse.data)
      setRecentOperations(recentOpsResponse.data?.Operations || [])
    } catch (error) {
      console.error("Erro ao carregar dados:", error)
      toast({
        title: "Erro ao carregar dados",
        description: "Não foi possível carregar os dados do dashboard.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchDashboardData()
  }, [])

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <Button variant="outline" size="sm" onClick={fetchDashboardData}>
          <RefreshCw className="mr-2 h-4 w-4" />
          Atualizar
        </Button>
      </div>

      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">Total de Clientes</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <div className="text-2xl font-bold">{stats?.TotalCustomers ?? 0}</div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">Operações Realizadas</CardTitle>
            <ArrowRightLeft className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <div className="text-2xl font-bold">{stats?.TotalOperations ?? 0}</div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">Volume de Câmbio (R$)</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <div className="text-2xl font-bold">
                {(stats?.TotalExchangeVolume ?? 0).toLocaleString("pt-BR", {
                  style: "currency",
                  currency: "BRL",
                })}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Cotações Recentes</CardTitle>
            <CardDescription>Últimas cotações de moedas estrangeiras</CardDescription>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="space-y-2">
                <Skeleton className="h-12 w-full" />
                <Skeleton className="h-12 w-full" />
                <Skeleton className="h-12 w-full" />
              </div>
            ) : exchangeRates && exchangeRates.length > 0 ? (
              <div className="space-y-4">
                {exchangeRates.map((rate) => (
                  <div key={`${rate.FromCurrencyCode}-${rate.ToCurrencyCode}`} className="flex items-center justify-between border-b pb-2">
                    <div className="font-medium">
                      {rate.ToCurrencyName} ({rate.ToCurrencyCode})
                    </div>
                    <div className="font-bold">
                      R${rate.Rate.toLocaleString("pt-BR", {
                        minimumFractionDigits: 4,
                        maximumFractionDigits: 4
                      })}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-8 text-muted-foreground">
                Nenhuma cotação disponível
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Operações Recentes</CardTitle>
            <CardDescription>Últimas operações de câmbio realizadas</CardDescription>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="space-y-2">
                <Skeleton className="h-12 w-full" />
                <Skeleton className="h-12 w-full" />
                <Skeleton className="h-12 w-full" />
              </div>
            ) : recentOperations && recentOperations.length > 0 ? (
              <div className="space-y-4">
                {recentOperations.map((operation) => (
                  <div key={operation.Id} className="flex items-center justify-between border-b pb-2">
                    <div>
                      <div className="font-medium">{operation.CustomerName}</div>
                      <div className="text-sm text-muted-foreground">
                        {operation.FromCurrencyCode} → {operation.ToCurrencyCode}
                      </div>
                      <div className="text-xs text-muted-foreground">
                        Taxa: {operation.ExchangeRate.toFixed(4)}
                      </div>
                    </div>
                    <div>
                      <div className="text-right font-bold">
                        {operation.Amount.toLocaleString("pt-BR", {
                          style: "currency",
                          currency: operation.FromCurrencyCode,
                          minimumFractionDigits: 4,
                          maximumFractionDigits: 4
                        })}
                      </div>
                      <div className="text-right text-sm text-muted-foreground">
                        {operation.FinalAmount.toLocaleString("pt-BR", {
                          style: "currency",
                          currency: operation.ToCurrencyCode,
                          minimumFractionDigits: 4,
                          maximumFractionDigits: 4
                        })}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-8 text-muted-foreground">
                Nenhuma operação encontrada
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
