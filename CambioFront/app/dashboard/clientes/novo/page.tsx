"use client"

import type React from "react"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useToast } from "@/hooks/use-toast"
import { api } from "@/lib/api"
import { useAuth } from "@/contexts/auth-context"
import { validateCPF, validateCNPJ, formatDocument } from "@/lib/document-utils"

export default function NovoClientePage() {
  const [name, setName] = useState("")
  const [document, setDocument] = useState("")
  const [phone, setPhone] = useState("")
  const [email, setEmail] = useState("")
  const [loading, setLoading] = useState(false)
  const { toast } = useToast()
  const router = useRouter()
  const { user } = useAuth()

  
  if (user && !user.IsMaster) {
    router.push("/dashboard/clientes")
    return null
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    
    const cleanDocument = document.replace(/\D/g, "")
    if (cleanDocument.length === 11) {
      if (!validateCPF(cleanDocument)) {
        toast({
          title: "CPF inválido",
          description: "Por favor, insira um CPF válido.",
          variant: "destructive",
        })
        return
      }
    } else if (cleanDocument.length === 14) {
      if (!validateCNPJ(cleanDocument)) {
        toast({
          title: "CNPJ inválido",
          description: "Por favor, insira um CNPJ válido.",
          variant: "destructive",
        })
        return
      }
    } else {
      toast({
        title: "Documento inválido",
        description: "Por favor, insira um CPF ou CNPJ válido.",
        variant: "destructive",
      })
      return
    }

    setLoading(true)

    try {
      await api.post("/api/Customer", {
        name,
        document: cleanDocument,
        phone: phone.replace(/\D/g, ""),
        email,
      })

      toast({
        title: "Cliente cadastrado com sucesso",
        description: "O cliente foi cadastrado com sucesso no sistema.",
      })

      router.push("/dashboard/clientes")
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || "Ocorreu um erro ao cadastrar o cliente."
      toast({
        title: "Erro ao cadastrar cliente",
        description: errorMessage,
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  const handleDocumentChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value
    const formattedValue = formatDocument(value)
    setDocument(formattedValue)
  }

  const handlePhoneChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value
    const digits = value.replace(/\D/g, "")
    let formattedValue = ""

    if (digits.length <= 2) {
      formattedValue = digits
    } else if (digits.length <= 6) {
      formattedValue = `(${digits.slice(0, 2)}) ${digits.slice(2)}`
    } else if (digits.length <= 10) {
      formattedValue = `(${digits.slice(0, 2)}) ${digits.slice(2, 6)}-${digits.slice(6)}`
    } else {
      formattedValue = `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7, 11)}`
    }

    setPhone(formattedValue)
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Novo Cliente</h1>
      </div>

      <Card className="max-w-2xl mx-auto">
        <CardHeader>
          <CardTitle>Cadastrar Cliente</CardTitle>
          <CardDescription>Preencha os dados do cliente para cadastrá-lo no sistema</CardDescription>
        </CardHeader>
        <form onSubmit={handleSubmit}>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Nome</Label>
              <Input
                id="name"
                placeholder="Nome completo"
                value={name}
                onChange={(e) => setName(e.target.value)}
                maxLength={80}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="document">CPF ou CNPJ</Label>
              <Input
                id="document"
                placeholder="CPF ou CNPJ"
                value={document}
                onChange={handleDocumentChange}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="phone">Telefone</Label>
              <Input id="phone" placeholder="(00) 00000-0000" value={phone} onChange={handlePhoneChange} required />
            </div>

            <div className="space-y-2">
              <Label htmlFor="email">E-mail</Label>
              <Input
                id="email"
                type="email"
                placeholder="email@exemplo.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                maxLength={50}
                required
              />
            </div>
          </CardContent>
          <CardFooter>
            <Button type="button" variant="outline" onClick={() => router.push("/dashboard/clientes")} className="mr-2">
              Cancelar
            </Button>
            <Button type="submit" disabled={loading}>
              {loading ? "Cadastrando..." : "Cadastrar Cliente"}
            </Button>
          </CardFooter>
        </form>
      </Card>
    </div>
  )
}
