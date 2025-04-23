"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useToast } from "@/hooks/use-toast"
import { api } from "@/lib/api"
import { Skeleton } from "@/components/ui/skeleton"
import { useAuth } from "@/contexts/auth-context"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"

interface Customer {
  id: number
  name: string
  document: string
  phone: string
  email: string
}

export default function EditarClientePage({ params }: { params: { id: string } }) {
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [deleting, setDeleting] = useState(false)
  const [showDeleteDialog, setShowDeleteDialog] = useState(false)
  const [customer, setCustomer] = useState<Customer | null>(null)
  const [formData, setFormData] = useState<Partial<Customer>>({})
  const { toast } = useToast()
  const router = useRouter()
  const { user } = useAuth()

  useEffect(() => {
    if (!user) {
      router.replace("/login")
      return
    }

    const fetchCustomer = async () => {
      try {
        const response = await api.get(`/api/Customer/${params.id}`)
        if (response.data) {
          setCustomer(response.data)
          setFormData(response.data)
        }
      } catch (error) {
        toast({
          title: "Erro ao carregar cliente",
          description: "Não foi possível carregar os dados do cliente.",
          variant: "destructive",
        })
        router.push("/dashboard/clientes")
      } finally {
        setLoading(false)
      }
    }

    fetchCustomer()
  }, [params.id, router, toast, user])

  const validateFormData = () => {
    if (!formData.name || formData.name.trim().length === 0) {
      toast({
        title: "Nome inválido",
        description: "O nome é obrigatório.",
        variant: "destructive",
      })
      return false
    }

    if (!formData.document || formData.document.trim().length === 0) {
      toast({
        title: "Documento inválido",
        description: "O documento é obrigatório.",
        variant: "destructive",
      })
      return false
    }

    if (!formData.email || formData.email.trim().length === 0) {
      toast({
        title: "Email inválido",
        description: "O email é obrigatório.",
        variant: "destructive",
      })
      return false
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailRegex.test(formData.email)) {
      toast({
        title: "Email inválido",
        description: "Por favor, insira um email válido.",
        variant: "destructive",
      })
      return false
    }

    return true
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!customer) return

    if (!validateFormData()) {
      return
    }

    setSaving(true)
    try {
      const payload = {
        name: formData.name,
        document: formData.document,
        phone: formData.phone,
        email: formData.email
      }

      const response = await api.put(`/api/Customer/${params.id}`, payload)
      if (response.data) {
        toast({
          title: "Cliente atualizado",
          description: "Os dados do cliente foram atualizados com sucesso.",
        })
        router.push("/dashboard/clientes")
      }
    } catch (error: any) {
      console.error("Error updating customer:", error)
      const errorMessage = error.response?.data?.message || "Não foi possível atualizar os dados do cliente."
      toast({
        title: "Erro ao atualizar cliente",
        description: errorMessage,
        variant: "destructive",
      })
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async () => {
    setDeleting(true)
    try {
      await api.delete(`/api/Customer/${params.id}`)
      toast({
        title: "Cliente excluído",
        description: "O cliente foi excluído com sucesso.",
      })
      router.push("/dashboard/clientes")
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || "Não foi possível excluir o cliente."
      toast({
        title: "Erro ao excluir cliente",
        description: errorMessage,
        variant: "destructive",
      })
    } finally {
      setDeleting(false)
      setShowDeleteDialog(false)
    }
  }

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-48" />
        </div>
        <div className="space-y-4">
          <Skeleton className="h-[200px] w-full" />
        </div>
      </div>
    )
  }

  if (!customer) {
    return null
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Editar Cliente</h1>
        <div className="flex space-x-2">
          <Button variant="outline" onClick={() => router.push("/dashboard/clientes")}>
            Voltar
          </Button>
          <Button 
            variant="destructive" 
            onClick={() => setShowDeleteDialog(true)}
            disabled={deleting}
          >
            {deleting ? "Excluindo..." : "Excluir Cliente"}
          </Button>
        </div>
      </div>

      <Dialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirmar Exclusão</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja excluir este cliente? Esta ação não pode ser desfeita.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setShowDeleteDialog(false)}
              disabled={deleting}
            >
              Cancelar
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={deleting}
            >
              {deleting ? "Excluindo..." : "Confirmar Exclusão"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Card>
        <CardHeader>
          <CardTitle>Dados do Cliente</CardTitle>
          <CardDescription>Atualize as informações do cliente</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Nome</Label>
              <Input
                id="name"
                value={formData.name || ""}
                onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                maxLength={80}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="document">Documento</Label>
              <Input
                id="document"
                value={formData.document || ""}
                onChange={(e) => setFormData(prev => ({ ...prev, document: e.target.value }))}
                maxLength={20}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="phone">Telefone</Label>
              <Input
                id="phone"
                value={formData.phone || ""}
                onChange={(e) => setFormData(prev => ({ ...prev, phone: e.target.value }))}
                maxLength={20}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">E-mail</Label>
              <Input
                id="email"
                type="email"
                value={formData.email || ""}
                onChange={(e) => setFormData(prev => ({ ...prev, email: e.target.value }))}
                maxLength={50}
              />
            </div>
            <div className="flex justify-end space-x-2">
              <Button type="button" variant="outline" onClick={() => router.push("/dashboard/clientes")}>
                Cancelar
              </Button>
              <Button type="submit" disabled={saving}>
                {saving ? "Salvando..." : "Salvar Alterações"}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
} 