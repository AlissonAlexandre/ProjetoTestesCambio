"use client"

import type React from "react"

import { useState, useEffect } from "react"
import { useRouter, useSearchParams } from "next/navigation"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useToast } from "@/hooks/use-toast"
import { api } from "@/lib/api"
import { Skeleton } from "@/components/ui/skeleton"
import { Search, ArrowRight } from "lucide-react"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"

interface Customer {
  Id: number
  Name: string
  Document: string
}

interface CustomerLimit {
  Id: number
  CustomerId: number
  Limit: number
}

interface Currency {
  Id: number
  Code: string
  Name: string
}

export default function NovaOperacaoPage() {
  const [searchTerm, setSearchTerm] = useState("")
  const [searchResults, setSearchResults] = useState<Customer[]>([])
  const [showCustomerDialog, setShowCustomerDialog] = useState(false)
  const [customer, setCustomer] = useState<Customer | null>(null)
  const [customerLimit, setCustomerLimit] = useState<CustomerLimit | null>(null)
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [fromCurrency, setFromCurrency] = useState<string>("")
  const [toCurrency, setToCurrency] = useState<string>("")
  const [amount, setAmount] = useState<string>("0.000")
  const [rate, setRate] = useState<number | null>(null)
  const [totalAmount, setTotalAmount] = useState<number | null>(null)
  const [brlRate, setBrlRate] = useState<number | null>(null)
  const [loading, setLoading] = useState(false)
  const [currenciesLoading, setCurrenciesLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const { toast } = useToast()
  const router = useRouter()
  const searchParams = useSearchParams()

  
  useEffect(() => {
    const fetchCurrencies = async () => {
      setCurrenciesLoading(true)
      try {
        const response = await api.get("/api/Currency/rates")
        const uniqueCurrencies = response.data?.Rates?.reduce((acc: Currency[], rate: any) => {
          
          if (!acc.some((c: Currency) => c.Id === rate.FromCurrencyId)) {
            acc.push({
              Id: rate.FromCurrencyId,
              Code: rate.FromCurrencyCode,
              Name: rate.FromCurrencyName
            });
          }
          
          if (!acc.some((c: Currency) => c.Id === rate.ToCurrencyId)) {
            acc.push({
              Id: rate.ToCurrencyId,
              Code: rate.ToCurrencyCode,
              Name: rate.ToCurrencyName
            });
          }
          return acc;
        }, []) || [];

        setCurrencies(uniqueCurrencies)

        
        const brlCurrency = uniqueCurrencies.find((c: Currency) => c.Name.includes("Real"))
        const usdCurrency = uniqueCurrencies.find((c: Currency) => c.Name.includes("Dólar"))

        if (brlCurrency) {
          setFromCurrency(`${brlCurrency.Name} (${brlCurrency.Code})`)
        }

        if (usdCurrency) {
          setToCurrency(`${usdCurrency.Name} (${usdCurrency.Code})`)
        }
      } catch (error) {
        toast({
          title: "Erro ao carregar moedas",
          description: "Não foi possível carregar a lista de moedas disponíveis.",
          variant: "destructive",
        })
      } finally {
        setCurrenciesLoading(false)
      }
    }

    fetchCurrencies()
  }, [])

  
  useEffect(() => {
    const clienteId = searchParams.get("clienteId")
    if (clienteId) {
      const fetchCustomer = async () => {
        setLoading(true)
        try {
          const response = await api.get(`/api/Customer/${clienteId}`)
          setCustomer(response.data)
          fetchCustomerLimit(response.data.Id)
        } catch (error) {
          toast({
            title: "Erro ao carregar cliente",
            description: "Não foi possível carregar os dados do cliente.",
            variant: "destructive",
          })
        } finally {
          setLoading(false)
        }
      }

      fetchCustomer()
    }
  }, [searchParams])

  const handleAmountChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value.replace(/[^0-9]/g, '');
    
    if (value === '') {
      setAmount('0.000');
      return;
    }

    
    const numericValue = parseInt(value, 10) / 1000;
    setAmount(numericValue.toFixed(3));
  }

  
  useEffect(() => {
    if (toCurrency) {
      const getBrlQuote = async () => {
        try {
          const toCurrencyCode = toCurrency.match(/\(([^)]+)\)/)?.[1]
          const toCurrencyId = currencies.find((c: Currency) => c.Code === toCurrencyCode)?.Id
          const brlCurrencyId = currencies.find((c: Currency) => c.Code === "BRL")?.Id

          if (!toCurrencyId || !brlCurrencyId) {
            return
          }

          
          if (toCurrencyCode === "BRL") {
            setBrlRate(1)
            return
          }

          
          const brlResponse = await api.get(`/api/Currency/quote/${toCurrencyId}/${brlCurrencyId}`)
          let brlQuoteRate = brlResponse.data?.rate || 0
          setBrlRate(brlQuoteRate)
        } catch (error) {
          console.error("Erro ao obter cotação do Real:", error)
          toast({
            title: "Erro ao obter cotação",
            description: "Não foi possível obter a cotação do Real.",
            variant: "destructive",
          })
        }
      }

      getBrlQuote()
    } else {
      setBrlRate(null)
    }
  }, [toCurrency, currencies])

  
  useEffect(() => {
    if (fromCurrency && toCurrency && amount) {
      const getQuote = async () => {
        try {
          
          const fromCurrencyCode = fromCurrency.match(/\(([^)]+)\)/)?.[1]
          const toCurrencyCode = toCurrency.match(/\(([^)]+)\)/)?.[1]

          const fromCurrencyId = currencies.find((c: Currency) => c.Code === fromCurrencyCode)?.Id
          const toCurrencyId = currencies.find((c: Currency) => c.Code === toCurrencyCode)?.Id

          if (!fromCurrencyId || !toCurrencyId) {
            return
          }

          
          const response = await api.get(`/api/Currency/quote/${toCurrencyId}/${fromCurrencyId}`)
          let quoteRate = response.data?.rate || 0
          
          
          if (toCurrencyCode === "BRL") {
            quoteRate = 1 / quoteRate
          }
          
          setRate(quoteRate)

          const amountValue = Number.parseFloat(amount)
          if (!isNaN(amountValue) && amountValue > 0) {
            setTotalAmount(amountValue * quoteRate)
          }
        } catch (error) {
          console.error("Erro ao obter cotação:", error)
          toast({
            title: "Erro ao obter cotação",
            description: "Não foi possível obter a cotação atual.",
            variant: "destructive",
          })
        }
      }

      getQuote()
    } else {
      setRate(null)
      setTotalAmount(null)
    }
  }, [fromCurrency, toCurrency, amount, currencies])

  const fetchCustomerLimit = async (customerId: number) => {
    try {
      const response = await api.get(`/api/CustomerLimit/${customerId}`)
      console.log("Response do limite:", response.data)
      
      if (response.data) {
        const limitData = Array.isArray(response.data) ? response.data[0] : response.data
        console.log("Limit data processado:", limitData)
        
        
        const processedLimit = {
          Id: limitData?.Id || 0,
          CustomerId: limitData?.CustomerId || customerId,
          Limit: limitData?.Limit || 0,
        }
        console.log("Limite processado final:", processedLimit)
        setCustomerLimit(processedLimit)
      } else {
        console.log("Nenhum dado de limite recebido")
        setCustomerLimit(null)
      }
    } catch (error) {
      console.error("Erro ao buscar limite:", error)
      toast({
        title: "Erro ao carregar limite",
        description: "Não foi possível carregar o limite do cliente.",
        variant: "destructive",
      })
      setCustomerLimit(null)
    }
  }

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!searchTerm) {
      toast({
        title: "Campo obrigatório",
        description: "Digite um CPF, CNPJ ou nome para pesquisar.",
        variant: "destructive",
      })
      return
    }

    setLoading(true)
    try {
      
      try {
        const documentResponse = await api.get(`/api/Customer/document/${searchTerm}`)
        setCustomer(documentResponse.data)
        fetchCustomerLimit(documentResponse.data.Id)
        setSearchResults([])
      } catch (error: any) {
        
        if (error.response?.status === 404) {
          const searchResponse = await api.get("/api/Customer/search", {
            params: { searchTerm },
          })

          if (searchResponse.data?.Items && searchResponse.data.Items.length > 0) {
            setSearchResults(searchResponse.data.Items)
            setShowCustomerDialog(true)
          } else {
            toast({
              title: "Cliente não encontrado",
              description: "Nenhum cliente encontrado com os dados informados.",
              variant: "destructive",
            })
            setCustomer(null)
            setCustomerLimit(null)
            setSearchResults([])
          }
        } else {
          throw error
        }
      }
    } catch (error) {
      toast({
        title: "Erro ao pesquisar cliente",
        description: "Ocorreu um erro ao pesquisar o cliente.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  const handleSelectCustomer = (selectedCustomer: Customer) => {
    setCustomer(selectedCustomer)
    fetchCustomerLimit(selectedCustomer.Id)
    setShowCustomerDialog(false)
    setSearchResults([])
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!customer) {
      toast({
        title: "Cliente não selecionado",
        description: "Selecione um cliente para realizar a operação.",
        variant: "destructive",
      })
      return
    }

    if (!fromCurrency || !toCurrency) {
      toast({
        title: "Moedas não selecionadas",
        description: "Selecione as moedas para a operação.",
        variant: "destructive",
      })
      return
    }

    const amountValue = Number.parseFloat(amount)
    if (isNaN(amountValue) || amountValue <= 0) {
      toast({
        title: "Valor inválido",
        description: "Insira um valor válido para a operação.",
        variant: "destructive",
      })
      return
    }

    const availableLimit = customerLimit?.Limit || 0
    if (totalAmount && totalAmount > availableLimit) {
      toast({
        title: "Limite excedido",
        description: "O valor da operação excede o limite disponível do cliente.",
        variant: "destructive",
      })
      return
    }

    if (!customerLimit) {
      toast({
        title: "Limite não cadastrado",
        description: "O cliente não possui limite cadastrado. Por favor, cadastre um limite antes de realizar a operação.",
        variant: "destructive",
      })
      return
    }

    setSubmitting(true)
    try {
      
      const fromCurrencyCode = fromCurrency.match(/\(([^)]+)\)/)?.[1]
      const toCurrencyCode = toCurrency.match(/\(([^)]+)\)/)?.[1]

      
      const fromCurrencyId = currencies.find((c: Currency) => c.Code === fromCurrencyCode)?.Id
      const toCurrencyId = currencies.find((c: Currency) => c.Code === toCurrencyCode)?.Id

      if (!fromCurrencyId || !toCurrencyId) {
        throw new Error("IDs das moedas não encontrados")
      }

      await api.post("/api/ExchangeOperation", {
        CustomerId: customer.Id,
        FromCurrencyCode: fromCurrencyCode,
        ToCurrencyCode: toCurrencyCode,
        Amount: amountValue,
      })

      toast({
        title: "Operação realizada com sucesso",
        description: "A operação de câmbio foi registrada com sucesso.",
      })

      router.push("/dashboard/operacoes")
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || "Ocorreu um erro ao processar a operação."
      toast({
        title: "Erro ao realizar operação",
        description: errorMessage,
        variant: "destructive",
      })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Nova Operação de Câmbio</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Pesquisar Cliente</CardTitle>
          <CardDescription>Pesquise o cliente pelo CPF, CNPJ ou nome</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSearch} className="flex items-center space-x-2">
            <Input
              placeholder="CPF, CNPJ ou nome do cliente"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="flex-1"
            />
            <Button type="submit" disabled={loading}>
              <Search className="mr-2 h-4 w-4" />
              Pesquisar
            </Button>
          </form>

          {loading ? (
            <div className="mt-4">
              <Skeleton className="h-20 w-full" />
            </div>
          ) : customer ? (
            <div className="mt-4 rounded-lg border p-4">
              <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <h3 className="font-medium">{customer.Name}</h3>
                  <p className="text-sm text-muted-foreground">{customer.Document}</p>
                </div>
                <div className="mt-2 sm:mt-0 text-right">
                  {customerLimit && customerLimit.Limit !== undefined ? (
                    <>
                      <div className="text-sm text-muted-foreground">Limite Disponível:</div>
                      <div className="font-bold">
                        {(customerLimit.Limit || 0).toLocaleString("pt-BR", {
                          style: "currency",
                          currency: "BRL",
                        })}
                      </div>
                    </>
                  ) : (
                    <div className="text-sm text-red-500">
                      Limite não cadastrado
                    </div>
                  )}
                </div>
              </div>
            </div>
          ) : null}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Detalhes da Operação</CardTitle>
          <CardDescription>Preencha os detalhes da operação de câmbio</CardDescription>
        </CardHeader>
        <form onSubmit={handleSubmit}>
          <CardContent className="space-y-4">
            {currenciesLoading ? (
              <div className="space-y-4">
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
              </div>
            ) : (
              <>
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="fromCurrency">Moeda de Origem</Label>
                    <Select value={fromCurrency} onValueChange={setFromCurrency}>
                      <SelectTrigger id="fromCurrency">
                        <SelectValue placeholder="Selecione a moeda" />
                      </SelectTrigger>
                      <SelectContent>
                        {currencies && currencies.length > 0 ? (
                          currencies
                            .filter((currency) => currency.Name !== (toCurrency?.split(" (")[0] || ""))
                            .map((currency) => (
                              <SelectItem key={currency.Id} value={`${currency.Name} (${currency.Code})`}>
                                {currency.Name} ({currency.Code})
                              </SelectItem>
                            ))
                        ) : (
                          <SelectItem value="no-currency" disabled>
                            Nenhuma moeda disponível
                          </SelectItem>
                        )}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="toCurrency">Moeda de Destino</Label>
                    <Select value={toCurrency} onValueChange={setToCurrency}>
                      <SelectTrigger id="toCurrency">
                        <SelectValue placeholder="Selecione a moeda" />
                      </SelectTrigger>
                      <SelectContent>
                        {currencies && currencies.length > 0 ? (
                          currencies
                            .filter((currency) => currency.Name !== (fromCurrency?.split(" (")[0] || ""))
                            .map((currency) => (
                              <SelectItem key={currency.Id} value={`${currency.Name} (${currency.Code})`}>
                                {currency.Name} ({currency.Code})
                              </SelectItem>
                            ))
                        ) : (
                          <SelectItem value="no-currency" disabled>
                            Nenhuma moeda disponível
                          </SelectItem>
                        )}
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="amount">Valor ({toCurrency ? toCurrency.match(/\(([^)]+)\)/)?.[1] : ''})</Label>
                  <Input
                    id="amount"
                    type="text"
                    inputMode="numeric"
                    value={amount}
                    onChange={handleAmountChange}
                    placeholder={`0.000 ${toCurrency ? toCurrency.match(/\(([^)]+)\)/)?.[1] : ''}`}
                  />
                </div>

                {rate !== null && totalAmount !== null && (
                  <div className="rounded-lg border p-4">
                    <div className="mb-2 text-sm font-medium text-muted-foreground">Taxa de conversão:</div>
                    <div className="mb-4 text-xl font-bold">
                      1 {fromCurrency.match(/\(([^)]+)\)/)?.[1]} = {rate.toLocaleString("pt-BR", { minimumFractionDigits: 4, maximumFractionDigits: 4 })} {toCurrency.match(/\(([^)]+)\)/)?.[1]}
                    </div>
                    <div className="flex items-center justify-between">
                      <div className="text-lg">
                        {(Number.parseFloat(amount) / rate).toLocaleString("pt-BR", { minimumFractionDigits: 3, maximumFractionDigits: 3 })} {fromCurrency.match(/\(([^)]+)\)/)?.[1]}
                      </div>
                      <ArrowRight className="mx-2 h-4 w-4 text-muted-foreground" />
                      <div className="text-lg font-bold">
                        {Number.parseFloat(amount).toLocaleString("pt-BR", { minimumFractionDigits: 3, maximumFractionDigits: 3 })} {toCurrency.match(/\(([^)]+)\)/)?.[1]}
                      </div>
                    </div>

                    {customerLimit && customerLimit.Limit !== undefined && (
                      <div className="mt-4 pt-4 border-t space-y-3">
                        <div className="flex justify-between items-center">
                          <span className="text-sm text-muted-foreground">Limite Disponível:</span>
                          <span className="font-medium">
                            {(customerLimit.Limit || 0).toLocaleString("pt-BR", {
                              style: "currency",
                              currency: "BRL",
                              minimumFractionDigits: 3,
                              maximumFractionDigits: 3
                            })}
                          </span>
                        </div>

                        <div className="flex justify-between items-center">
                          <span className="text-sm text-muted-foreground">Valor da Operação em BRL:</span>
                          <span className="font-medium">
                            {((Number.parseFloat(amount) / (brlRate || 0)) || 0).toLocaleString("pt-BR", {
                              style: "currency",
                              currency: "BRL",
                              minimumFractionDigits: 3,
                              maximumFractionDigits: 3
                            })}
                          </span>
                        </div>

                        <div className="flex justify-between items-center">
                          <span className="text-sm text-muted-foreground">Valor da Operação Origem (em {fromCurrency ? fromCurrency.match(/\(([^)]+)\)/)?.[1] : ''}):</span>
                          <span className="font-medium">
                            {((Number.parseFloat(amount) / rate) || 0).toLocaleString("pt-BR", {
                              style: "currency",
                              currency: fromCurrency ? fromCurrency.match(/\(([^)]+)\)/)?.[1] : '',
                              minimumFractionDigits: 3,
                              maximumFractionDigits: 3
                            })}
                          </span>
                        </div>

                        <div className="flex justify-between items-center">
                          <span className="text-sm text-muted-foreground">Valor da Operação Destino (em {toCurrency ? toCurrency.match(/\(([^)]+)\)/)?.[1] : ''}):</span>
                          <span className="font-medium">
                            {(Number.parseFloat(amount) || 0).toLocaleString("pt-BR", {
                              style: "currency",
                              currency: toCurrency ? toCurrency.match(/\(([^)]+)\)/)?.[1] : '',
                              minimumFractionDigits: 3,
                              maximumFractionDigits: 3
                            })}
                          </span>
                        </div>

                        <div className="flex justify-between items-center pt-2 border-t">
                          <span className="text-sm font-medium">Limite Restante:</span>
                          <span
                            className={`font-bold ${((customerLimit.Limit || 0) - ((Number.parseFloat(amount) * (brlRate || 0)) || 0)) < 0 ? "text-red-500" : ""}`}
                          >
                            {Math.max(0, (customerLimit.Limit || 0) - ((Number.parseFloat(amount) / (brlRate || 0)) || 0)).toLocaleString("pt-BR", {
                              style: "currency",
                              currency: "BRL",
                              minimumFractionDigits: 3,
                              maximumFractionDigits: 3
                            })}
                          </span>
                        </div>
                      </div>
                    )}
                  </div>
                )}
              </>
            )}
          </CardContent>
          <CardFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => router.push("/dashboard/operacoes")}
              className="mr-2"
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              disabled={submitting || !customer || !fromCurrency || !toCurrency || !amount || !rate}
            >
              {submitting ? "Processando..." : "Confirmar Operação"}
            </Button>
          </CardFooter>
        </form>
      </Card>

      <Dialog open={showCustomerDialog} onOpenChange={setShowCustomerDialog}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>Selecionar Cliente</DialogTitle>
            <DialogDescription>
              Múltiplos clientes encontrados. Selecione o cliente desejado.
            </DialogDescription>
          </DialogHeader>
          <div className="max-h-[400px] overflow-y-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead>
                  <TableHead>Documento</TableHead>
                  <TableHead className="text-right">Ação</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {searchResults.map((result) => (
                  <TableRow key={result.Id}>
                    <TableCell>{result.Name}</TableCell>
                    <TableCell>{result.Document}</TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => handleSelectCustomer(result)}
                      >
                        Selecionar
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
