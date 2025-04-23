"use client"

import type React from "react"

import { useState, useEffect, use } from "react"
import { useRouter, useParams } from "next/navigation"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useToast } from "@/hooks/use-toast"
import { api } from "@/lib/api"
import { useAuth } from "@/contexts/auth-context"
import { Skeleton } from "@/components/ui/skeleton"

interface CustomerLimit {
  id: number
  customerId: number
  limit: number
  createdAt?: string
  lastUpdatedAt?: string
}

interface Customer {
  Id: number
  Name: string
  Document: string
}

export default function LimitePage() {
  const params = useParams()
  const [customer, setCustomer] = useState<Customer | null>(null)
  const customerId = Number.parseInt(params?.id as string)
  const [limit, setLimit] = useState<CustomerLimit>({
    id: 0,
    customerId,
    limit: 0,
    createdAt: "2024-01-01",
    lastUpdatedAt: "2024-01-01"
  })
  const [newLimit, setNewLimit] = useState<string>("0")
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const { toast } = useToast()
  const router = useRouter()
  const { user } = useAuth()
  const [IsMaster, setIsMaster] = useState(false)

  useEffect(() => {
    setIsMaster(user?.IsMaster || false)
  }, [user])

  useEffect(() => {
    if (user === undefined) {
      
      console.log("É UNDEFINED!!!")
      return
    }
    if (!user?.IsMaster) {
      console.log("É MASTER!!!")
      router.push("/dashboard/clientes")
    }
  }, [IsMaster, router])

  useEffect(() => {
    if (isNaN(customerId)) {
      console.log("ID do cliente inválido:", customerId)
      router.push("/dashboard/clientes")
      return
    }
  }, [customerId, router])

  const fetchData = async () => {
    setLoading(true)
    try {
      const customerResponse = await api.get(`/api/Customer/${customerId}`)
      console.log("Customer data:", customerResponse.data)
      if (customerResponse.data) {
        setCustomer({
          Id: customerResponse.data.id || customerResponse.data.Id,
          Name: customerResponse.data.name || customerResponse.data.Name,
          Document: customerResponse.data.document || customerResponse.data.Document
        })
      }

      try {
        const limitResponse = await api.get(`/api/CustomerLimit/${customerId}`)
        console.log("Limit response:", limitResponse)
        console.log("Limit data:", limitResponse.data)

        if (limitResponse.data) {
          const limitData = Array.isArray(limitResponse.data) 
            ? limitResponse.data[0] 
            : limitResponse.data


          if (limitData && typeof limitData === 'object') {
            const newLimit = {
              id:  limitData.Id || 0,
              customerId: limitData.CustomerId || customerId,
              limit:  limitData.Limit || 0,
              createdAt:  limitData.CreatedAt || "2024-01-01",
              lastUpdatedAt: limitData.LastUpdatedAt || "2024-01-01"
            }
            console.log("Setting new limit:", newLimit)
            setLimit(newLimit)
            setNewLimit((newLimit.limit || 0).toString())
          } else {
            setDefaultLimit()
          }
        } else {
          setDefaultLimit()
        }
      } catch (error: any) {
        console.log("Erro ao buscar limite:", error)
        console.log("Status do erro:", error.response?.status)
        console.log("Dados do erro:", error.response?.data)
        setDefaultLimit()
      }
    } catch (error) {
      console.error("Erro ao carregar dados:", error)
      toast({
        title: "Erro ao carregar dados",
        description: "Não foi possível carregar os dados do cliente e seu limite.",
        variant: "destructive"
      })
      setDefaultLimit()
    } finally {
      setLoading(false)
    }
  }

  const setDefaultLimit = () => {
    console.log("Definindo limite padrão")
    setLimit({
      id: 0,
      customerId: customerId,
      limit: 0,
      createdAt: "2024-01-01",
      lastUpdatedAt: "2024-01-01"
    })
    setNewLimit("0")
    console.log("Limite padrão definido:", limit)
  }

  useEffect(() => {
    fetchData()
  }, [customerId, router])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    const limitValue = Number.parseFloat(newLimit)
    if (isNaN(limitValue) || limitValue < 0) {
      toast({
        title: "Valor inválido",
        description: "Por favor, insira um valor de limite válido.",
        variant: "destructive"
      })
      return
    }

    setSubmitting(true)
    console.log("Submetendo limite:", {
      id: limit.id,
      customerId,
      limitValue
    })

    try {
      if (limit.id !== 0) {
        console.log("Atualizando limite existente")
        await api.put("/api/CustomerLimit", {
          limitId: limit.id,
          newLimit: limitValue,
        })

        toast({
          title: "Limite atualizado",
          description: "O limite do cliente foi atualizado com sucesso."
        })
      } else {
        console.log("Criando novo limite")
        const response = await api.post("/api/CustomerLimit", {
          customerId,
          limit: limitValue,
        })
        console.log("Resposta da criação:", response.data)

        toast({
          title: "Limite cadastrado",
          description: "O limite do cliente foi cadastrado com sucesso."
        })
      }

      await fetchData()
    } catch (error: any) {
      console.error("Erro ao processar limite:", error)
      console.log("Status do erro:", error.response?.status)
      console.log("Dados do erro:", error.response?.data)
      
      const errorMessage = error.response?.data?.message || "Ocorreu um erro ao processar o limite."
      toast({
        title: "Erro ao processar limite",
        description: errorMessage,
        variant: "destructive"
      })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Gerenciar Limite</h1>
      </div>

      <Card className="max-w-2xl mx-auto">
        <CardHeader>
          <CardTitle>
            {loading ? (
              <Skeleton className="h-8 w-48" />
            ) : (
              `Limite de ${customer?.Name || 'Cliente'}`
            )}
          </CardTitle>
          <CardDescription>
            {loading ? (
              <Skeleton className="h-4 w-72" />
            ) : (
              `Gerencie o limite de operações para o cliente ${customer?.Document || ''}`
            )}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="space-y-4">
              <Skeleton className="h-20 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
          ) : (
            <div className="space-y-6">
              {limit?.limit != 0 ? (
                <div className="rounded-lg border p-4">
                  <div className="text-sm font-medium text-muted-foreground">Limite Atual</div>
                  <div className="mt-1 text-2xl font-bold">
                    {(limit?.limit || 0).toLocaleString("pt-BR", {
                      style: "currency",
                      currency: "BRL",
                    })}
                  </div>
                  {limit?.lastUpdatedAt && typeof limit.lastUpdatedAt === 'string' && (
                    <div className="mt-2 text-xs text-muted-foreground">
                      Última atualização: {new Date(limit.lastUpdatedAt).toLocaleString("pt-BR")}
                    </div>
                  )}
                </div>
              ) : (
                <div className="rounded-lg border p-4 bg-muted">
                  <div className="text-sm text-center text-muted-foreground">
                    Nenhum limite cadastrado. Defina um limite para este cliente.
                  </div>
                </div>
              )}

              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="limit">{limit.id !== 0 ? "Atualizar Limite" : "Definir Limite"}</Label>
                  <div className="flex items-center">
                    <span className="bg-muted px-3 py-2 rounded-l-md border border-r-0">R$</span>
                    <Input
                      id="limit"
                      type="number"
                      min="0"
                      step="0.01"
                      value={newLimit}
                      onChange={(e) => setNewLimit(e.target.value)}
                      className="rounded-l-none"
                      required
                    />
                  </div>
                </div>

                <div className="flex justify-end space-x-2">
                  <Button type="button" variant="outline" onClick={() => router.push("/dashboard/clientes")}>
                    Cancelar
                  </Button>
                  <Button type="submit" disabled={submitting}>
                    {submitting ? "Processando..." : limit.id !== 0 ? "Atualizar" : "Cadastrar"}
                  </Button>
                </div>
              </form>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
