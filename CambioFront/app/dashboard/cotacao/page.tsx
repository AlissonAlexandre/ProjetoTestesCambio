"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { useToast } from "@/hooks/use-toast"
import { api } from "@/lib/api"
import { ArrowRight, RefreshCw } from "lucide-react"
import { Skeleton } from "@/components/ui/skeleton"

interface Currency {
  Id: number
  Code: string
  Name: string
}

export default function CotacaoPage() {
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [fromCurrency, setFromCurrency] = useState<string>("")
  const [toCurrency, setToCurrency] = useState<string>("BRL")
  const [amount, setAmount] = useState<string>("0.0000")
  const [rate, setRate] = useState<number | null>(null)
  const [result, setResult] = useState<number | null>(null)
  const [loading, setLoading] = useState(true)
  const [quoteLoading, setQuoteLoading] = useState(false)
  const { toast } = useToast()

  const fetchCurrencies = async () => {
    setLoading(true)
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

      
      if (uniqueCurrencies.length > 0) {
        const foreignCurrency = uniqueCurrencies.find((c: Currency) => c.Code !== "BRL")
        if (foreignCurrency) {
          setFromCurrency(foreignCurrency.Code)
        }
      }
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

  const getQuote = async () => {
    if (!fromCurrency || !toCurrency) {
      toast({
        title: "Erro de validação",
        description: "Selecione as moedas para cotação.",
        variant: "destructive",
      })
      return
    }

    setQuoteLoading(true)
    try {
      const fromCurrencyId = currencies.find((c: Currency) => c.Code === fromCurrency)?.Id
      const toCurrencyId = currencies.find((c: Currency) => c.Code === toCurrency)?.Id

      if (!fromCurrencyId || !toCurrencyId) {
        throw new Error("Moeda não encontrada")
      }

      const response = await api.get(`/api/Currency/quote/${toCurrencyId}/${fromCurrencyId}`)
      const quoteRate = response.data?.rate || 0
      setRate(quoteRate)

      const amountValue = Number.parseFloat(amount)
      if (!isNaN(amountValue) && amountValue > 0) {
        setResult(amountValue * quoteRate)
      }
    } catch (error) {
      toast({
        title: "Erro ao obter cotação",
        description: "Não foi possível obter a cotação para as moedas selecionadas.",
        variant: "destructive",
      })
    } finally {
      setQuoteLoading(false)
    }
  }

  const handleAmountChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value.replace(/[^0-9]/g, '');
    
    if (value === '') {
      setAmount('0.0000');
      return;
    }

    
    const numericValue = parseInt(value, 10) / 10000;
    setAmount(numericValue.toFixed(4));
  }

  useEffect(() => {
    fetchCurrencies()
  }, [])

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Cotação de Moedas</h1>
        <Button variant="outline" size="sm" onClick={fetchCurrencies}>
          <RefreshCw className="mr-2 h-4 w-4" />
          Atualizar Moedas
        </Button>
      </div>

      <Card className="max-w-2xl mx-auto">
        <CardHeader>
          <CardTitle>Converter Moedas</CardTitle>
          <CardDescription>Selecione as moedas e o valor para obter a cotação atual</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {loading ? (
            <div className="space-y-4">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="fromCurrency">De</Label>
                  <Select value={fromCurrency} onValueChange={setFromCurrency}>
                    <SelectTrigger id="fromCurrency">
                      <SelectValue placeholder="Selecione a moeda" />
                    </SelectTrigger>
                    <SelectContent>
                      {currencies && currencies.length > 0 ? (
                        currencies
                          .filter((currency) => currency.Code !== toCurrency)
                          .map((currency) => (
                            <SelectItem key={currency.Id} value={currency.Code}>
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
                  <Label htmlFor="toCurrency">Para</Label>
                  <Select value={toCurrency} onValueChange={setToCurrency}>
                    <SelectTrigger id="toCurrency">
                      <SelectValue placeholder="Selecione a moeda" />
                    </SelectTrigger>
                    <SelectContent>
                      {currencies && currencies.length > 0 ? (
                        currencies
                          .filter((currency) => currency.Code !== fromCurrency)
                          .map((currency) => (
                            <SelectItem key={currency.Id} value={currency.Code}>
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
                <Label htmlFor="amount">Valor</Label>
                <Input
                  id="amount"
                  type="text"
                  inputMode="numeric"
                  value={amount}
                  onChange={handleAmountChange}
                  placeholder="0.0000"
                />
              </div>

              <Button className="w-full" onClick={getQuote} disabled={quoteLoading || !fromCurrency || !toCurrency}>
                {quoteLoading ? "Calculando..." : "Calcular"}
              </Button>

              {rate !== null && result !== null && (
                <div className="mt-6 rounded-lg border p-4">
                  <div className="mb-2 text-sm font-medium text-muted-foreground">Taxa de conversão:</div>
                  <div className="mb-4 text-xl font-bold">
                    1 {fromCurrency} = {rate.toFixed(4)} {toCurrency}
                  </div>
                  <div className="flex items-center justify-between">
                    <div className="text-lg">
                      {Number.parseFloat(amount).toFixed(4)} {fromCurrency}
                    </div>
                    <ArrowRight className="mx-2 h-4 w-4 text-muted-foreground" />
                    <div className="text-lg font-bold">
                      {result.toFixed(4)} {toCurrency}
                    </div>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
