"use client"

import type React from "react"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { useToast } from "@/hooks/use-toast"
import { api } from "@/lib/api"
import { Plus, Search, RefreshCw, User, DollarSign, Pencil } from "lucide-react"
import { Skeleton } from "@/components/ui/skeleton"
import { useAuth } from "@/contexts/auth-context"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"

interface Customer {
  Id: number
  Name: string
  Document: string
  Phone: string
  Email: string
}

interface PaginatedResponse<T> {
  Items: T[]
  TotalItems: number
  PageNumber: number
  TotalPages: number
  HasNextPage: boolean
  HasPreviousPage: boolean
}

const noRetryRoutes = ["/api/Auth/refresh-token", "/api/Auth/login", "/api/User/password"]

export default function ClientesPage() {
  const [customers, setCustomers] = useState<Customer[]>([])
  const [searchTerm, setSearchTerm] = useState("")
  const [loading, setLoading] = useState(true)
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [pageSize] = useState(10)
  const [isTransitioning, setIsTransitioning] = useState(false)
  const { toast } = useToast()
  const router = useRouter()
  const { user } = useAuth()

  const fetchCustomers = async (page: number = currentPage) => {
    if (isTransitioning) return

    setIsTransitioning(true)
    setLoading(true)
    
    setCustomers([])

    try {
      const params: any = {
        pageNumber: page,
        pageSize
      }

      if (searchTerm.trim()) {
        params.searchTerm = searchTerm
      }

      const response = await api.get<PaginatedResponse<Customer>>("/api/Customer/search", {
        params
      })

      if (response.data?.Items) {
        setCustomers(response.data.Items)
        setTotalPages(response.data.TotalPages || 1)
        setCurrentPage(response.data.PageNumber || 1)
      } else {
        setTotalPages(1)
        setCurrentPage(1)
      }
    } catch (error) {
      setTotalPages(1)
      setCurrentPage(1)
      toast({
        title: "Erro ao carregar clientes",
        description: "Não foi possível carregar a lista de clientes.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
      setTimeout(() => {
        setIsTransitioning(false)
      }, 300)
    }
  }

  useEffect(() => {
    fetchCustomers(1)
  }, [])

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    if (!isTransitioning) {
      fetchCustomers(1)
    }
  }

  const handlePageChange = (page: number) => {
    if (!isTransitioning) {
      fetchCustomers(page)
    }
  }

  const handleViewLimit = (customerId: number) => {
    router.push(`/dashboard/clientes/${customerId}/limite`)
  }

  const handleNewOperation = (customerId: number) => {
    router.push(`/dashboard/operacoes/nova?clienteId=${customerId}`)
  }

  const handleEditCustomer = (customerId: number) => {
    router.push(`/dashboard/clientes/${customerId}/editar`)
  }

  const handleRefresh = async () => {
    if (isTransitioning) return

    const oldSearchTerm = searchTerm
    setSearchTerm("")
    
    setIsTransitioning(true)
    setLoading(true)
    
    setCustomers([])

    try {
      const response = await api.get<PaginatedResponse<Customer>>("/api/Customer/search", {
        params: {
          pageNumber: 1,
          pageSize
        }
      })
      
      if (response.data?.Items) {
        setCustomers(response.data.Items)
        setTotalPages(response.data.TotalPages || 1)
        setCurrentPage(1)
      } else {
        setTotalPages(1)
        setCurrentPage(1)
      }
    } catch (error) {
      setSearchTerm(oldSearchTerm)
      setTotalPages(1)
      setCurrentPage(1)
      toast({
        title: "Erro ao carregar clientes",
        description: "Não foi possível carregar a lista de clientes.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
      setTimeout(() => {
        setIsTransitioning(false)
      }, 300)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Clientes</h1>
        {user?.IsMaster && (
          <Button onClick={() => router.push("/dashboard/clientes/novo")}>
            <Plus className="mr-2 h-4 w-4" />
            Novo Cliente
          </Button>
        )}
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Gerenciar Clientes</CardTitle>
          <CardDescription>Visualize, pesquise e gerencie os clientes cadastrados</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSearch} className="flex items-center space-x-2 mb-6">
            <Input
              placeholder="Pesquisar por nome, CPF ou CNPJ"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="flex-1"
            />
            <Button type="submit">
              <Search className="mr-2 h-4 w-4" />
              Pesquisar
            </Button>
            <Button type="button" variant="outline" onClick={handleRefresh}>
              <RefreshCw className="h-4 w-4" />
            </Button>
          </form>

          {loading ? (
            <div className="space-y-4">
              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Nome</TableHead>
                      <TableHead>Documento</TableHead>
                      <TableHead>Telefone</TableHead>
                      <TableHead>E-mail</TableHead>
                      <TableHead className="text-right">Ações</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {Array.from({ length: 3 }).map((_, index) => (
                      <TableRow key={index}>
                        <TableCell><Skeleton className="h-4 w-[200px]" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-[120px]" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-[100px]" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-[180px]" /></TableCell>
                        <TableCell className="text-right">
                          <div className="flex justify-end space-x-2">
                            <Skeleton className="h-8 w-[80px]" />
                            <Skeleton className="h-8 w-[120px]" />
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            </div>
          ) : !customers || customers.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground">Nenhum cliente encontrado.</p>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Nome</TableHead>
                      <TableHead>Documento</TableHead>
                      <TableHead>Telefone</TableHead>
                      <TableHead>E-mail</TableHead>
                      <TableHead className="text-right">Ações</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {customers.map((customer) => (
                      <TableRow key={customer.Id}>
                        <TableCell className="font-medium">{customer.Name}</TableCell>
                        <TableCell>{customer.Document}</TableCell>
                        <TableCell>{customer.Phone}</TableCell>
                        <TableCell>{customer.Email}</TableCell>
                        <TableCell className="text-right">
                          <div className="flex justify-end space-x-2">
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleViewLimit(customer.Id)}
                            >
                              <User className="mr-2 h-4 w-4" />
                              Limite
                            </Button>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => router.push(`/dashboard/clientes/${customer.Id}`)}
                            >
                              <Pencil className="mr-2 h-4 w-4" />
                              Editar
                            </Button>
                            <Button
                              variant="default"
                              size="sm"
                              onClick={() => handleNewOperation(customer.Id)}
                            >
                              <DollarSign className="mr-2 h-4 w-4" />
                              Nova Operação
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>

              <div className="flex items-center justify-between">
                <div className="text-sm text-muted-foreground">
                  Página {currentPage} de {totalPages}
                </div>
                <div className="flex space-x-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handlePageChange(currentPage - 1)}
                    disabled={currentPage === 1}
                  >
                    Anterior
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handlePageChange(currentPage + 1)}
                    disabled={currentPage === totalPages}
                  >
                    Próxima
                  </Button>
                </div>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
