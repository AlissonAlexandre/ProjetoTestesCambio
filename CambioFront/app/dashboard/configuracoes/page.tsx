"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useToast } from "@/hooks/use-toast"
import { api } from "@/lib/api"
import { useAuth } from "@/contexts/auth-context"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Skeleton } from "@/components/ui/skeleton"
import { useRouter } from "next/navigation"

interface UserProfile {
  id: number
  name: string
  email: string
  IsMaster: boolean
  createdAt: string
}

export default function ConfiguracoesPage() {
  const [saving, setSaving] = useState(false)
  const [currentPassword, setCurrentPassword] = useState("")
  const [newPassword, setNewPassword] = useState("")
  const [confirmPassword, setConfirmPassword] = useState("")
  const [formData, setFormData] = useState<{ name: string; email: string }>({ name: "", email: "" })
  const { toast } = useToast()
  const { user, loading, updateUser, updateToken } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (!loading && !user) {
      router.replace("/login")
    }
    if (user) {
      setFormData({ name: user.name, email: user.email })
    }
  }, [user, loading])

  const validateProfileData = () => {
    if (formData.name && formData.name.length > 80) {
      toast({
        title: "Nome inválido",
        description: "O nome deve ter no máximo 80 caracteres.",
        variant: "destructive",
      })
      return false
    }

    if (formData.email) {
      if (formData.email.length > 50) {
        toast({
          title: "Email inválido",
          description: "O email deve ter no máximo 50 caracteres.",
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
    }

    return true
  }

  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!user) return

    
    if (formData.name === user.name && formData.email === user.email) {
      toast({
        title: "Nenhuma alteração",
        description: "Não houve alteração nos dados do perfil.",
      })
      return
    }

    if (!validateProfileData()) {
      return
    }

    setSaving(true)
    
    const payload: { name?: string; email?: string } = {}
    if (formData.name !== user.name) payload.name = formData.name
    if (formData.email !== user.email) payload.email = formData.email

    try {
      
      const token = localStorage.getItem("token")
      if (!token) {
        throw new Error("No token found")
      }

      
      const response = await api.put("/api/User/profile", payload, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      })
      
      if (response.data) {
        updateUser(response.data)
        toast({
          title: "Perfil atualizado",
          description: "Seus dados foram atualizados com sucesso.",
        })
      }
    } catch (error: any) {
      console.error("Error updating profile:", error)
      const errorMessage = error.response?.data?.message || "Não foi possível atualizar seus dados."
      
      
      if (error.response?.status === 405) {
        console.error("Método HTTP incorreto - tentando com POST")
        try {
          const response = await api.post("/api/User/profile", payload)
          if (response.data) {
            updateUser(response.data)
            toast({
              title: "Perfil atualizado",
              description: "Seus dados foram atualizados com sucesso.",
            })
            return
          }
        } catch (retryError: any) {
          console.error("Erro na segunda tentativa:", retryError)
        }
      }

      toast({
        title: "Erro ao atualizar perfil",
        description: errorMessage,
        variant: "destructive",
      })
    } finally {
      setSaving(false)
    }
  }

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!user) return
    
    if (newPassword.length < 6) {
      toast({
        title: "Senha inválida",
        description: "A senha deve ter no mínimo 6 caracteres.",
        variant: "destructive",
      })
      return
    }

    if (newPassword.length > 100) {
      toast({
        title: "Senha inválida",
        description: "A senha deve ter no máximo 100 caracteres.",
        variant: "destructive",
      })
      return
    }

    if (newPassword !== confirmPassword) {
      toast({
        title: "Senhas diferentes",
        description: "A confirmação da senha não coincide com a nova senha.",
        variant: "destructive",
      })
      return
    }

    setSaving(true)
    try {
      
      const token = localStorage.getItem("token")
      if (!token) {
        throw new Error("No token found")
      }

      const response = await api.put("/api/User/password", {
        currentPassword,
        newPassword,
        confirmNewPassword: confirmPassword
      }, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      })
      
      
      setCurrentPassword("")
      setNewPassword("")
      setConfirmPassword("")
      
      
      if (response.data?.token) {
        updateToken(response.data.token)
      }
      
      toast({
        title: "Senha atualizada",
        description: "Sua senha foi alterada com sucesso.",
      })
    } catch (error: any) {
      console.error("Error changing password:", error)
      const errorMessage = error.response?.data?.message || "Não foi possível alterar sua senha. Verifique se a senha atual está correta."
      toast({
        title: "Erro ao alterar senha",
        description: errorMessage,
        variant: "destructive",
      })
    } finally {
      setSaving(false)
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

  if (!user) {
    return null
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Configurações</h1>
      </div>

      <Tabs defaultValue="profile" className="space-y-4">
        <TabsList>
          <TabsTrigger value="profile">Perfil</TabsTrigger>
          <TabsTrigger value="password">Alterar Senha</TabsTrigger>
        </TabsList>

        <TabsContent value="profile">
          <Card>
            <CardHeader>
              <CardTitle>Perfil do Usuário</CardTitle>
              <CardDescription>Atualize suas informações pessoais</CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleUpdateProfile} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="name">Nome</Label>
                  <Input
                    id="name"
                    value={formData.name}
                    onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                    maxLength={80}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="email">E-mail</Label>
                  <Input
                    id="email"
                    type="email"
                    value={formData.email}
                    onChange={(e) => setFormData(prev => ({ ...prev, email: e.target.value }))}
                    maxLength={50}
                  />
                </div>
                <Button type="submit" disabled={saving}>
                  {saving ? "Salvando..." : "Salvar Alterações"}
                </Button>
              </form>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="password">
          <Card>
            <CardHeader>
              <CardTitle>Alterar Senha</CardTitle>
              <CardDescription>Atualize sua senha de acesso</CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleChangePassword} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="currentPassword">Senha Atual</Label>
                  <Input
                    id="currentPassword"
                    type="password"
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.target.value)}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="newPassword">Nova Senha</Label>
                  <Input
                    id="newPassword"
                    type="password"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="confirmPassword">Confirmar Nova Senha</Label>
                  <Input
                    id="confirmPassword"
                    type="password"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                  />
                </div>
                <Button type="submit" disabled={saving}>
                  {saving ? "Alterando..." : "Alterar Senha"}
                </Button>
              </form>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
} 