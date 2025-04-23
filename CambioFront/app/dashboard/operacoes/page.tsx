"use client"

import type React from "react"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { useToast } from "@/hooks/use-toast"
import { api } from "@/lib/api"
import { Plus, Search, RefreshCw, Edit2 } from "lucide-react"
import { Skeleton } from "@/components/ui/skeleton"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { format } from "date-fns"
import { ptBR } from "date-fns/locale"

interface ExchangeOperation {
  Id: number
  CustomerId: number
  CustomerName: string
  CustomerDocument: string
  FromCurrencyId: number
  FromCurrencyCode: string
  ToCurrencyId: number
  ToCurrencyCode: string
  Amount: number
  ExchangeRate: number
  FinalAmount: number
  Status: number
  CreatedAt: string
  CreatedById: number
  CreatedByUserName: string
}

export default function OperacoesPage() {
  const [operations, setOperations] = useState<ExchangeOperation[]>([])
  const [document, setDocument] = useState("")
  const [status, setStatus] = useState("")
  const [loading, setLoading] = useState(true)
  const { toast } = useToast()
  const router = useRouter()

  const fetchOperations = async () => {
    setLoading(true)
    try {
      const params: any = {}

      if (document) {
        params.customerDocument = document
      }

      if (status) {
        params.status = Number.parseInt(status)
      }

      const response = await api.get("/api/ExchangeOperation/search", { params })
      console.log("Operações carregadas:", response.data.Operations)
      setOperations(response.data?.Operations || [])
    } catch (error) {
      toast({
        title: "Erro ao carregar operações",
        description: "Não foi possível carregar a lista de operações.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchOperations()
  }, [])

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    fetchOperations()
  }

  const handleEditOperation = (operation: ExchangeOperation) => {
    router.push(`/dashboard/operacoes/${operation.Id}/editar`)
  }

  const getStatusText = (status: number) => {
    switch (status) {
      case 0:
        return "Pendente"
      case 1:
        return "Concluída"
      case 2:
        return "Modificada"
      case 3:
        return "Excluída"
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
        return "text-blue-600 bg-blue-100 px-2 py-1 rounded-full text-xs"
      case 3:
        return "text-red-600 bg-red-100 px-2 py-1 rounded-full text-xs"
      default:
        return "text-gray-600 bg-gray-100 px-2 py-1 rounded-full text-xs"
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Operações de Câmbio</h1>
        <Button onClick={() => router.push("/dashboard/operacoes/nova")}>
          <Plus className="mr-2 h-4 w-4" />
          Nova Operação
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Gerenciar Operações</CardTitle>
          <CardDescription>Visualize, pesquise e gerencie as operações de câmbio</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSearch} className="flex flex-wrap items-center gap-2 mb-6">
            <Input
              placeholder="CPF/CNPJ do cliente"
              value={document}
              onChange={(e) => setDocument(e.target.value)}
              className="flex-1 min-w-[200px]"
            />
            <Select value={status} onValueChange={setStatus}>
              <SelectTrigger className="w-[180px]">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="-1">Todos</SelectItem>
                <SelectItem value="0">Pendente</SelectItem>
                <SelectItem value="1">Concluída</SelectItem>
                <SelectItem value="2">Modificada</SelectItem>
                <SelectItem value="3">Excluída</SelectItem>
              </SelectContent>
            </Select>
            <Button type="submit">
              <Search className="mr-2 h-4 w-4" />
              Pesquisar
            </Button>
            <Button type="button" variant="outline" onClick={fetchOperations}>
              <RefreshCw className="h-4 w-4" />
            </Button>
          </form>

          {loading ? (
            <div className="space-y-2">
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
            </div>
          ) : operations.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground">Nenhuma operação encontrada.</p>
            </div>
          ) : (
            <div className="rounded-md border overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>ID</TableHead>
                    <TableHead>Cliente</TableHead>
                    <TableHead>Moedas</TableHead>
                    <TableHead>Valor</TableHead>
                    <TableHead>Data</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Ações</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {operations.map((operation) => (
                    <TableRow key={operation.Id}>
                      <TableCell>{operation.Id}</TableCell>
                      <TableCell>
                        <div>{operation.CustomerName}</div>
                        <div className="text-xs text-muted-foreground">{operation.CustomerDocument}</div>
                      </TableCell>
                      <TableCell>
                        {operation.FromCurrencyCode} → {operation.ToCurrencyCode}
                      </TableCell>
                      <TableCell>
                        <div>
                          {operation.Amount.toLocaleString("pt-BR", {
                            style: "currency",
                            currency: operation.ToCurrencyCode,
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                          })}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          {operation.FinalAmount.toLocaleString("pt-BR", {
                            style: "currency",
                            currency: operation.FromCurrencyCode,
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                          })}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          Taxa: {operation.ExchangeRate.toFixed(4)}
                        </div>
                      </TableCell>
                      <TableCell>
                        {format(new Date(operation.CreatedAt), "dd/MM/yyyy HH:mm", { locale: ptBR })}
                      </TableCell>
                      <TableCell>
                        <span className={getStatusClass(operation.Status)}>{getStatusText(operation.Status)}</span>
                      </TableCell>
                      <TableCell className="text-right">
                        <Button 
                          variant="outline" 
                          size="sm" 
                          onClick={() => handleEditOperation(operation)}
                          disabled={operation.Status === 3}
                        >
                          <Edit2 className="mr-2 h-4 w-4" />
                          Editar
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
