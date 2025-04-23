"use client"

import { useEffect, useState } from "react"
import { useRouter, useParams } from "next/navigation"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useToast } from "@/hooks/use-toast"
import { api } from "@/lib/api"
import { Skeleton } from "@/components/ui/skeleton"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { ArrowRight } from "lucide-react"

interface Currency {
  Id: number
  Code: string
  Name: string
}

interface ExchangeOperation {
  Id: number
  CustomerId: number
  CustomerName: string
  CustomerDocument: string
  FromCurrency: string
  ToCurrency: string
  Amount: number
  ExchangeRate: number
  FinalAmount: number
  Status: number
}

interface CustomerLimit {
  Id: number
  CustomerId: number
  Limit: number
  UsedLimit: number
  AvailableLimit: number
}

export default function EditarOperacaoPage() {
  const [operation, setOperation] = useState<ExchangeOperation | null>(null)
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [fromCurrency, setFromCurrency] = useState<string>("")
  const [toCurrency, setToCurrency] = useState<string>("")
  const [amount, setAmount] = useState<string>("0.000")
  const [rate, setRate] = useState<number | null>(null)
  const [totalAmount, setTotalAmount] = useState<number | null>(null)
  const [brlRate, setBrlRate] = useState<number | null>(null)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [customerLimit, setCustomerLimit] = useState<CustomerLimit | null>(null)
  const { toast } = useToast()
  const router = useRouter()
  const params = useParams()

  
  const [brlAmount, setBrlAmount] = useState<number>(0);
  const [oldBrlAmount, setOldBrlAmount] = useState<number>(0);

  useEffect(() => {
    const fetchOperation = async () => {
      try {
        const response = await api.get(`/api/ExchangeOperation/${params.id}`)
        console.log("Resposta completa da API:", response)
        
        
        if (!response.data) {
          console.error("Resposta da API sem dados")
          throw new Error("Resposta da API inválida")
        }

        const op = response.data.Operation || response.data
        console.log("Dados da operação:", op)
        
        if (!op || !op.CustomerDocument) {
          console.error("Dados da operação inválidos:", op)
          throw new Error("Dados da operação inválidos")
        }

        setOperation(op)
        setFromCurrency(op.FromCurrency)
        setToCurrency(op.ToCurrency)
        setAmount(op.Amount.toFixed(3))
        setRate(op.ExchangeRate)
        setTotalAmount(op.FinalAmount)

        try {
          
          const customerResponse = await api.get(`/api/Customer/document/${op.CustomerDocument}`)
          console.log("Resposta do cliente:", customerResponse)
          
          if (!customerResponse.data) {
            throw new Error("Cliente não encontrado")
          }

          
          const limitResponse = await api.get(`/api/CustomerLimit/${customerResponse.data.Id}`)
          console.log("Resposta do limite:", limitResponse)
          
          if (!limitResponse.data || !Array.isArray(limitResponse.data) || limitResponse.data.length === 0) {
            console.error("Resposta do limite sem dados")
            throw new Error("Não foi possível carregar o limite do cliente")
          }

          
          const limitData = limitResponse.data[0]
          console.log("Dados do limite:", limitData)
          
          if (!limitData || typeof limitData.Limit !== 'number') {
            throw new Error("Dados do limite inválidos")
          }

          setCustomerLimit({
            Id: limitData.Id || 0,
            CustomerId: limitData.CustomerId || customerResponse.data.Id,
            Limit: limitData.Limit,
            UsedLimit: limitData.UsedLimit || 0,
            AvailableLimit: limitData.Limit - (limitData.UsedLimit || 0)
          })
        } catch (limitError) {
          console.error("Erro ao carregar limite:", limitError)
          toast({
            title: "Erro ao carregar limite",
            description: "Não foi possível carregar o limite do cliente, mas você pode continuar editando a operação.",
            variant: "destructive",
          })
        }
      } catch (error) {
        console.error("Erro ao carregar operação:", error)
        toast({
          title: "Erro ao carregar operação",
          description: "Não foi possível carregar os dados da operação.",
          variant: "destructive",
        })
        router.push("/dashboard/operacoes")
      }
    }

    const fetchCurrencies = async () => {
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
      } catch (error) {
        toast({
          title: "Erro ao carregar moedas",
          description: "Não foi possível carregar a lista de moedas disponíveis.",
          variant: "destructive",
        })
      } finally {
        setLoading(false)
      }
    }

    fetchOperation()
    fetchCurrencies()
  }, [params.id])

  
  useEffect(() => {
    if (toCurrency) {
      const getBrlQuote = async () => {
        try {
          const toCurrencyCode = toCurrency.match(/\(([^)]+)\)/)?.[1]
          const toCurrencyId = currencies.find((c: Currency) => c.Code === toCurrency)?.Id
          const brlCurrencyId = currencies.find((c: Currency) => c.Code === "BRL")?.Id
          console.log("toCurrencyId", toCurrencyId)
          if (!toCurrencyId || !brlCurrencyId) {
            console.log('Não foi possível encontrar os IDs das moedas');
            return
          }

          
          if (toCurrencyCode === "BRL") {
            console.log('Taxa BRL -> Moeda destino:', 1);
            setBrlRate(1)
            return
          }

          
          const brlResponse = await api.get(`/api/Currency/quote/${toCurrencyId}/${brlCurrencyId}`)
          let brlQuoteRate = brlResponse.data?.rate || 0
          console.log('Taxa BRL -> Moeda destino:', brlQuoteRate);
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
          const fromCurrencyId = currencies.find((c: Currency) => c.Code === fromCurrency)?.Id
          const toCurrencyId = currencies.find((c: Currency) => c.Code === toCurrency)?.Id

          if (!fromCurrencyId || !toCurrencyId) {
            return
          }

          const response = await api.get(`/api/Currency/quote/${fromCurrencyId}/${toCurrencyId}`)
          let quoteRate = response.data?.rate || 0
          setRate(quoteRate)

          const amountValue = Number.parseFloat(amount)
          if (!isNaN(amountValue) && amountValue > 0) {
            
            if (fromCurrency === "BRL") {
              setTotalAmount(amountValue / quoteRate)
            } else {
              
              setTotalAmount(amountValue * quoteRate)
            }
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

  
  useEffect(() => {
    const updateBRLValues = async () => {
      if (!fromCurrency || !amount || !operation || !toCurrency || !brlRate) return;

      try {
        
        const toCurrencyCode = toCurrency.match(/\(([^)]+)\)/)?.[1];
        if (toCurrencyCode === "BRL") {
          setBrlAmount(Number.parseFloat(amount));
          setOldBrlAmount(operation.Amount);
          return;
        }

        
        const newBrlValue = Number.parseFloat(amount) / (brlRate || 1);
        const oldBrlValue = operation.Amount / brlRate ;

        console.log('Taxa BRL/Moeda destino:', brlRate);
        console.log('Nova operação em BRL:', newBrlValue);
        console.log('Operação antiga em BRL:', oldBrlValue);
        
        setBrlAmount(newBrlValue);
        setOldBrlAmount(oldBrlValue);
      } catch (error) {
        console.error("Erro ao atualizar valores em BRL:", error);
      }
    };

    updateBRLValues();
  }, [fromCurrency, toCurrency, amount, operation, brlRate]);

  const handleAmountChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value.replace(/[^0-9]/g, '');
    
    if (value === '') {
      setAmount('0.000');
      return;
    }

    const numericValue = parseInt(value, 10) / 1000;
    setAmount(numericValue.toFixed(3));
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!operation) {
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

    
    if (customerLimit) {
      const newAvailableLimit = customerLimit.Limit - (customerLimit.UsedLimit - oldBrlAmount + brlAmount)
      
      if (newAvailableLimit < 0) {
        toast({
          title: "Limite excedido",
          description: "O novo valor da operação excede o limite disponível do cliente.",
          variant: "destructive",
        })
        return
      }
    }

    setSubmitting(true)
    try {
      console.log('Operacao:', operation)
      await api.put(`/api/ExchangeOperation/${operation.Id}`, {
        CustomerId: operation.CustomerId,
        FromCurrencyCode: fromCurrency,
        ToCurrencyCode: toCurrency,
        Amount: amountValue,
        Status: 2 
      })

      toast({
        title: "Operação atualizada",
        description: "A operação foi atualizada com sucesso.",
      })

      router.push("/dashboard/operacoes")
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || "Ocorreu um erro ao atualizar a operação."
      toast({
        title: "Erro ao atualizar operação",
        description: errorMessage,
        variant: "destructive",
      })
    } finally {
      setSubmitting(false)
    }
  }

  const handleDelete = async () => {
    if (!operation) return;

    setSubmitting(true);
    try {
      await api.delete(`/api/ExchangeOperation/${operation.Id}`);

      toast({
        title: "Operação excluída",
        description: "A operação foi excluída com sucesso.",
      });

      router.push("/dashboard/operacoes");
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || "Ocorreu um erro ao excluir a operação.";
      toast({
        title: "Erro ao excluir operação",
        description: errorMessage,
        variant: "destructive",
      });
    } finally {
      setSubmitting(false);
    }
  };

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
        return "text-yellow-600 bg-yellow-100"
      case 1:
        return "text-green-600 bg-green-100"
      case 2:
        return "text-blue-600 bg-blue-100"
      case 3:
        return "text-red-600 bg-red-100"
      default:
        return "text-gray-600 bg-gray-100"
    }
  }

  if (loading || !operation) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-[300px]" />
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-[200px]" />
            <Skeleton className="h-4 w-[300px]" />
          </CardHeader>
          <CardContent className="space-y-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Editar Operação #{operation.Id}</h1>
        <div className="flex items-center gap-2">
          <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusClass(operation.Status)}`}>
            {getStatusText(operation.Status)}
          </span>
          <Button
            variant="destructive"
            onClick={handleDelete}
            disabled={operation.Status === 3 || submitting}
          >
            Excluir Operação
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Detalhes da Operação</CardTitle>
          <CardDescription>Edite os detalhes da operação de câmbio</CardDescription>
        </CardHeader>
        <form onSubmit={handleSubmit}>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label>Cliente</Label>
              <div className="rounded-lg border p-4">
                <div>
                  <div className="font-medium">{operation.CustomerName}</div>
                  <div className="text-sm text-muted-foreground">{operation.CustomerDocument}</div>
                </div>
              </div>
            </div>

            <div className="space-y-2">
              <Label>Valores Originais</Label>
              <div className="rounded-lg border p-4">
                <div className="space-y-2">
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-muted-foreground">Moeda de Origem:</span>
                    <span className="font-medium">{operation.FromCurrency}</span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-muted-foreground">Moeda de Destino:</span>
                    <span className="font-medium">{operation.ToCurrency}</span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-muted-foreground">Valor Original:</span>
                    <span className="font-medium">
                      {operation.FinalAmount.toLocaleString("pt-BR", {
                        minimumFractionDigits: 3,
                        maximumFractionDigits: 3
                      })} {operation.FromCurrency}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-muted-foreground">Taxa de Câmbio:</span>
                    <span className="font-medium">
                      1 {operation.FromCurrency} = {operation.ExchangeRate.toLocaleString("pt-BR", { minimumFractionDigits: 4, maximumFractionDigits: 4 })} {operation.ToCurrency}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-muted-foreground">Valor Final:</span>
                    <span className="font-medium">
                      {operation.Amount.toLocaleString("pt-BR", {
                        minimumFractionDigits: 3,
                        maximumFractionDigits: 3
                      })} {operation.ToCurrency}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="fromCurrency">Nova Moeda de Origem</Label>
                <Select value={fromCurrency} onValueChange={setFromCurrency}>
                  <SelectTrigger id="fromCurrency">
                    <SelectValue placeholder="Selecione a moeda" />
                  </SelectTrigger>
                  <SelectContent>
                    {currencies.map((currency) => (
                      <SelectItem 
                        key={currency.Id} 
                        value={currency.Code}
                        disabled={currency.Code === toCurrency}
                      >
                        {currency.Name} ({currency.Code})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="toCurrency">Nova Moeda de Destino</Label>
                <Select value={toCurrency} onValueChange={setToCurrency}>
                  <SelectTrigger id="toCurrency">
                    <SelectValue placeholder="Selecione a moeda" />
                  </SelectTrigger>
                  <SelectContent>
                    {currencies.map((currency) => (
                      <SelectItem 
                        key={currency.Id} 
                        value={currency.Code}
                        disabled={currency.Code === fromCurrency}
                      >
                        {currency.Name} ({currency.Code})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="amount">Novo Valor ({toCurrency || ''})</Label>
              <Input
                id="amount"
                type="text"
                inputMode="numeric"
                value={amount}
                onChange={handleAmountChange}
                placeholder={`0.000 ${toCurrency || ''}`}
              />
            </div>

            {customerLimit && (
              <div className="space-y-2">
                <Label>Limite do Cliente</Label>
                <div className="rounded-lg border p-4">
                  <div className="space-y-2">
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-muted-foreground">Limite Total:</span>
                      <span className="font-medium">
                        {customerLimit.Limit.toLocaleString("pt-BR", {
                          style: "currency",
                          currency: "BRL"
                        })}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-muted-foreground">Limite Utilizado:</span>
                      <span className="font-medium">
                        {(customerLimit.UsedLimit - oldBrlAmount + brlAmount).toLocaleString("pt-BR", {
                          style: "currency",
                          currency: "BRL",
                          minimumFractionDigits: 3,
                          maximumFractionDigits: 3
                        })}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-muted-foreground">Limite Disponível:</span>
                      <span className="font-medium">
                        {(customerLimit.Limit - (customerLimit.UsedLimit - oldBrlAmount + brlAmount)).toLocaleString("pt-BR", {
                          style: "currency",
                          currency: "BRL",
                          minimumFractionDigits: 3,
                          maximumFractionDigits: 3
                        })}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {rate !== null && totalAmount !== null && (
              <div className="rounded-lg border p-4">
                <div className="mb-2 text-sm font-medium text-muted-foreground">Nova Taxa de conversão:</div>
                <div className="mb-4 text-xl font-bold">
                  1 {fromCurrency} = {rate.toLocaleString("pt-BR", { minimumFractionDigits: 4, maximumFractionDigits: 4 })} {toCurrency}
                </div>
                <div className="flex items-center justify-between">
                  <div className="text-lg">
                    {(Number.parseFloat(amount) / rate).toLocaleString("pt-BR", { minimumFractionDigits: 3, maximumFractionDigits: 3 })} {fromCurrency}
                  </div>
                  <ArrowRight className="mx-2 h-4 w-4 text-muted-foreground" />
                  <div className="text-lg font-bold">
                    {Number.parseFloat(amount).toLocaleString("pt-BR", { minimumFractionDigits: 3, maximumFractionDigits: 3 })} {toCurrency}
                  </div>
                </div>
                <div className="mt-2 text-sm text-muted-foreground text-right">
                  Valor em BRL: {brlAmount.toLocaleString("pt-BR", {
                    style: "currency",
                    currency: "BRL",
                    minimumFractionDigits: 3,
                    maximumFractionDigits: 3
                  })}
                </div>
              </div>
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
              disabled={
                submitting || 
                !fromCurrency || 
                !toCurrency || 
                !amount || 
                !rate || 
                operation.Status === 3 || 
                (customerLimit && (
                  customerLimit.Limit - (customerLimit.UsedLimit - oldBrlAmount + brlAmount) < 0
                ))
              }
            >
              {submitting ? "Processando..." : "Salvar Alterações"}
            </Button>
          </CardFooter>
        </form>
      </Card>
    </div>
  )
} 